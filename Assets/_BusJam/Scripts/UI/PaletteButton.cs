using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BusJam.UI
{
    
    public sealed class PaletteButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image swatch;
        [SerializeField] private TMP_Text label;
        [SerializeField] private GameObject selectedFrame;

        private char _cell;

        public char Cell => _cell;
        public event Action<char> Picked;

        private void Awake()
        {
            button.onClick.AddListener(() => Picked?.Invoke(_cell));
        }

        public void Setup(char cell, Color color, string text)
        {
            _cell = cell;
            swatch.color = color;
            label.text = text;

            // Açık renkli butonda koyu yazı ve koyu renkli butonda beyaz yazı olacak
            label.color = color.grayscale > 0.6f ? new Color32(0x22, 0x26, 0x2B, 0xFF) : Color.white;
        }

        public void SetSelected(bool selected) => selectedFrame.SetActive(selected);
    }
}