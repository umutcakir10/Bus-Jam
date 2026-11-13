using BusJam.Core;

namespace BusJam.Gameplay.Domain
{
    public enum MoveEventType
    {
        ToBus,       
        ToSlot,      
        SlotToBus,   
        BusDeparted, 
    }

    
    public readonly struct MoveEvent
    {
        public readonly MoveEventType Type;
        public readonly GridPos Cell;  
        public readonly int Slot;      
        public readonly int BusIndex;  
        public readonly int Seat;      

        private MoveEvent(MoveEventType type, GridPos cell, int slot, int busIndex, int seat)
        {
            Type = type;
            Cell = cell;
            Slot = slot;
            BusIndex = busIndex;
            Seat = seat;
        }

        public static MoveEvent ToBus(GridPos cell, int busIndex, int seat) =>
            new MoveEvent(MoveEventType.ToBus, cell, -1, busIndex, seat);

        public static MoveEvent ToSlot(GridPos cell, int slot) =>
            new MoveEvent(MoveEventType.ToSlot, cell, slot, -1, -1);

        public static MoveEvent SlotToBus(int slot, int busIndex, int seat) =>
            new MoveEvent(MoveEventType.SlotToBus, default, slot, busIndex, seat);

        public static MoveEvent BusDeparted(int busIndex) =>
            new MoveEvent(MoveEventType.BusDeparted, default, -1, busIndex, -1);
    }
}