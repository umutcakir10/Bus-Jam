using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BusJam.UI
{
    // Oyun açılınca karşımıza çıkan  dokun ve başla ekranı
    // Panelin tamamı bir buton olduğundan dolayı oyuncu ekranın herhangi bir yerine dokunabilir
    public sealed class StartPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private Button playButton;
        [SerializeField] private Button editorButton;

        public event Action EditorClicked;

        public event Action PlayClicked;

        private void Awake()
        {
            playButton.onClick.AddListener(() => PlayClicked?.Invoke());
            editorButton.onClick.AddListener(() => EditorClicked?.Invoke());
        }

        public void Show(string levelTitle)
        {
            levelText.text = levelTitle;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
