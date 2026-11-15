using DG.Tweening;
using UnityEngine;

namespace BusJam.UI
{
    // İlk levellarda oyuncu takılınca doğru yolcuyu gösteren el
    public sealed class TutorialHand : MonoBehaviour
    {
        [SerializeField] private RectTransform hand;
        [SerializeField] private float pressDistance = 40f;
        [SerializeField] private float pressDuration = 0.45f;
        // Elin hedefe yaklaştığı yön, görsel sağ alttan sol üste uzanan bir parmak
        [SerializeField] private Vector2 pressDirection = new Vector2(0.7f, -0.7f);

        private Tween _press;

        public bool IsShowing => hand.gameObject.activeSelf;

        private void Awake()
        {
            hand.gameObject.SetActive(false);
        }

        public void ShowAt(Vector3 screenPosition)
        {
            // Canvas ölçeğiyle çarpıyoruz ki her ekran boyutunda aynı mesafe görünsün
            float distance = pressDistance * hand.lossyScale.y;
            Vector3 offset = (Vector3)pressDirection * distance;

            hand.gameObject.SetActive(true);
            hand.position = screenPosition + offset;

            // Parmak yolcuya doğru uzanıp geri çekiliyor, buraya dokun demenin en tanıdık yolu
            _press?.Kill();
            _press = hand.DOMove(screenPosition, pressDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(hand.gameObject);
        }

        public void Hide()
        {
            _press?.Kill();
            hand.gameObject.SetActive(false);
        }
    }
}