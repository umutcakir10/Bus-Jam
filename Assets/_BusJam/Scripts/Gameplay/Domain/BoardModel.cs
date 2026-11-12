using System;
using BusJam.Core;
using BusJam.LevelSystem;

namespace BusJam.Gameplay.Domain
{
    
    public sealed class BoardModel
    {
        public int Width { get; }
        public int Height { get; }
        public int CellCount => Width * Height;
        public int PassengerCount { get; private set; }

        
        private readonly CellType[] _cells;
        private readonly PassengerColor[] _colors;

        
        public BoardModel(LevelData level)
        {
            if (level == null)
                throw new ArgumentNullException(nameof(level));

            Width = level.Width;
            Height = level.Height;
            _cells = new CellType[CellCount];
            _colors = new PassengerColor[CellCount];

            for (int y = 0; y < Height; y++)
            {
                string row = level.rows[y];
                for (int x = 0; x < Width; x++)
                {
                    int i = y * Width + x;
                    char c = row[x];

                    if (LevelFormat.TryParseColor(c, out PassengerColor color))
                    {
                        _cells[i] = CellType.Passenger;
                        _colors[i] = color;
                        PassengerCount++;
                    }
                    else if (c == LevelFormat.EmptyCell)
                        _cells[i] = CellType.Empty;
                    else if (c == LevelFormat.ObstacleCell)
                        _cells[i] = CellType.Obstacle;
                    else if (c == LevelFormat.VoidCell)
                        _cells[i] = CellType.Void;
                    else
                        throw new ArgumentException($"Unknown cell '{c}' at {new GridPos(x, y)}. Was the level validated?");
                }
            }
        }

        public bool IsInside(GridPos p) => p.X >= 0 && p.X < Width && p.Y >= 0 && p.Y < Height;

        public int ToIndex(GridPos p) => p.Y * Width + p.X;

        public GridPos ToPos(int index) => new GridPos(index % Width, index / Width);

        public CellType GetCell(GridPos p) => _cells[ToIndex(p)];

        public PassengerColor GetColor(GridPos p) => _colors[ToIndex(p)];

   
        public bool IsWalkable(GridPos p) => IsInside(p) && _cells[ToIndex(p)] == CellType.Empty;

    
        public void RemovePassenger(GridPos p)
        {
            int i = ToIndex(p);
            if (_cells[i] != CellType.Passenger)
                throw new InvalidOperationException($"No passenger at {p}.");

            _cells[i] = CellType.Empty;
            _colors[i] = PassengerColor.None;
            PassengerCount--;
        }
    }
}