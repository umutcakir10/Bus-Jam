using BusJam.Core;
using BusJam.Gameplay.Domain;
using UnityEngine;

namespace BusJam.Gameplay.Presentation
{
    // Yoldaki otobüsler öndeki durakta, diğerleri onun solunda sırada.
    // Otobüsler level başında bir kere üretiliyor ve oyun boyunca yok edilmiyor o yüzden havuza gerek yok.
    public sealed class BusLineView : MonoBehaviour
    {
        // Renk sayısıyla indeksleniyor 0 (None) boş kalıyor.
        private const int ColorSlotCount = (int)PassengerColor.Purple + 1;

        [SerializeField] private BusView[] prefabs;
        [SerializeField] private float queueSpacing = 5f;

        private readonly BusView[] _prefabByColor = new BusView[ColorSlotCount];
        private BusView[] _buses;
        private Vector3 _stop;
        private int _front;

        private void Awake()
        {
            foreach (BusView prefab in prefabs)
                _prefabByColor[(int)prefab.Color] = prefab;
        }

        public void Build(BusQueue queue, BoardLayout layout)
        {
            Clear();
            _buses = new BusView[queue.Count];
            _stop = layout.BusStopPosition;
            _front = queue.CurrentIndex;

            for (int i = 0; i < queue.Count; i++)
            {
                PassengerColor color = queue.GetColor(i);
                BusView prefab = _prefabByColor[(int)color];
                if (prefab == null)
                {
                    Debug.LogError($"No bus prefab for {color}.", this);
                    continue;
                }

                BusView bus = Instantiate(prefab, transform);
                bus.Place(LinePosition(i), i == _front);
                _buses[i] = bus;
            }
        }

        public BusView GetBus(int busIndex) => _buses[busIndex];

        public void Clear()
        {
            if (_buses == null)
                return;

            foreach (BusView bus in _buses)
            {
                if (bus != null)
                    Destroy(bus.gameObject);
            }
            _buses = null;
        }

        private void Update()
        {
            if (_buses == null || _front >= _buses.Length)
                return;

            BusView front = _buses[_front];
            if (front == null || !front.IsLeaving)
                return;

            // Öndeki otobüs gerçekten yola çıkınca sıra bir ilerliyor, arkadakiler yeni yerlerine sürüyor
            _front++;
            for (int i = _front; i < _buses.Length; i++)
            {
                if (_buses[i] != null)
                    _buses[i].DriveTo(LinePosition(i), i == _front);
            }
        }

        // Duraktaki otobüs 0, sıradaki 1... Her biri duraktan o kadar aralık solda duruyor
        private Vector3 LinePosition(int busIndex)
        {
            return _stop + Vector3.left * (queueSpacing * (busIndex - _front));
        }
    }
}