using DG.Tweening;
using UnityEngine;

namespace BusJam.Flow
{
    // DOTween'i oyun açılırken bir kere hazırlıyoruz
    public static class TweenSetup
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            // Safe mode açık: yok edilmiş bir nesnenin tween'i hata vermek yerine sessizce duruyor
            // Kapasiteyi baştan veriyoruz, oyun sırasında DOTween kendi listesini büyütüp bellek ayırmasın
            DOTween.Init(recycleAllByDefault: false, useSafeMode: true, logBehaviour: LogBehaviour.ErrorsOnly)
                .SetCapacity(200, 50);
        }
    }
}