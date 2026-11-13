using System.Collections.Generic;
using BusJam.Core;
using UnityEngine;

namespace BusJam.Gameplay.Presentation
{
    
    public sealed class PassengerView : MonoBehaviour
    {
        // Animator'a string yerine sayı ile soruyoruz dönüşümü bir kere en başta yapıyoruz.
        private static readonly int IsWalkingId = Animator.StringToHash("IsWalking");
        private static readonly int JumpId = Animator.StringToHash("Jump");

        private const float ArriveDistanceSqr = 0.0001f;
        private const float ShakeDuration = 0.35f;
        private const float ShakeAmount = 0.08f;
        private const float ShakeFrequency = 50f;

        [SerializeField] private PassengerColor color;
        [SerializeField] private Animator animator;
        [SerializeField] private float walkSpeed = 4f;
        [SerializeField] private float turnSpeed = 720f;

        // Liste bir kere oluşuyor, bitince temizleniyor her yürüyüşte yeni liste açıp çöp üretmiyoruz.
        private readonly List<Vector3> _points = new List<Vector3>(16);
        private int _nextPoint;
        private BusView _bus;
        private int _seat;

        private Vector3 _restFacing = Vector3.forward;
        private bool _isWalking;
        private float _shakeTimeLeft;

        public PassengerColor Color => color;

        public void Place(Vector3 position, Vector3 facing)
        {
            transform.SetPositionAndRotation(position, Quaternion.LookRotation(facing));
            _restFacing = facing;
        }

        public void WalkTo(Vector3 point) => _points.Add(point);

        public void SetRestFacing(Vector3 facing) => _restFacing = facing;

        // Elindeki plan bitince bu koltuğa yürüyüp oturacak.
        public void BoardBus(BusView bus, int seat)
        {
            _bus = bus;
            _seat = seat;
            bus.ReserveSeat();
        }

        public void Shake() => _shakeTimeLeft = ShakeDuration;

        public void PlayJump() => animator.SetTrigger(JumpId);

        // Havuza geri dönerken her şeyi sıfırlıyoruz ki bir sonraki kullanımda temiz başlasın.
        public void ResetForReuse()
        {
            _points.Clear();
            _nextPoint = 0;
            _bus = null;
            _restFacing = Vector3.forward;
            _shakeTimeLeft = 0f;
            animator.transform.localPosition = Vector3.zero;
            SetWalking(false);
            animator.ResetTrigger(JumpId);
        }

        private void Update()
        {
            if (_nextPoint < _points.Count)
                StepTowardPoint();
            else if (_bus != null)
                StepTowardSeat();
            else
                Rest();

            UpdateShake();
        }

        private void StepTowardPoint()
        {
            if (!MoveTowards(_points[_nextPoint]))
                return;

            _nextPoint++;
            if (_nextPoint == _points.Count)
            {
                _points.Clear();
                _nextPoint = 0;
            }
        }

        private void StepTowardSeat()
        {
            // Slotta bekleyen yolcu, otobüsü durağa varmadan yürümeye başlamasın
            if (!_bus.IsAtStop)
            {
                Rest();
                return;
            }

            Transform seat = _bus.GetSeat(_seat);
            if (!MoveTowards(seat.position))
                return;

            // Koltuğa oturunca otobüsün çocuğu oluyor otobüs gidince yolcu da onunla gidiyor
            // Yüzü otobüsün gittiği yöne dönük, kameradan yandan görünüyor
            transform.SetParent(seat, true);
            transform.SetPositionAndRotation(seat.position, Quaternion.LookRotation(Vector3.right));
            _restFacing = Vector3.right;
            SetWalking(false);
            _bus.NotifySeated();
            _bus = null;
        }

        private void Rest()
        {
            SetWalking(false);
            // Otobüste oturan yolcu dinlenirken yola doğru dönmeye çalışmasın
            if (transform.parent == null || transform.parent.GetComponentInParent<BusView>() == null)
                Turn(_restFacing);
        }

        // Hedefe doğru bir kare yürür vardıysa true döner.
        private bool MoveTowards(Vector3 target)
        {
            Vector3 position = transform.position;
            Vector3 flat = target - position;
            flat.y = 0f;
            if (flat.sqrMagnitude > ArriveDistanceSqr)
                Turn(flat);

            transform.position = Vector3.MoveTowards(position, target, walkSpeed * Time.deltaTime);
            SetWalking(true);
            return (transform.position - target).sqrMagnitude < ArriveDistanceSqr;
        }

        private void Turn(Vector3 direction)
        {
            Quaternion goal = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, goal, turnSpeed * Time.deltaTime);
        }

        // Animator'a sadece değer değişince yazıyoruz her kare aynı şeyi göndermek boşa iş demek
        private void SetWalking(bool walking)
        {
            if (_isWalking == walking)
                return;

            _isWalking = walking;
            animator.SetBool(IsWalkingId, walking);
        }

        // Önü kapalı yolcu titriyor. Sadece modeli oynatıyoruz hücredeki yeri bozulmasın diye.
        private void UpdateShake()
        {
            if (_shakeTimeLeft <= 0f)
                return;

            _shakeTimeLeft -= Time.deltaTime;
            float strength = Mathf.Max(0f, _shakeTimeLeft) / ShakeDuration;
            float x = Mathf.Sin(_shakeTimeLeft * ShakeFrequency) * ShakeAmount * strength;
            animator.transform.localPosition = new Vector3(x, 0f, 0f);
        }

        // Bileşen eklendiği an Unity bunu çağırır Animator alanını elle sürüklemekten kurtarıyor.
        private void Reset()
        {
            animator = GetComponentInChildren<Animator>();
        }
    }
}