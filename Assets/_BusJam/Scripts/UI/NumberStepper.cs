using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BusJam.UI
{
    
    public sealed class NumberStepper : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private Button minusButton;
        [SerializeField] private Button plusButton;

        public event Action<int> Stepped;

        private void Awake()
        {
            minusButton.onClick.AddListener(() => Stepped?.Invoke(-1));
            plusButton.onClick.AddListener(() => Stepped?.Invoke(1));
        }

        public void SetLabel(string text) => label.text = text;

        public void SetValue(int value) => valueText.text = value.ToString();
    }
}