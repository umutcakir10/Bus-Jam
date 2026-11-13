using System;
using BusJam.Core;

namespace BusJam.Gameplay.Domain
{
    
    public sealed class WaitingArea
    {
        private readonly PassengerColor[] _slots;

        public int Capacity => _slots.Length;
        public int Count { get; private set; }
        public bool IsFull => Count == Capacity;
        public bool IsEmpty => Count == 0;

        public WaitingArea(int capacity)
        {
            if (capacity < 1)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            _slots = new PassengerColor[capacity];
        }

        public PassengerColor GetColor(int slot) => _slots[slot];

        
        public int Add(PassengerColor color)
        {
            if (color == PassengerColor.None)
                throw new ArgumentException("A waiting passenger needs a color.", nameof(color));

            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] != PassengerColor.None)
                    continue;

                _slots[i] = color;
                Count++;
                return i;
            }

            throw new InvalidOperationException("Waiting area is full.");
        }

        
        public bool TryFindColor(PassengerColor color, out int slot)
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] == color && color != PassengerColor.None)
                {
                    slot = i;
                    return true;
                }
            }

            slot = -1;
            return false;
        }

        public void Remove(int slot)
        {
            if (_slots[slot] == PassengerColor.None)
                throw new InvalidOperationException($"Slot {slot} is already empty.");

            _slots[slot] = PassengerColor.None;
            Count--;
        }
    }
}