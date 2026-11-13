namespace BusJam.Flow
{
    // Sahne yeniden yüklense de kaybolmayan oturum bilgisi
    // Static alanlar sahneye bağlı değil oyun açık kaldığı sürece yaşıyor
    
    public static class GameSession
    {
        public static int LevelIndex { get; set; }
    }
}