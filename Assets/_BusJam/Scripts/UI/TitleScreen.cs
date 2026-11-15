using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BusJam.UI
{
    // Oyun açılınca çıkan tam ekran başlık ekranı
   
    public sealed class TitleScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private Button playButton;
        [SerializeField] private Button editorButton;
        [SerializeField] private Button tapArea;

        private Tween _breath;
        private Tween _fade;

        public event Action PlayClicked;
        public event Action EditorClicked;

        private void Awake()
        {
            playButton.onClick.AddListener(() => PlayClicked?.Invoke());
            editorButton.onClick.AddListener(() => EditorClicked?.Invoke());
            tapArea.onClick.AddListener(() => PlayClicked?.Invoke());
        }

        public void Show(string levelTitle)
        {
            levelText.text = levelTitle;
            gameObject.SetActive(true);

            _fade?.Kill();
            group.alpha = 1f;
            group.interactable = true;

            // Play butonu beklerken hafifçe büyüyüp küçülüyor oyuncuyu dokunmaya davet ediyor
            _breath?.Kill();
            playButton.transform.localScale = Vector3.one;
            _breath = playButton.transform.DOScale(1.06f, 0.7f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject);
        }

        // Ekran bir anda kaybolmak yerine yumuşakça soluyor arkasındaki board ortaya çıkıyor
        public void Hide()
        {
            if (!gameObject.activeSelf)
                return;

            _breath?.Kill();
            _fade?.Kill();
            group.interactable = false;
            _fade = group.DOFade(0f, 0.3f)
                .SetLink(gameObject)
                .OnComplete(() => gameObject.SetActive(false));
        }

        // Oyun başında animasyonsuz gizlemek için
        public void HideInstant()
        {
            _breath?.Kill();
            _fade?.Kill();
            gameObject.SetActive(false);
        }
    }
}