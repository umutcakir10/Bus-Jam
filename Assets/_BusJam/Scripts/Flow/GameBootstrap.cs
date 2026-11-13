using System.Collections.Generic;
using BusJam.Core;
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
        [SerializeField] private BusLineView busLineView;
        [SerializeField] private TapInput tapInput;
        private MoveDirector _director;

        private MoveResolver _resolver;
        private BoardLayout _layout;

        
        private readonly List<GridPos> _path = new List<GridPos>(32);
        private readonly List<MoveEvent> _events = new List<MoveEvent>(16);

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

            _resolver = new MoveResolver(level);
            _layout = new BoardLayout(_resolver.Board.Width, _resolver.Board.Height, level.waitingSlotCount, layoutSettings);

            boardView.Build(_resolver.Board, _layout);
            passengerView.Build(_resolver.Board, _layout);
            busLineView.Build(_resolver.Buses, _layout);
            cameraFitter.Fit(_layout.GetPlayAreaBounds());
            _director = new MoveDirector(_resolver.Board, _layout, passengerView, busLineView);
        }

        private void Update()
        {
            if (_resolver == null || !tapInput.TryGetTappedCell(_layout, out GridPos cell))
                return;

            TapResult result = _resolver.Tap(cell, _path, _events);

            if (result == TapResult.Moved)
                _director.Play(_path, _events);
            else if (result == TapResult.Blocked)
                _director.PlayBlocked(cell);
            if (result == TapResult.Moved && _resolver.State != GameState.Playing)
                Debug.Log($"Game over: {_resolver.State}");
        }
    }
}