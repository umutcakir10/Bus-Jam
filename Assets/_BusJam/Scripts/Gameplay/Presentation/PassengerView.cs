using System.Collections.Generic;
using BusJam.Core;
using DG.Tweening;
using UnityEngine;

namespace BusJam.Gameplay.Presentation
{
    // Tek bir yolcunun görüntüsü, kural bilmez, hakem ne dediyse onu oynatır
    // Bir yürüme planı var: sırayla gidilecek noktalar, en sonda da varsa bir otobüs koltuğu
    public sealed class PassengerView : MonoBehaviour
    {
        // Animator'a string yerine sayı ile soruyoruz, dönüşümü bir kere en başta yapıyoruz
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

        // Otobüsün önünde, koltuğa zıplamadan önce durduğu noktanın otobüs ortasına uzaklığı
        [SerializeField] private float boardingOffset = 1.3f;
        [SerializeField] private float jumpPower = 1.2f;
        [SerializeField] private float jumpDuration = 0.4f;

        // Liste bir kere oluşuyor, bitince temizleniyor, her yürüyüşte yeni liste açıp çöp üretmiyoruz
        private readonly List<Vector3> _points = new List<Vector3>(16);
        private int _nextPoint;
        private BusView _bus;
        private int _seat;

        private Vector3 _restFacing = Vector3.forward;
        private bool _isWalking;
        private bool _jumping;
        private float _shakeTimeLeft;

        public PassengerColor Color => color;

        public void Place(Vector3 position, Vector3 facing)
        {
            transform.SetPositionAndRotation(position, Quaternion.LookRotation(facing));
            _restFacing = facing;
        }

        public void WalkTo(Vector3 point) => _points.Add(point);

        public void SetRestFacing(Vector3 facing) => _restFacing = facing;

        // Elindeki plan bitince bu koltuğa gidip oturacak
        public void BoardBus(BusView bus, int seat)
        {
            _bus = bus;
            _seat = seat;
            bus.ReserveSeat();
        }

        public void Shake() => _shakeTimeLeft = ShakeDuration;

        public void PlayJump() => animator.SetTrigger(JumpId);

        // Havuza geri dönerken her şeyi sıfırlıyoruz ki bir sonraki kullanımda temiz başlasın
        public void ResetForReuse()
        {
            transform.DOKill();
            _points.Clear();
            _nextPoint = 0;
            _bus = null;
            _jumping = false;
            _restFacing = Vector3.forward;
            _shakeTimeLeft = 0f;
            animator.transform.localPosition = Vector3.zero;
            SetWalking(false);
            animator.ResetTrigger(JumpId);
        }

        private void Update()
        {
            // Zıplarken hareketi DOTween yönetiyor, araya girmiyoruz
            if (_jumping)
                return;

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

            // Koltuk otobüsün içinde ve yüksekte, oraya yürürse havada süzülüyormuş gibi görünüyor
            // O yüzden yerde, otobüsün önünde koltuğun hizasına kadar yürüyüp oradan zıplıyor
            Transform seat = _bus.GetSeat(_seat);
            Vector3 door = new Vector3(seat.position.x, 0f, seat.position.z - boardingOffset);
            if (!MoveTowards(door))
                return;

            JumpIntoSeat(seat);
        }

        private void JumpIntoSeat(Transform seat)
        {
            BusView bus = _bus;
            _bus = null;
            _jumping = true;
            SetWalking(false);
            animator.SetTrigger(JumpId);
            transform.rotation = Quaternion.LookRotation(Vector3.forward);

            // Yere indiğinde koltuğun çocuğu oluyor, otobüs giderken yolcu da onunla gidiyor
            // Otobüs yolcunun gerçekten oturduğunu ancak burada öğreniyor, ondan önce yola çıkmıyor
            transform.DOJump(seat.position, jumpPower, 1, jumpDuration)
                .SetEase(Ease.Linear)
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    transform.SetParent(seat, true);
                    transform.SetPositionAndRotation(seat.position, Quaternion.LookRotation(Vector3.right));
                    _restFacing = Vector3.right;
                    _jumping = false;
                    bus.NotifySeated();
                });
        }

        private void Rest()
        {
            SetWalking(false);
            // Otobüste oturan yolcu dinlenirken yola doğru dönmeye çalışmasın
            if (transform.parent == null || transform.parent.GetComponentInParent<BusView>() == null)
                Turn(_restFacing);
        }

        // Hedefe doğru bir kare yürür, vardıysa true döner
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

        // Animator'a sadece değer değişince yazıyoruz, her kare aynı şeyi göndermek boşa iş
        private void SetWalking(bool walking)
        {
            if (_isWalking == walking)
                return;

            _isWalking = walking;
            animator.SetBool(IsWalkingId, walking);
        }

        // Önü kapalı yolcu titriyor, sadece modeli oynatıyoruz, hücredeki yeri bozulmasın diye
        private void UpdateShake()
        {
            if (_shakeTimeLeft <= 0f)
                return;

            _shakeTimeLeft -= Time.deltaTime;
            float strength = Mathf.Max(0f, _shakeTimeLeft) / ShakeDuration;
            float x = Mathf.Sin(_shakeTimeLeft * ShakeFrequency) * ShakeAmount * strength;
            animator.transform.localPosition = new Vector3(x, 0f, 0f);
        }

        // Bileşen eklendiği an Unity bunu çağırır, Animator alanını elle sürüklemekten kurtarıyor
        private void Reset()
        {
            animator = GetComponentInChildren<Animator>();
        }
    }
}