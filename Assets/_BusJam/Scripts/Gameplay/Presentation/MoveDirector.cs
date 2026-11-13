using System.Collections.Generic;
using BusJam.Core;
using BusJam.Gameplay.Domain;
using UnityEngine;

namespace BusJam.Gameplay.Presentation
{
    
    public sealed class MoveDirector
    {
        private readonly BoardModel _board;
        private readonly BoardLayout _layout;
        private readonly PassengerBoardView _passengers;
        private readonly BusLineView _buses;

        // Hangi slotta hangi yolcu bekliyor, otobüs gelince buradan bulup gönderiyoruz
        private readonly PassengerView[] _slotViews;

        public MoveDirector(BoardModel board, BoardLayout layout, PassengerBoardView passengers, BusLineView buses)
        {
            _board = board;
            _layout = layout;
            _passengers = passengers;
            _buses = buses;
            _slotViews = new PassengerView[layout.SlotCount];
        }

        public void Play(List<GridPos> path, List<MoveEvent> events)
        {
            for (int i = 0; i < events.Count; i++)
            {
                MoveEvent e = events[i];
                switch (e.Type)
                {
                    case MoveEventType.ToBus:
                        {
                            PassengerView view = TakeFromGrid(e.Cell, path);
                            view.BoardBus(_buses.GetBus(e.BusIndex), e.Seat);
                            break;
                        }
                    case MoveEventType.ToSlot:
                        {
                            PassengerView view = TakeFromGrid(e.Cell, path);
                            view.WalkTo(_layout.SlotToWorld(e.Slot));
                            view.SetRestFacing(Vector3.forward);
                            _slotViews[e.Slot] = view;
                            break;
                        }
                    case MoveEventType.SlotToBus:
                        {
                            PassengerView view = _slotViews[e.Slot];
                            _slotViews[e.Slot] = null;
                            view.BoardBus(_buses.GetBus(e.BusIndex), e.Seat);
                            break;
                        }
                    case MoveEventType.BusDeparted:
                        _buses.GetBus(e.BusIndex).RequestDeparture();
                        break;
                }
            }
        }

        public void PlayBlocked(GridPos cell)
        {
            PassengerView view = _passengers.GetAt(_board.ToIndex(cell));
            if (view != null)
                view.Shake();
        }

        private PassengerView TakeFromGrid(GridPos cell, List<GridPos> path)
        {
            PassengerView view = _passengers.Take(_board.ToIndex(cell));

            // path[0] yolcunun durduğu hücre oradan sonraki hücreleri sırayla yürüyor
            for (int i = 1; i < path.Count; i++)
                view.WalkTo(_layout.CellToWorld(path[i]));

            return view;
        }
    }
}