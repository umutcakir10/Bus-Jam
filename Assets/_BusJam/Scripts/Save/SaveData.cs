using System;

namespace BusJam.Save
{
    
    // Level'ı sıra numarasıyla değil adıyla tutuyoruz
    // Level Editor araya yeni bir level eklerse sıra numaraları kayar ama adı aynı kalır
    [Serializable]
    public sealed class SaveData
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public string currentLevelId;
    }
}
