using System.Collections.Generic;
using BusJam.Core;
using DG.Tweening;
using UnityEngine;
using BusJam.Audio;

namespace BusJam.Gameplay.Presentation
{
    
    // Sıranın verdiği yere sürer, yolcularını sayar, herkes oturunca yola çıkar
    // Otobüs hep +X yönüne, ekranda soldan sağa gider
    public sealed class BusView : MonoBehaviour
    {
        [SerializeField] private PassengerColor color;
        [SerializeField] private Transform[] seats;
        [SerializeField] private float driveSpeed = 10f;
        [SerializeField] private float exitDistance = 25f;

        private Tween _move;
        private Tween _punch;
        private bool _targetIsStop;
        private int _reserved;
        private int _seated;
        private bool _departureRequested;

        public PassengerColor Color => color;
        public int SeatCount => seats.Length;
        public bool IsLeaving { get; private set; }

        // Durağa giden hareket bittiyse otobüs durakta bekliyor demektir
        public bool IsAtStop => _targetIsStop && !IsLeaving && (_move == null || !_move.IsActive());

        public Transform GetSeat(int index) => seats[index];

        public void Place(Vector3 position, bool isStop)
        {
            _move?.Kill();
            transform.position = position;
            _targetIsStop = isStop;
        }

        // Süreyi mesafeden hesaplıyoruz, uzaktaki otobüs daha uzun sürede gelsin
        // OutCubic ile hızlı başlayıp yavaşlayarak duruyor, gerçek bir otobüs gibi
        public void DriveTo(Vector3 position, bool isStop)
        {
            _move?.Kill();
            _targetIsStop = isStop;
            float duration = Vector3.Distance(transform.position, position) / driveSpeed * 1.6f;
            _move = transform.DOMove(position, duration).SetEase(Ease.OutCubic).SetLink(gameObject);
        }

        public void ReserveSeat() => _reserved++;

        // Her yolcu oturunca otobüs hafifçe esniyor, binişin hissedilmesi için
        public void NotifySeated()
        {
            _seated++;
            _punch?.Complete();
            _punch = transform.DOPunchScale(new Vector3(0.06f, -0.08f, 0.06f), 0.25f, 6, 0.5f).SetLink(gameObject);
            SoundPlayer.Play(Sfx.Seat);
        }

        public void RequestDeparture() => _departureRequested = true;

        private void Update()
        {
            
            // O yüzden otobüs, rezerve edilen herkes oturana kadar bekliyor
            if (_departureRequested && !IsLeaving && IsAtStop && _seated == _reserved)
                Leave();
        }

        // Yavaş kalkıp hızlanarak gidiyor, ekrandan çıkınca yok etmiyoruz, gizliyoruz
        private void Leave()
        {
            SoundPlayer.Play(Sfx.BusLeave);
            IsLeaving = true;
            _move?.Kill();
            _move = transform.DOMoveX(transform.position.x + exitDistance, exitDistance / driveSpeed * 0.8f)
                .SetEase(Ease.InQuad)
                .SetLink(gameObject)
                .OnComplete(() => gameObject.SetActive(false));
        }

        // Bileşen eklendiği an çalışır, modeldeki SeatAnchor_0, _1, _2'yi sırayla koltuklara koyar
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