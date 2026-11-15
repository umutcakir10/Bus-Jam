using System.Collections.Generic;
using BusJam.Core;
using BusJam.Gameplay.Domain;
using BusJam.Gameplay.Presentation;
using BusJam.LevelSystem;
using BusJam.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using BusJam.Audio;

namespace BusJam.Flow
{
    // Game sahnesinin yöneticisi
    // Levelı yükler görüntüleri kurar süreyi işletir oyun bitince kartı açar
    public sealed class GameController : MonoBehaviour
    {
        // Ready: Start kartı açık, oyuncunun Play'e basmasını bekliyoruz
        // Playing: oyuncu dokunabiliyor ve süre akıyor
        // Ending: oyun bitti ama son animasyonlar oynasın diye kısa bir süre bekliyoruz
        // Ended: sonuç kartı açık, butonlara basılmasını bekliyoruz
        private enum FlowState { Ready, Playing, Ending, Ended }

        [SerializeField] private BoardLayoutSettings layoutSettings = new BoardLayoutSettings();
        [SerializeField] private BoardView boardView;
        [SerializeField] private CameraFitter cameraFitter;
        [SerializeField] private EnvironmentLayout environment;
        [SerializeField] private PassengerBoardView passengerView;
        [SerializeField] private BusLineView busLineView;
        [SerializeField] private TapInput tapInput;
        [SerializeField] private GameHud hud;
        [SerializeField] private CardPanel cardPanel;
        [SerializeField] private TitleScreen titleScreen;
        [SerializeField] private float endPanelDelay = 1.5f;

        private MoveResolver _resolver;
        private BoardLayout _layout;
        private MoveDirector _director;
        private LevelTimer _timer;
        private FlowState _flow;
        private float _endPanelCountdown;

        // Her dokunuşta tekrar kullanılıyor oyun sırasında yeni liste açılmıyor
        private readonly List<GridPos> _path = new List<GridPos>(32);
        private readonly List<MoveEvent> _events = new List<MoveEvent>(16);
        private readonly List<GridPos> _revealPath = new List<GridPos>(32);

        private void Awake()
        {
            Application.targetFrameRate = 60;
            cardPanel.Hide();
            cardPanel.ActionClicked += OnCardAction;
            titleScreen.EditorClicked += OnEditorButton;
            hud.SetEditorButtonVisible(GameSession.ReturnToEditor);
            titleScreen.HideInstant();
            titleScreen.PlayClicked += OnTitlePlay;
            titleScreen.EditorClicked += OnEditorClicked;
        }

        private void OnDestroy()
        {
            cardPanel.ActionClicked -= OnCardAction;
            titleScreen.EditorClicked -= OnEditorButton;
            titleScreen.PlayClicked -= OnTitlePlay;
            titleScreen.EditorClicked -= OnEditorClicked;
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
            if (!LevelRepository.TryLoad(levelId, out LevelData level, errors))
            {
                Debug.LogError($"Could not load '{levelId}':\n - {string.Join("\n - ", errors)}", this);
                return;
            }

            _resolver = new MoveResolver(level);
            _layout = new BoardLayout(_resolver.Board.Width, _resolver.Board.Height, level.waitingSlotCount, layoutSettings);

            boardView.Build(_resolver.Board, _layout);
            passengerView.Build(_resolver.Board, _layout);
            passengerView.HideCells(level, false);
            passengerView.RevealOpened(_resolver, _revealPath);
            busLineView.Build(_resolver.Buses, _layout);
            cameraFitter.Fit(_layout.GetPlayAreaBounds());
            environment.Place(_layout);
            _director = new MoveDirector(_resolver.Board, _layout, passengerView, busLineView);

            _timer = new LevelTimer(level.timeLimitSeconds);
            hud.SetLevel(level.displayName);
            hud.SetTime(_timer.Remaining);

            // Editörden test ediliyorsa oyuncunun kaydına dokunmuyoruz
            if (!GameSession.ReturnToEditor)
                GameSession.SaveProgress(GameSession.LevelIndex);

            if (GameSession.StartScreenShown)
            {
                BeginPlay();
            }
            else
            {
                _flow = FlowState.Ready;
                titleScreen.Show(level.displayName);
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
            cardPanel.Hide();
            titleScreen.Hide();
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
                {
                    _director.Play(_path, _events);
                    passengerView.RevealOpened(_resolver, _revealPath);
                    SoundPlayer.Play(Sfx.Tap);
                }
                else if (result == TapResult.Blocked)
                {
                    _director.PlayBlocked(cell);
                    SoundPlayer.Play(Sfx.Blocked);
                }
            }

            if (_resolver.State == GameState.Playing)
                return;

            _timer.Stop();
            _flow = FlowState.Ending;

            // Kazanınca hemen sonraki levelı kaydediyoruz
            // Oyuncu kart açıkken oyunu kapatsa bile bir dahaki sefere yeni leveldan başlar
            if (_resolver.State == GameState.Won && !GameSession.ReturnToEditor)
                GameSession.SaveProgress((GameSession.LevelIndex + 1) % LevelCatalog.Count);

            // Kazanınca asıl bekleme son otobüsün gitmesi buradaki süre sadece kısa bir nefes
            // Süre bittiyse kart neredeyse hemen gelebilir
            if (_resolver.State == GameState.Won || _resolver.LoseReason == LoseReason.TimeUp)
                _endPanelCountdown = 0.3f;
            else
                _endPanelCountdown = endPanelDelay;
        }

        private void UpdateEnding()
        {
            // Son yolcu binip son otobüs yola çıkmadan kazanma kartını açmıyoruz
            if (_resolver.State == GameState.Won && !busLineView.AllDeparted)
                return;
            _endPanelCountdown -= Time.deltaTime;
            if (_endPanelCountdown > 0f)
                return;

            _flow = FlowState.Ended;
            if (_resolver.State == GameState.Won)
            {
                SoundPlayer.Play(Sfx.Win);
                cardPanel.ShowWin(CountStars(), GameSession.LevelIndex == LevelCatalog.Count - 1);
            }
            else
            {
                SoundPlayer.Play(Sfx.Lose);
                cardPanel.ShowLose(_resolver.LoseReason);
            }
        }

        // Sürenin yarısından fazlası kaldıysa 3, dörtte birinden fazlası kaldıysa 2, yoksa 1 yıldız
        private int CountStars()
        {
            float left = _timer.Remaining / _timer.Duration;
            if (left >= 0.5f)
                return 3;
            return left >= 0.25f ? 2 : 1;
        }

        private void OnCardAction(CardAction action)
        {
            SoundPlayer.Play(Sfx.Click);
            switch (action)
            {
                case CardAction.Play:
                    if (_flow == FlowState.Ready)
                        BeginPlay();
                    break;
                case CardAction.Next:
                    // Son leveldan sonra başa dönüyoruz
                    GameSession.LevelIndex = (GameSession.LevelIndex + 1) % LevelCatalog.Count;
                    ReloadScene();
                    break;
                case CardAction.Retry:
                    ReloadScene();
                    break;
                case CardAction.Menu:
                    // Ana menüye dönünce Start kartı tekrar çıkıyor ve oyuncunun kayıtlı levelı açılıyor
                    GameSession.ReturnToEditor = false;
                    GameSession.StartScreenShown = false;
                    GameSession.ReloadFromSave();
                    ReloadScene();
                    break;
                case CardAction.Editor:
                    OnEditorClicked();
                    break;
            }
        }

        private void OnTitlePlay()
        {
            SoundPlayer.Play(Sfx.Click);
            if (_flow == FlowState.Ready)
                BeginPlay();
        }
        private void OnEditorClicked()
        {
            SceneManager.LoadScene(SceneNames.LevelEditor);
        }

        
        // Karttaki Editor butonu OnCardActionda zaten tıkladığı için oraya ses eklemedik iki kere çalmasın
        private void OnEditorButton()
        {
            SoundPlayer.Play(Sfx.Click);
            OnEditorClicked();
        }

        // Sahneyi baştan yüklemek en temiz sıfırlama, önceki leveldan hiçbir şey kalmıyor
        private void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}