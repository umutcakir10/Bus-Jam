using System;

namespace BusJam.Gameplay.Domain
{
    // Level süresi
    // Zamanı kendisi ölçmüyor, her kare ne kadar geçtiğini dışarıdan alıyor
    // Böylece testte "10 saniye geçti" diyebiliyoruz gerçekten 10 saniye beklemeden
    public sealed class LevelTimer
    {
        public float Duration { get; }
        public float Remaining { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsExpired => Remaining <= 0f;

        public LevelTimer(float durationSeconds)
        {
            if (durationSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));

            Duration = durationSeconds;
            Remaining = durationSeconds;
        }

        public void Start() => IsRunning = !IsExpired;

        public void Stop() => IsRunning = false;

        // Süre tam bu karede bittiyse true döner sonraki karelerde artık false
        public bool Tick(float deltaTime)
        {
            if (!IsRunning)
                return false;

            Remaining -= deltaTime;
            if (Remaining > 0f)
                return false;

            Remaining = 0f;
            IsRunning = false;
            return true;
        }
    }
}