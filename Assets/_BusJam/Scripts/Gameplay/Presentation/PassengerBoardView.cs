using BusJam.Core;
using BusJam.Gameplay.Domain;
using UnityEngine;
using System.Collections.Generic;
using BusJam.LevelSystem;

namespace BusJam.Gameplay.Presentation
{
   
    public sealed class PassengerBoardView : MonoBehaviour
    {
        [SerializeField] private PassengerPool pool;

        
        [SerializeField] private Vector3 gridFacing = Vector3.forward;

        [Header("Gizli yolcular")]
        [SerializeField] private Material hiddenMaterial;
        [SerializeField] private GameObject hiddenMarkPrefab;

        private readonly List<int> _hiddenCells = new List<int>(16);

        private PassengerView[] _viewByCell;

        public void Build(BoardModel board, BoardLayout layout)
        {
            Clear();
            _viewByCell = new PassengerView[board.CellCount];

            for (int i = 0; i < board.CellCount; i++)
            {
                GridPos cell = board.ToPos(i);
                if (board.GetCell(cell) != CellType.Passenger)
                    continue;

                PassengerView view = pool.Get(board.GetColor(cell));
                view.Place(layout.CellToWorld(cell), gridFacing);
                _viewByCell[i] = view;
            }
        }

        public PassengerView GetAt(int cellIndex) => _viewByCell[cellIndex];

        // Yolcu grid'den ayrılınca kaydını siliyoruz, artık o hücrede değil
        public PassengerView Take(int cellIndex)
        {
            PassengerView view = _viewByCell[cellIndex];
            _viewByCell[cellIndex] = null;
            return view;
        }

        // Level dosyasında küçük harfle yazılmış hücrelerdeki yolcuları gizliyoruz
        public void HideCells(LevelData level, bool showColor)
        {
            _hiddenCells.Clear();
            for (int y = 0; y < level.Height; y++)
            {
                string row = level.rows[y];
                for (int x = 0; x < level.Width; x++)
                {
                    if (!LevelFormat.IsHidden(row[x]))
                        continue;

                    int index = y * level.Width + x;
                    PassengerView view = _viewByCell[index];
                    if (view == null)
                        continue;

                    view.Hide(hiddenMaterial, hiddenMarkPrefab, showColor);
                    _hiddenCells.Add(index);
                }
            }
        }

        // Her hamleden sonra çağrılıyor, çıkış yolu açılan gizli yolcunun rengi ortaya çıkıyor
        // Yolu kapalıyken zaten hareket edemediği için oyuncu rengini bilmediği yolcuyu asla oynatamıyor
        public void RevealOpened(MoveResolver resolver, List<GridPos> pathBuffer)
        {
            for (int k = _hiddenCells.Count - 1; k >= 0; k--)
            {
                int index = _hiddenCells[k];
                PassengerView view = _viewByCell[index];
                if (view == null)
                {
                    _hiddenCells.RemoveAt(k);
                    continue;
                }

                if (!resolver.HasExitPath(resolver.Board.ToPos(index), pathBuffer))
                    continue;

                view.Reveal();
                _hiddenCells.RemoveAt(k);
            }
        }

        public void Clear()
        {
            if (_viewByCell == null)
                return;

            for (int i = 0; i < _viewByCell.Length; i++)
            {
                _hiddenCells.Clear();
                if (_viewByCell[i] != null)
                    pool.Release(_viewByCell[i]);
            }
            _viewByCell = null;
        }
    }
}