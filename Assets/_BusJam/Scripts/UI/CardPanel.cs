using System;
using BusJam.Gameplay.Domain;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BusJam.UI
{
    public enum CardAction { Play, Next, Retry, Menu, Editor }

    // Start, kazanma ve kaybetme ekranlarının hepsi aynı kart
    // Hangi ekran olduğuna göre sadece içeriği değişiyor: kurdele rengi, başlık, ikon, resim ve butonlar
    
    public sealed class CardPanel : MonoBehaviour
    {
        [Serializable]
        private struct CardButton
        {
            public Button button;
            public TMP_Text label;
            public Image icon;
        }

        [Header("Parts")]
        [SerializeField] private CanvasGroup group;
        [SerializeField] private RectTransform card;
        [SerializeField] private Image ribbon;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text subtitle;
        [SerializeField] private Image topIcon;
        [SerializeField] private GameObject starsRoot;
        [SerializeField] private Image[] stars;
        [SerializeField] private Image illustration;
        [SerializeField] private CardButton primary;
        [SerializeField] private CardButton secondary;

        [Header("Top icons")]
        [SerializeField] private Sprite stopwatchIcon;
        [SerializeField] private Sprite slotsFullIcon;
        [SerializeField] private Sprite busIcon;
        [SerializeField] private Sprite starFull;
        [SerializeField] private Sprite starEmpty;

        [Header("Illustrations")]
        [SerializeField] private Sprite missedBusArt;
        [SerializeField] private Sprite noSpaceArt;
        [SerializeField] private Sprite winArt;
        [SerializeField] private Sprite winAllArt;

        [Header("Button icons")]
        [SerializeField] private Sprite retryIcon;
        [SerializeField] private Sprite homeIcon;
        [SerializeField] private Sprite nextIcon;
        [SerializeField] private Sprite playIcon;
        [SerializeField] private Sprite editIcon;

        [Header("Ribbon colors")]
        [SerializeField] private Color failColor = new Color32(0xE8, 0x32, 0x3C, 0xFF);
        [SerializeField] private Color winColor = new Color32(0x8B, 0x3F, 0xE0, 0xFF);
        [SerializeField] private Color startColor = new Color32(0x2F, 0x7B, 0xF0, 0xFF);

        private CardAction _primaryAction;
        private CardAction _secondaryAction;
        private Sequence _show;

        public event Action<CardAction> ActionClicked;

        private void Awake()
        {
            primary.button.onClick.AddListener(() => ActionClicked?.Invoke(_primaryAction));
            secondary.button.onClick.AddListener(() => ActionClicked?.Invoke(_secondaryAction));
        }

        public void ShowStart(string levelTitle)
        {
            Fill(startColor, "Bus Jam", levelTitle, busIcon, 0, winAllArt);
            SetButtons(CardAction.Play, "Play", playIcon, CardAction.Editor, "Level Editor", editIcon);
            Open(false);
        }

        public void ShowWin(int starCount, bool wasLastLevel)
        {
            if (wasLastLevel)
            {
                Fill(winColor, "Level Complete!", "You finished every level!", null, starCount, winAllArt);
                SetButtons(CardAction.Next, "Start Over", homeIcon, CardAction.Retry, "Retry", retryIcon);
            }
            else
            {
                Fill(winColor, "Level Complete!", "Every bus is on its way!", null, starCount, winArt);
                SetButtons(CardAction.Next, "Next Level", nextIcon, CardAction.Retry, "Retry", retryIcon);
            }
            Open(false);
        }

        public void ShowLose(LoseReason reason)
        {
            if (reason == LoseReason.TimeUp)
                Fill(failColor, "Level Failed!", "Out of time!", stopwatchIcon, 0, missedBusArt);
            else
                Fill(failColor, "Level Failed!", "No space left to wait", slotsFullIcon, 0, noSpaceArt);

            SetButtons(CardAction.Retry, "Retry", retryIcon, CardAction.Menu, "Main Menu", homeIcon);
            Open(true);
        }

        public void Hide()
        {
            _show?.Kill();
            gameObject.SetActive(false);
        }

        // Yıldız varsa üstte yıldızlar yoksa da tek bir ikon gösteriliyor
        private void Fill(Color ribbonColor, string titleText, string subtitleText, Sprite icon, int starCount, Sprite art)
        {
            ribbon.color = ribbonColor;
            title.text = titleText;
            subtitle.text = subtitleText;

            topIcon.gameObject.SetActive(starCount == 0 && icon != null);
            topIcon.sprite = icon;

            starsRoot.SetActive(starCount > 0);
            for (int i = 0; i < stars.Length; i++)
                stars[i].sprite = i < starCount ? starFull : starEmpty;

            illustration.sprite = art;
        }

        private void SetButtons(CardAction primaryAction, string primaryText, Sprite primaryIcon,
                                CardAction secondaryAction, string secondaryText, Sprite secondaryIcon)
        {
            _primaryAction = primaryAction;
            primary.label.text = primaryText;
            primary.icon.sprite = primaryIcon;

            _secondaryAction = secondaryAction;
            secondary.label.text = secondaryText;
            secondary.icon.sprite = secondaryIcon;
        }

        // Kart arkadaki karartmayla birlikte yaylanarak açılıyor
        // Kaybedince bir kere sallanıyor kazanınca yıldızlar sırayla beliriyor
        private void Open(bool failed)
        {
            gameObject.SetActive(true);
            _show?.Kill();

            group.alpha = 0f;
            card.localScale = Vector3.one * 0.7f;
            card.localRotation = Quaternion.identity;

            _show = DOTween.Sequence().SetLink(gameObject)
                .Append(group.DOFade(1f, 0.2f))
                .Join(card.DOScale(1f, 0.45f).SetEase(Ease.OutBack));

            if (failed)
                _show.Append(card.DOShakeRotation(0.4f, new Vector3(0f, 0f, 5f), 12, 0f));

            if (topIcon.gameObject.activeSelf)
            {
                topIcon.transform.localScale = Vector3.zero;
                _show.Insert(0.25f, topIcon.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack));
            }

            if (starsRoot.activeSelf)
            {
                for (int i = 0; i < stars.Length; i++)
                {
                    stars[i].transform.localScale = Vector3.zero;
                    _show.Append(stars[i].transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
                }
            }
        }
    }
}