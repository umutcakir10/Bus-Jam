using System.Collections.Generic;
using BusJam.Gameplay.Domain;
using BusJam.Gameplay.Presentation;
using BusJam.LevelSystem;
using UnityEngine;

namespace BusJam.Flow
{
   
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private string levelId = "level_001";
        [SerializeField] private BoardLayoutSettings layoutSettings = new BoardLayoutSettings();
        [SerializeField] private BoardView boardView;
        [SerializeField] private CameraFitter cameraFitter;
        [SerializeField] private PassengerBoardView passengerView;

        private void Awake()
        {
            Application.targetFrameRate = 60;
        }

        private void Start()
        {
            var errors = new List<string>();
            if (!LevelRepository.TryLoadBuiltIn(levelId, out LevelData level, errors))
            {
                Debug.LogError($"Could not load '{levelId}':\n - {string.Join("\n - ", errors)}", this);
                return;
            }

            var board = new BoardModel(level);
            var layout = new BoardLayout(board.Width, board.Height, level.waitingSlotCount, layoutSettings);
            boardView.Build(board, layout);
            passengerView.Build(board, layout);
            cameraFitter.Fit(layout.GetPlayAreaBounds());
        }
    }
}
