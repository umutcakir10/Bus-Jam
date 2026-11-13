using BusJam.Core;
using BusJam.Gameplay.Domain;
using UnityEngine;

namespace BusJam.Gameplay.Presentation
{
   
    public sealed class PassengerBoardView : MonoBehaviour
    {
        [SerializeField] private PassengerPool pool;

        
        [SerializeField] private Vector3 gridFacing = Vector3.forward;

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

        public void Clear()
        {
            if (_viewByCell == null)
                return;

            for (int i = 0; i < _viewByCell.Length; i++)
            {
                if (_viewByCell[i] != null)
                    pool.Release(_viewByCell[i]);
            }
            _viewByCell = null;
        }
    }
}