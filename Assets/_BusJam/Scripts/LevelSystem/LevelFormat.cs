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
                case 'R': case 'r': color = PassengerColor.Red; return true;
                case 'B': case 'b': color = PassengerColor.Blue; return true;
                case 'G': case 'g': color = PassengerColor.Green; return true;
                case 'Y': case 'y': color = PassengerColor.Yellow; return true;
                case 'P': case 'p': color = PassengerColor.Purple; return true;
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
        // Küçük harf aynı rengin gizli yolcusu kurallar için ikisi aynı yolcu
        // Gizli olması sadece ekranda rengini saklamak demek
        public static bool IsHidden(char c) => char.IsLower(c) && TryParseColor(c, out _);

        public static char ToHiddenChar(PassengerColor color) => char.ToLowerInvariant(ToChar(color));

        public static bool IsKnownCell(char c)
        {
            return c == EmptyCell || c == VoidCell || c == ObstacleCell || TryParseColor(c, out _);
        }
    }
}