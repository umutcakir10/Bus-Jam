using System;
using BusJam.Gameplay.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BusJam.UI
{
    // Level bitince açılan panel
    // Butonlara basılınca ne olacağını bilmiyor, sadece basıldı diye haber veriyor
    public sealed class EndPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button nextButton;
        [SerializeField] private TMP_Text nextLabel;
        [SerializeField] private Button retryButton;

        public event Action NextClicked;
        public event Action RetryClicked;

        private void Awake()
        {
            nextButton.onClick.AddListener(() => NextClicked?.Invoke());
            retryButton.onClick.AddListener(() => RetryClicked?.Invoke());
        }

        public void ShowWin(bool wasLastLevel)
        {
            titleText.text = "Level Complete!";
            messageText.text = wasLastLevel ? "You finished every level!" : "Every bus is on its way";
            nextLabel.text = wasLastLevel ? "Start Over" : "Next Level";
            nextButton.gameObject.SetActive(true);
            gameObject.SetActive(true);
        }

        public void ShowLose(LoseReason reason)
        {
            titleText.text = "Level Failed";
            messageText.text = reason == LoseReason.TimeUp ? "Out of time" : "No space left to wait";

            
            nextButton.gameObject.SetActive(false);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}