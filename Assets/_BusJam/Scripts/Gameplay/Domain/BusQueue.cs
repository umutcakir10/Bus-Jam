using System;
using BusJam.Core;
using BusJam.LevelSystem;

namespace BusJam.Gameplay.Domain
{
    
    public sealed class BusQueue
    {
        private readonly PassengerColor[] _colors;

        public int Capacity { get; }
        public int Count => _colors.Length;

        
        public int CurrentIndex { get; private set; }
        public int SeatsTaken { get; private set; }

        public bool AllDeparted => CurrentIndex >= _colors.Length;
        public PassengerColor CurrentColor => AllDeparted ? PassengerColor.None : _colors[CurrentIndex];
        public bool IsCurrentFull => !AllDeparted && SeatsTaken == Capacity;

        
        public BusQueue(LevelData level)
        {
            if (level == null)
                throw new ArgumentNullException(nameof(level));

            Capacity = level.busCapacity;
            _colors = new PassengerColor[level.buses.Length];
            for (int i = 0; i < _colors.Length; i++)
            {
                if (!LevelFormat.TryParseColor(level.buses[i], out _colors[i]))
                    throw new ArgumentException($"Unknown bus color '{level.buses[i]}'. Was the level validated?");
            }
        }

        public PassengerColor GetColor(int busIndex) => _colors[busIndex];

        public bool CanBoard(PassengerColor color)
        {
            return !AllDeparted && !IsCurrentFull && color == CurrentColor;
        }

      
        public int Board(PassengerColor color)
        {
            if (!CanBoard(color))
                throw new InvalidOperationException($"A {color} passenger cannot board now.");

            return SeatsTaken++;
        }

       
        public void Depart()
        {
            if (!IsCurrentFull)
                throw new InvalidOperationException("Only a full bus can depart.");

            CurrentIndex++;
            SeatsTaken = 0;
        }
    }
}