using System;

namespace BusJam.LevelSystem
{
    
    [Serializable]
    public sealed class LevelData
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public string id;
        public string displayName;

        public int timeLimitSeconds = 60;
        public int busCapacity = 3;
        public int waitingSlotCount = 5;

        // rows[0] is the front row (closest to the waiting slots).
        public string[] rows;

        // Bus arrival order, one color letter per bus. Example: "RBR".
        public string buses;

        public int Height => rows == null ? 0 : rows.Length;
        public int Width => Height == 0 || rows[0] == null ? 0 : rows[0].Length;
    }
}