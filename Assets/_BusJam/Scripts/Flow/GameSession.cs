using BusJam.LevelSystem;
using BusJam.Save;

namespace BusJam.Flow
{
    // Sahne yeniden yüklense de kaybolmayan oturum bilgisi
    // Oyun açılınca kayıttan dolduruluyor ve sonrasında da ilerleme olunca kayda yazılıyor
    public static class GameSession
    {
        private static bool _loaded;

        public static int LevelIndex { get; set; }

        // Start ekranı sadece oyun ilk açıldığında çıkıyor 
        public static bool StartScreenShown { get; set; }

        public static void EnsureLoaded()
        {
            if (_loaded)
                return;

            _loaded = true;
            LevelIndex = IndexOf(SaveSystem.Load().currentLevelId);
        }

        public static void SaveProgress(int levelIndex)
        {
            SaveSystem.Save(new SaveData { currentLevelId = LevelCatalog.Ids[levelIndex] });
        }

        // Kayıttaki level artık yoksa silinmiş de olabilir,  baştan başlıyoruz
        private static int IndexOf(string levelId)
        {
            if (string.IsNullOrEmpty(levelId))
                return 0;

            for (int i = 0; i < LevelCatalog.Count; i++)
            {
                if (LevelCatalog.Ids[i] == levelId)
                    return i;
            }
            return 0;
        }
    }
}