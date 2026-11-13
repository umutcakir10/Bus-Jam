using System.Collections.Generic;
using BusJam.Core;
using BusJam.Gameplay.Domain;
using BusJam.Gameplay.Presentation;
using BusJam.LevelSystem;
using BusJam.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BusJam.Flow
{
    
    public sealed class GameController : MonoBehaviour
    {
        // Readyde Start paneli açık ve  oyuncunun dokunmasını bekliyoruz
        // Playing: oyuncu dokunabiliyor ve süre akıyor
        // Ending: oyun bitti ama son animasyonlar oynasın diye kısa bir süre bekliyoruz
        // Ended: bitiş paneli açık, butonlara basılmasını bekliyoruz
        private enum FlowState { Ready, Playing, Ending, Ended }

        [SerializeField] private BoardLayoutSettings layoutSettings = new BoardLayoutSettings();
        [SerializeField] private BoardView boardView;
        [SerializeField] private CameraFitter cameraFitter;
        [SerializeField] private PassengerBoardView passengerView;
        [SerializeField] private BusLineView busLineView;
        [SerializeField] private TapInput tapInput;
        [SerializeField] private GameHud hud;
        [SerializeField] private StartPanel startPanel;
        [SerializeField] private EndPanel endPanel;
        [SerializeField] private float endPanelDelay = 1.5f;

        private MoveResolver _resolver;
        private BoardLayout _layout;
        private MoveDirector _director;
        private LevelTimer _timer;
        private FlowState _flow;
        private float _endPanelCountdown;

        // Her dokunuşta tekrar kullanılıyor, oyun sırasında yeni liste açılmıyor
        private readonly List<GridPos> _path = new List<GridPos>(32);
        private readonly List<MoveEvent> _events = new List<MoveEvent>(16);

        private void Awake()
        {
            Application.targetFrameRate = 60;
            startPanel.Hide();
            endPanel.Hide();
            startPanel.PlayClicked += OnPlayClicked;
            endPanel.NextClicked += OnNextClicked;
            endPanel.RetryClicked += OnRetryClicked;
        }

        private void OnDestroy()
        {
            startPanel.PlayClicked -= OnPlayClicked;
            endPanel.NextClicked -= OnNextClicked;
            endPanel.RetryClicked -= OnRetryClicked;
        }

        private void Start()
        {
            if (LevelCatalog.Count == 0)
            {
                Debug.LogError("No levels found in Resources/Levels.", this);
                return;
            }

            GameSession.EnsureLoaded();

            // Kayıtlı sıra numarası level sayısını aşıyorsa başa dönüyoruz
            if (GameSession.LevelIndex < 0 || GameSession.LevelIndex >= LevelCatalog.Count)
                GameSession.LevelIndex = 0;

            string levelId = LevelCatalog.Ids[GameSession.LevelIndex];
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

            _timer = new LevelTimer(level.timeLimitSeconds);
            hud.SetLevel(level.displayName);
            hud.SetTime(_timer.Remaining);

            // Oyuncu eğer ki bu levelı yarıda bırakıp çıkarsa bir dahaki sefere buradan devam etsin
            GameSession.SaveProgress(GameSession.LevelIndex);

            if (GameSession.StartScreenShown)
            {
                BeginPlay();
            }
            else
            {
                _flow = FlowState.Ready;
                startPanel.Show(level.displayName);
            }
        }

        private void Update()
        {
            if (_resolver == null)
                return;

            switch (_flow)
            {
                case FlowState.Playing:
                    UpdatePlaying();
                    break;
                case FlowState.Ending:
                    UpdateEnding();
                    break;
            }
        }

        private void BeginPlay()
        {
            startPanel.Hide();
            GameSession.StartScreenShown = true;
            _timer.Start();
            _flow = FlowState.Playing;
        }

        private void UpdatePlaying()
        {
            if (_timer.Tick(Time.deltaTime))
                _resolver.TimeUp();
            hud.SetTime(_timer.Remaining);

            if (_resolver.State == GameState.Playing && tapInput.TryGetTappedCell(_layout, out GridPos cell))
            {
                TapResult result = _resolver.Tap(cell, _path, _events);
                if (result == TapResult.Moved)
                    _director.Play(_path, _events);
                else if (result == TapResult.Blocked)
                    _director.PlayBlocked(cell);
            }

            if (_resolver.State == GameState.Playing)
                return;

            _timer.Stop();
            _flow = FlowState.Ending;

            // Kazanınca hemen sonraki levelı kaydediyoruz
            // Oyuncu paneldeyken oyunu kapatsa bile bir dahaki sefere yeni leveldan başlasın
            if (_resolver.State == GameState.Won)
                GameSession.SaveProgress((GameSession.LevelIndex + 1) % LevelCatalog.Count);

           
            _endPanelCountdown = _resolver.LoseReason == LoseReason.TimeUp ? 0.3f : endPanelDelay;
        }

        private void UpdateEnding()
        {
            _endPanelCountdown -= Time.deltaTime;
            if (_endPanelCountdown > 0f)
                return;

            _flow = FlowState.Ended;
            if (_resolver.State == GameState.Won)
                endPanel.ShowWin(GameSession.LevelIndex == LevelCatalog.Count - 1);
            else
                endPanel.ShowLose(_resolver.LoseReason);
        }

        private void OnPlayClicked()
        {
            if (_flow == FlowState.Ready)
                BeginPlay();
        }

        private void OnNextClicked()
        {
            // Son level'dan sonra başa dönüyoruz
            GameSession.LevelIndex = (GameSession.LevelIndex + 1) % LevelCatalog.Count;
            ReloadScene();
        }

        private void OnRetryClicked()
        {
            ReloadScene();
        }

        // Sahneyi baştan yüklemek en temiz sıfırlama yaptığım yer önceki level'dan hiçbir şey kalmıyor
        private void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}