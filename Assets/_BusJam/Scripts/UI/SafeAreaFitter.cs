using UnityEngine;

namespace BusJam.UI
{
    
    // Bu nesneyi ekranın güvenli bölgesine sığdırıyoruz HUD bunun içinde olduğu için hep görünür kalıyor
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rect;
        private Rect _applied;

        private void Awake()
        {
            _rect = (RectTransform)transform;
            Apply();
        }

        // Ekran döner ya da editörde Game penceresinin boyutu değişirse yeniden uyguluyoruz
        private void Update()
        {
            if (Screen.safeArea != _applied)
                Apply();
        }

        private void Apply()
        {
            Rect safe = Screen.safeArea;
            _applied = safe;

            // Piksel cinsinden güvenli bölgeyi 0 ile 1 arasındaki anchor değerlerine çeviriyoruz
            Vector2 min = safe.position;
            Vector2 max = safe.position + safe.size;
            min.x /= Screen.width;
            min.y /= Screen.height;
            max.x /= Screen.width;
            max.y /= Screen.height;

            _rect.anchorMin = min;
            _rect.anchorMax = max;
        }
    }
}