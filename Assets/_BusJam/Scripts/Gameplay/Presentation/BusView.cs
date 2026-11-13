using System.Collections.Generic;
using BusJam.Core;
using UnityEngine;

namespace BusJam.Gameplay.Presentation
{
    // Tek bir otobüsün görüntüsü. Sıranın verdiği yere sürer yolcularını sayar
    // Otobüs hep +X yönüne ekranda soldan sağa gider.
    public sealed class BusView : MonoBehaviour
    {
        private const float ArriveDistanceSqr = 0.0001f;

        [SerializeField] private PassengerColor color;
        [SerializeField] private Transform[] seats;
        [SerializeField] private float driveSpeed = 10f;
        [SerializeField] private float exitDistance = 25f;

        private Vector3 _target;
        private bool _targetIsStop;
        private int _reserved;
        private int _seated;
        private bool _departureRequested;

        public PassengerColor Color => color;
        public int SeatCount => seats.Length;
        public bool IsLeaving { get; private set; }
        public bool IsAtStop => _targetIsStop && !IsLeaving && IsAtTarget;

        private bool IsAtTarget => (transform.position - _target).sqrMagnitude < ArriveDistanceSqr;

        public Transform GetSeat(int index) => seats[index];

        public void Place(Vector3 position, bool isStop)
        {
            transform.position = position;
            DriveTo(position, isStop);
        }

        public void DriveTo(Vector3 position, bool isStop)
        {
            _target = position;
            _targetIsStop = isStop;
        }

        public void ReserveSeat() => _reserved++;
        public void NotifySeated() => _seated++;
        public void RequestDeparture() => _departureRequested = true;

        private void Update()
        {
            // Hakem otobüsü hemen gönderir ama son yolcu hâlâ yürüyor olabilir
            // O yüzden otobüs rezerve edilen herkes oturana kadar bekliyor
            if (_departureRequested && !IsLeaving && IsAtStop && _seated == _reserved)
            {
                IsLeaving = true;
                _target = transform.position + Vector3.right * exitDistance;
            }

            transform.position = Vector3.MoveTowards(transform.position, _target, driveSpeed * Time.deltaTime);

            // Ekrandan çıkınca yok etmiyoruz, sadece gizliyoruz oyun sırasında Destroy yok
            if (IsLeaving && IsAtTarget)
                gameObject.SetActive(false);
        }

        // Bileşen eklendiği an çalışır modeldeki SeatAnchor_0, _1, _2'yi sırayla bulup koltuklara koyar
        private void Reset()
        {
            var found = new List<Transform>();
            foreach (Transform t in GetComponentsInChildren<Transform>(true))
            {
                if (t.name.StartsWith("SeatAnchor_"))
                    found.Add(t);
            }
            found.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            seats = found.ToArray();
        }
    }
}