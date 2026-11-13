using System.Collections.Generic;
using System.Text;
using BusJam.Core;
using BusJam.Gameplay.Domain;
using BusJam.Gameplay.Presentation;
using BusJam.LevelSystem;
using BusJam.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BusJam.Flow
{
    // Level Editor sahnesinin yöneticisi burasıdır
    // Taslağı tutuyor butonlardan gelen istekleri taslağa uyguluyor ve her değişiklikte boardu yeniden çizmemize yarıyor
    public sealed class LevelEditorController : MonoBehaviour
    {
        private const int TimeStep = 5;

        [Header("Board")]
        [SerializeField] private BoardLayoutSettings layoutSettings = new BoardLayoutSettings();
        [SerializeField] private BoardView boardView;
        [SerializeField] private PassengerBoardView passengerView;
        [SerializeField] private BusLineView busLineView;
        [SerializeField] private CameraFitter cameraFitter;
        [SerializeField] private TapInput tapInput;

        // Üst ve alt paneller ekranı kapladığı için kameraya biraz daha derin bir alan gösteriyoruz
        [SerializeField] private float extraViewDepth = 8f;
        // Alt panel üsttekinden büyük ve board'u ortalamak için görüntüyü biraz yukarı kaydırıyoruz
        [SerializeField] private float viewShift = 1.5f;

        [Header("Top bar")]
        [SerializeField] private TMP_Text levelLabel;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button newButton;
        [SerializeField] private NumberStepper widthStepper;
        [SerializeField] private NumberStepper heightStepper;
        [SerializeField] private NumberStepper timeStepper;
        [SerializeField] private NumberStepper slotsStepper;

        [Header("Bottom bar")]
        [SerializeField] private PaletteButton paletteButtonPrefab;
        [SerializeField] private Transform paletteRoot;
        [SerializeField] private TMP_InputField busesInput;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button testButton;
        [SerializeField] private Button playButton;

        private readonly List<PaletteButton> _palette = new List<PaletteButton>();
        private readonly List<string> _errors = new List<string>();
        private readonly StringBuilder _status = new StringBuilder();

        private LevelDraft _draft;
        private BoardLayout _layout;
        private int _levelIndex;
        private char _brush = 'R';
        private bool _unsaved;

        private void Start()
        {
            LevelCatalog.Refresh();
            CreatePalette();
            HookUpControls();

            if (LevelCatalog.Count == 0)
                NewLevel();
            else
                OpenLevel(Mathf.Clamp(GameSession.LevelIndex, 0, LevelCatalog.Count - 1));
        }

        private void Update()
        {
            if (_layout == null || IsPointerOverUi())
                return;
            if (!tapInput.TryGetTapRay(out Ray ray))
                return;
            if (!TryFindTappedCell(ray, out GridPos cell))
                return;

            // Hücre zaten bu fırçanın rengindeyse boşuna yeniden çizmiyoruz
            if (_draft.GetCell(cell.X, cell.Y) == _brush)
                return;

            _draft.Paint(cell.X, cell.Y, _brush);
            MarkChanged();
        }

        
        private bool TryFindTappedCell(Ray ray, out GridPos cell)
        {
            if (TapInput.TryRayToCell(ray, tapInput.TapHeight, _layout, out cell)
                && LevelFormat.TryParseColor(_draft.GetCell(cell.X, cell.Y), out _))
                return true;

            return TapInput.TryRayToCell(ray, 0f, _layout, out cell);
        }

        private void HookUpControls()
        {
            previousButton.onClick.AddListener(() => OpenLevel(Wrap(_levelIndex - 1)));
            nextButton.onClick.AddListener(() => OpenLevel(Wrap(_levelIndex + 1)));
            newButton.onClick.AddListener(NewLevel);

            widthStepper.SetLabel("Width");
            widthStepper.Stepped += step => { _draft.Resize(_draft.Width + step, _draft.Height); MarkChanged(); };
            heightStepper.SetLabel("Height");
            heightStepper.Stepped += step => { _draft.Resize(_draft.Width, _draft.Height + step); MarkChanged(); };
            timeStepper.SetLabel("Time");
            timeStepper.Stepped += step => { _draft.SetTimeLimit(_draft.TimeLimitSeconds + step * TimeStep); MarkChanged(); };
            slotsStepper.SetLabel("Slots");
            slotsStepper.Stepped += step => { _draft.SetWaitingSlots(_draft.WaitingSlotCount + step); MarkChanged(); };

            busesInput.onEndEdit.AddListener(text => { _draft.SetBuses(text); MarkChanged(); });

            saveButton.onClick.AddListener(() => TrySave());
            testButton.onClick.AddListener(TestLevel);
            playButton.onClick.AddListener(() => SceneManager.LoadScene(SceneNames.Game));
        }

        private void CreatePalette()
        {
            AddPaletteButton(LevelFormat.ToChar(PassengerColor.Red), new Color32(0xE7, 0x1B, 0x31, 0xFF), "");
            AddPaletteButton(LevelFormat.ToChar(PassengerColor.Blue), new Color32(0x0A, 0x54, 0xD4, 0xFF), "");
            AddPaletteButton(LevelFormat.ToChar(PassengerColor.Green), new Color32(0x19, 0x97, 0x2C, 0xFF), "");
            AddPaletteButton(LevelFormat.ToChar(PassengerColor.Yellow), new Color32(0xE2, 0x93, 0x11, 0xFF), "");
            AddPaletteButton(LevelFormat.ToChar(PassengerColor.Purple), new Color32(0x50, 0x23, 0xBC, 0xFF), "");
            AddPaletteButton(LevelFormat.EmptyCell, new Color32(0xF2, 0xF2, 0xF2, 0xFF), "Empty");
            AddPaletteButton(LevelFormat.VoidCell, new Color32(0x3C, 0x3F, 0x44, 0xFF), "Hole");
            AddPaletteButton(LevelFormat.ObstacleCell, new Color32(0x5E, 0x64, 0x6B, 0xFF), "Wall");
        }

        private void AddPaletteButton(char cell, Color color, string text)
        {
            PaletteButton button = Instantiate(paletteButtonPrefab, paletteRoot);
            button.Setup(cell, color, text);
            button.Picked += picked =>
            {
                _brush = picked;
                RefreshPalette();
            };
            _palette.Add(button);
        }

        private void OpenLevel(int index)
        {
            if (LevelCatalog.Count == 0)
                return;

            string id = LevelCatalog.Ids[index];
            if (!LevelRepository.TryLoad(id, out LevelData level, _errors))
            {
                statusText.text = $"Could not open {id}:\n - {string.Join("\n - ", _errors)}";
                if (_draft == null)
                    NewLevel();
                return;
            }

            _levelIndex = index;
            _draft = LevelDraft.FromLevel(level);
            _unsaved = false;
            Rebuild();
        }

        private void NewLevel()
        {
            _draft = LevelDraft.CreateNew(LevelCatalog.NextFreeId());

            // Henüz kaydedilmediği için listenin sonundaymış gibi davranıyoruz
            _levelIndex = LevelCatalog.Count;
            _unsaved = true;
            Rebuild();
        }

        private void MarkChanged()
        {
            _unsaved = true;
            Rebuild();
        }

        // Taslağı oyunun kendi görüntü sınıflarıyla çiziyoruz ve aynı zamanda da editörde görülen oyundakiyle aynı
        
        private void Rebuild()
        {
            LevelData data = _draft.ToLevelData();
            var board = new BoardModel(data);
            var buses = new BusQueue(data);
            _layout = new BoardLayout(board.Width, board.Height, data.waitingSlotCount, layoutSettings);

            boardView.Build(board, _layout);
            passengerView.Build(board, _layout);
            busLineView.Build(buses, _layout);

            Bounds area = _layout.GetPlayAreaBounds();
            area.Expand(new Vector3(0f, 0f, extraViewDepth));
            area.center += Vector3.back * viewShift;
            cameraFitter.Fit(area);

            RefreshUi(data);
        }

        private void RefreshUi(LevelData data)
        {
            levelLabel.text = _unsaved ? $"{_draft.DisplayName} *" : _draft.DisplayName;
            widthStepper.SetValue(_draft.Width);
            heightStepper.SetValue(_draft.Height);
            timeStepper.SetValue(_draft.TimeLimitSeconds);
            slotsStepper.SetValue(_draft.WaitingSlotCount);
            busesInput.SetTextWithoutNotify(_draft.Buses);
            RefreshPalette();

            bool valid = LevelValidator.Validate(data, _errors);
            saveButton.interactable = valid;
            testButton.interactable = valid;

            _status.Clear();
            AppendColorCounts();
            if (valid)
            {
                _status.Append("Ready to save");
            }
            else
            {
                foreach (string error in _errors)
                    _status.Append("- ").Append(error).Append('\n');
            }
            statusText.text = _status.ToString();
        }

        // Her renk için yolcu sayısını ve otobüs koltuklarını yan yana gösteriyoruz
        // Tasarımcı ikisini eşitlemeye çalışırken hangi renkte kaç eksik olduğunu hemen görüyor
        private void AppendColorCounts()
        {
            for (int i = (int)PassengerColor.Red; i <= (int)PassengerColor.Purple; i++)
            {
                var color = (PassengerColor)i;
                int passengers = _draft.CountPassengers(color);
                int seats = _draft.CountBuses(color) * _draft.BusCapacity;
                if (passengers == 0 && seats == 0)
                    continue;

                _status.Append(LevelFormat.ToChar(color)).Append(": ")
                    .Append(passengers).Append(" passengers / ")
                    .Append(seats).Append(" seats\n");
            }
        }

        private void RefreshPalette()
        {
            foreach (PaletteButton button in _palette)
                button.SetSelected(button.Cell == _brush);
        }

        private bool TrySave()
        {
            LevelData data = _draft.ToLevelData();
            if (!LevelRepository.TrySave(data, _errors))
            {
                statusText.text = $"Could not save:\n - {string.Join("\n - ", _errors)}";
                return false;
            }

            _unsaved = false;
            _levelIndex = LevelCatalog.IndexOf(data.id);
            RefreshUi(data);
            statusText.text = $"Saved {data.id}";
            return true;
        }

        private void TestLevel()
        {
            if (!TrySave())
                return;

            GameSession.LevelIndex = _levelIndex;
            GameSession.StartScreenShown = true;
            GameSession.ReturnToEditor = true;
            SceneManager.LoadScene(SceneNames.Game);
        }

        private int Wrap(int index)
        {
            int count = LevelCatalog.Count;
            if (count == 0)
                return 0;
            return ((index % count) + count) % count;
        }

        // Panellerin üstüne dokunulunca boarda boya sürülmesin istiyoruz
        private static bool IsPointerOverUi()
        {
            if (EventSystem.current == null)
                return false;
            if (Input.touchCount > 0)
                return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
            return EventSystem.current.IsPointerOverGameObject();
        }
    }
}