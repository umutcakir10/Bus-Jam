using UnityEngine;

namespace BusJam.Flow
{
    // Bilgisayarda oyun telefon oranında bir pencerede açılıyor
    // Pencere ekrandan uzunsa ya da yataysa arayüz bozuluyor
    // Açılışta bir kere bakıp pencereyi telefon oranına ve ekran yüksekliğine sığdırıyoruz
    public static class WindowSetup
    {
        private const float PhoneAspect = 9f / 19.5f;
        private const float MaxScreenShare = 0.85f;

        // Bu etiket sayesinde Unity metodu oyun açılınca kendisi çağırıyor, sahneye nesne koymaya gerek yok
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void FitWindowToScreen()
        {
#if UNITY_STANDALONE && !UNITY_EDITOR
            // Önceki açılıştan büyütülmüş ya da yatay bir boyut kalmış olsa bile düzeltiyoruz
            int maxHeight = Mathf.RoundToInt(Screen.currentResolution.height * MaxScreenShare);
            int height = Mathf.Min(Screen.height, maxHeight);
            int width = Mathf.RoundToInt(height * PhoneAspect);

            if (Screen.width != width || Screen.height != height || Screen.fullScreenMode != FullScreenMode.Windowed)
                Screen.SetResolution(width, height, FullScreenMode.Windowed);
#endif
        }
    }
}