using System.Collections.Generic;
using BusJam.Core;
using BusJam.LevelSystem;

namespace BusJam.Gameplay.Domain
{
    public enum GameState { Playing, Won, Lost }

    public enum TapResult
    {
        Ignored, 
        Blocked, 
        Moved,   
    }

   
    public sealed class MoveResolver
    {
        public BoardModel Board { get; }
        public BusQueue Buses { get; }
        public WaitingArea Waiting { get; }
        public GameState State { get; private set; }

        private readonly BoardPathfinder _pathfinder;

        
        public MoveResolver(LevelData level)
        {
            Board = new BoardModel(level);
            Buses = new BusQueue(level);
            Waiting = new WaitingArea(level.waitingSlotCount);
            _pathfinder = new BoardPathfinder(Board);
            State = GameState.Playing;
        }

       
        public TapResult Tap(GridPos cell, List<GridPos> path, List<MoveEvent> events)
        {
            path.Clear();
            events.Clear();

            if (State != GameState.Playing)
                return TapResult.Ignored;
            if (!Board.IsInside(cell) || Board.GetCell(cell) != CellType.Passenger)
                return TapResult.Ignored;
            if (!_pathfinder.TryFindExitPath(cell, path))
                return TapResult.Blocked;

            PassengerColor color = Board.GetColor(cell);
            Board.RemovePassenger(cell);

            if (Buses.CanBoard(color))
            {
                int busIndex = Buses.CurrentIndex;
                int seat = Buses.Board(color);
                events.Add(MoveEvent.ToBus(cell, busIndex, seat));
                DepartFullBuses(events);
            }
            else
            {
                
                int slot = Waiting.Add(color);
                events.Add(MoveEvent.ToSlot(cell, slot));
            }

            UpdateState();
            return TapResult.Moved;
        }

        private void DepartFullBuses(List<MoveEvent> events)
        {
            while (Buses.IsCurrentFull)
            {
                events.Add(MoveEvent.BusDeparted(Buses.CurrentIndex));
                Buses.Depart();

              
                while (!Buses.AllDeparted && !Buses.IsCurrentFull
                       && Waiting.TryFindColor(Buses.CurrentColor, out int slot))
                {
                    Waiting.Remove(slot);
                    int busIndex = Buses.CurrentIndex;
                    int seat = Buses.Board(Buses.CurrentColor);
                    events.Add(MoveEvent.SlotToBus(slot, busIndex, seat));
                }
            }
        }

        private void UpdateState()
        {
            if (Buses.AllDeparted)
                State = GameState.Won;
            else if (Waiting.IsFull)
                State = GameState.Lost;
        }
    }
}
