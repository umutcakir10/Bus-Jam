using System;
using BusJam.Core;

namespace BusJam.LevelSystem
{
  
    public static class LevelFormat
    {
        public const char EmptyCell = '.';
        public const char VoidCell = '#';
        public const char ObstacleCell = 'X';

        
        public static bool TryParseColor(char c, out PassengerColor color)
        {
            switch (c)
            {
                case 'R': color = PassengerColor.Red; return true;
                case 'B': color = PassengerColor.Blue; return true;
                case 'G': color = PassengerColor.Green; return true;
                case 'Y': color = PassengerColor.Yellow; return true;
                case 'P': color = PassengerColor.Purple; return true;
                default: color = PassengerColor.None; return false;
            }
        }

        public static char ToChar(PassengerColor color)
        {
            switch (color)
            {
                case PassengerColor.Red: return 'R';
                case PassengerColor.Blue: return 'B';
                case PassengerColor.Green: return 'G';
                case PassengerColor.Yellow: return 'Y';
                case PassengerColor.Purple: return 'P';
                default:
                    throw new ArgumentOutOfRangeException(nameof(color), color, "Color has no level-file letter.");
            }
        }

        public static bool IsKnownCell(char c)
        {
            return c == EmptyCell || c == VoidCell || c == ObstacleCell || TryParseColor(c, out _);
        }
    }
}