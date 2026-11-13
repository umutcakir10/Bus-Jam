using System.Collections.Generic;
using BusJam.Core;
using BusJam.LevelSystem;

namespace BusJam.Gameplay.Domain
{
    public enum GameState { Playing, Won, Lost }

    public enum LoseReason { None, NoSpaceLeft, TimeUp }

    public enum TapResult
    {
        Ignored,
        Blocked,
        Moved,
    }

 
    // Board'un, otobüslerin ve slotların sahibi bir dokunuşu zincirleme tepkileriyle birlikte anında çözer
    // Görüntü sadece ürettiği olayları oynatır
    public sealed class MoveResolver
    {
        public BoardModel Board { get; }
        public BusQueue Buses { get; }
        public WaitingArea Waiting { get; }
        public GameState State { get; private set; }
        public LoseReason LoseReason { get; private set; }

        private readonly BoardPathfinder _pathfinder;

        public MoveResolver(LevelData level)
        {
            Board = new BoardModel(level);
            Buses = new BusQueue(level);
            Waiting = new WaitingArea(level.waitingSlotCount);
            _pathfinder = new BoardPathfinder(Board);
            State = GameState.Playing;
        }

        // Listeleri çağıran veriyor ve her dokunuşta tekrar kullanıyor böylece hiç çöp üretmiyoruz
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
                // Burada mutlaka boş slot var çünkü slotlar dolduğu an oyun bitiyor
                int slot = Waiting.Add(color);
                events.Add(MoveEvent.ToSlot(cell, slot));
            }

            UpdateState();
            return TapResult.Moved;
        }

        // Süre dolunca dışarıdan çağrılıyor, oyun zaten bittiyse hiçbir şey yapmıyor
        public void TimeUp()
        {
            if (State != GameState.Playing)
                return;

            State = GameState.Lost;
            LoseReason = LoseReason.TimeUp;
        }

        private void DepartFullBuses(List<MoveEvent> events)
        {
            // Otobüs gidiyor yenisi slottan yolcu alıyor, o da dolarsa o da gidiyor
            // Zincir kaç halka olursa olsun tek dokunuşta çözülüyor
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
            {
                State = GameState.Won;
            }
            else if (Waiting.IsFull)
            {
                State = GameState.Lost;
                LoseReason = LoseReason.NoSpaceLeft;
            }
        }
    }
}