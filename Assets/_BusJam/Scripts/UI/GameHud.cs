using TMPro;
using UnityEngine;

namespace BusJam.UI
{
    // Üstteki level adı ve süre
    public sealed class GameHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private float warningSeconds = 10f;
        [SerializeField] private Color normalColor = new Color32(0x22, 0x26, 0x2B, 0xFF);
        [SerializeField] private Color warningColor = new Color32(0xE7, 0x1B, 0x31, 0xFF);

        // Süreyi her kare string'e çevirmek çöp üretir, onun yerine karakterleri bu diziye kendimiz yazıyoruz

        private readonly char[] _timeChars = new char[5];
        private int _shownSeconds = -1;

        public void SetLevel(string title)
        {
            levelText.text = title;
        }

        public void SetTime(float secondsLeft)
        {
            int total = Mathf.CeilToInt(Mathf.Max(0f, secondsLeft));

            
            if (total == _shownSeconds)
                return;
            _shownSeconds = total;

            int minutes = total / 60;
            int seconds = total % 60;
            int length = 0;
            if (minutes >= 10)
                _timeChars[length++] = (char)('0' + minutes / 10);
            _timeChars[length++] = (char)('0' + minutes % 10);
            _timeChars[length++] = ':';
            _timeChars[length++] = (char)('0' + seconds / 10);
            _timeChars[length++] = (char)('0' + seconds % 10);

            timerText.SetCharArray(_timeChars, 0, length);

            // Son saniyelerde süre kırmızıya dönüyor, oyuncu acele etmesi gerektiğini görsün
            timerText.color = total <= warningSeconds ? warningColor : normalColor;
        }
    }
}