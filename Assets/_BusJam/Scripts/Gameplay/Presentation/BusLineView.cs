using BusJam.Core;
using BusJam.Gameplay.Domain;
using UnityEngine;

namespace BusJam.Gameplay.Presentation
{
    
    public sealed class BusLineView : MonoBehaviour
    {
        
        private const int ColorSlotCount = (int)PassengerColor.Purple + 1;

        [SerializeField] private BusView[] prefabs;

        
        [SerializeField] private float queueSpacing = 3f;

        private readonly BusView[] _prefabByColor = new BusView[ColorSlotCount];
        private BusView[] _buses;

        private void Awake()
        {
            foreach (BusView prefab in prefabs)
                _prefabByColor[(int)prefab.Color] = prefab;
        }

        public void Build(BusQueue queue, BoardLayout layout)
        {
            Clear();
            _buses = new BusView[queue.Count];

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
                int placeInLine = i - queue.CurrentIndex;
                bus.Place(layout.BusStopPosition + Vector3.left * (queueSpacing * placeInLine));
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
    }
}