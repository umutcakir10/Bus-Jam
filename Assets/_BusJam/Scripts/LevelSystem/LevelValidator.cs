using System.Collections.Generic;
using BusJam.Core;

namespace BusJam.LevelSystem
{
    
    public static class LevelValidator
    {
        public const int MaxWidth = 10;
        public const int MaxHeight = 12;
        public const int MaxWaitingSlots = 7;
        public const int MaxTimeLimitSeconds = 600;

     
        private const int ColorSlotCount = (int)PassengerColor.Purple + 1;

        public static bool Validate(LevelData level, List<string> errors)
        {
            errors.Clear();

            if (level == null)
            {
                errors.Add("Level data is missing (file is empty or not valid JSON).");
                return false;
            }

            ValidateSettings(level, errors);
            bool gridOk = ValidateGrid(level, errors);
            bool busesOk = ValidateBuses(level, errors);

          
            if (gridOk && busesOk && level.busCapacity > 0)
            {
                ValidateColorCounts(level, errors);
            }

            return errors.Count == 0;
        }

        private static void ValidateSettings(LevelData level, List<string> errors)
        {
            if (level.version != LevelData.CurrentVersion)
                errors.Add($"Unsupported version {level.version} (expected {LevelData.CurrentVersion}).");

            if (string.IsNullOrWhiteSpace(level.id))
                errors.Add("Level id is empty.");

            if (level.timeLimitSeconds < 1 || level.timeLimitSeconds > MaxTimeLimitSeconds)
                errors.Add($"Time limit must be 1-{MaxTimeLimitSeconds} seconds (was {level.timeLimitSeconds}).");

            if (level.busCapacity < 1)
                errors.Add($"Bus capacity must be at least 1 (was {level.busCapacity}).");

            if (level.waitingSlotCount < 1 || level.waitingSlotCount > MaxWaitingSlots)
                errors.Add($"Waiting slot count must be 1-{MaxWaitingSlots} (was {level.waitingSlotCount}).");
        }

        private static bool ValidateGrid(LevelData level, List<string> errors)
        {
            int height = level.Height;
            if (height == 0)
            {
                errors.Add("Grid has no rows.");
                return false;
            }
            if (height > MaxHeight)
            {
                errors.Add($"Grid height must be 1-{MaxHeight} (was {height}).");
                return false;
            }

            int width = level.Width;
            if (width == 0 || width > MaxWidth)
            {
                errors.Add($"Grid width must be 1-{MaxWidth} (row 0 has {width} cells).");
                return false;
            }

            bool ok = true;
            for (int y = 0; y < height; y++)
            {
                string row = level.rows[y];
                int length = row == null ? 0 : row.Length;
                if (length != width)
                {
                    errors.Add($"Row {y} has {length} cells, expected {width}.");
                    ok = false;
                    continue;
                }

                for (int x = 0; x < width; x++)
                {
                    char c = row[x];
                    if (!LevelFormat.IsKnownCell(c))
                    {
                        errors.Add($"Row {y}, column {x}: unknown cell '{c}'.");
                        ok = false;
                    }
                }
            }
            return ok;
        }

        private static bool ValidateBuses(LevelData level, List<string> errors)
        {
            if (string.IsNullOrEmpty(level.buses))
            {
                errors.Add("Level has no buses.");
                return false;
            }

            bool ok = true;
            for (int i = 0; i < level.buses.Length; i++)
            {
                char c = level.buses[i];
                if (!LevelFormat.TryParseColor(c, out _))
                {
                    errors.Add($"Bus {i}: unknown color '{c}'.");
                    ok = false;
                }
            }
            return ok;
        }

        private static void ValidateColorCounts(LevelData level, List<string> errors)
        {
            var passengerCounts = new int[ColorSlotCount];
            var busCounts = new int[ColorSlotCount];

            foreach (string row in level.rows)
            {
                foreach (char c in row)
                {
                    if (LevelFormat.TryParseColor(c, out PassengerColor color))
                        passengerCounts[(int)color]++;
                }
            }

            foreach (char c in level.buses)
            {
                if (LevelFormat.TryParseColor(c, out PassengerColor color))
                    busCounts[(int)color]++;
            }

            for (int i = 1; i < ColorSlotCount; i++)
            {
                int expected = busCounts[i] * level.busCapacity;
                if (passengerCounts[i] != expected)
                {
                    var color = (PassengerColor)i;
                    errors.Add($"{color}: {passengerCounts[i]} passengers, but {busCounts[i]} bus(es) x {level.busCapacity} seats = {expected}.");
                }
            }
        }
    }
}