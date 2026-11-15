using UnityEngine;

namespace BusJam.Gameplay.Presentation
{
    // Kaldırımları ve yol çizgilerini her levelda board'un kenarlarına taşıyor
    // Level boyutu değiştiği için sabit bir yere koysaydık büyük levelda gridin üstüne binerdi
    public sealed class EnvironmentLayout : MonoBehaviour
    {
        [SerializeField] private Transform roadDecor;
        [SerializeField] private Transform topStrip;
        [SerializeField] private Transform bottomStrip;

        [Header("Yol")]
        // Çizgilerin yolun üstünde görünmesi için yol yüzeyinin yüksekliği
        [SerializeField] private float roadSurfaceHeight = 0.05f;
        [SerializeField] private Transform dashPrefab;
        [SerializeField] private int dashCount = 20;
        [SerializeField] private float dashSpacing = 2f;

        [Header("Kaldırımlar")]
        [SerializeField] private float topGap = 1f;
        [SerializeField] private float bottomGap = 1f;
        private BoardLayout _layout;

        private void Awake()
        {
            // Kesik çizgileri elle tek tek koymak yerine bir kere burada diziyoruz
            float start = -dashSpacing * (dashCount - 1) * 0.5f;
            for (int i = 0; i < dashCount; i++)
            {
                Transform dash = Instantiate(dashPrefab, roadDecor);
                dash.localPosition = new Vector3(start + i * dashSpacing, 0f, 0f);
            }
        }

        public void Place(BoardLayout layout)
        {
            _layout = layout;
            Bounds area = layout.GetPlayAreaBounds();
            Vector3 stop = layout.BusStopPosition;
            float x = area.center.x;

            roadDecor.position = new Vector3(x, roadSurfaceHeight, stop.z);
            topStrip.position = new Vector3(x, 0f, area.max.z + topGap);
            bottomStrip.position = new Vector3(x, 0f, area.min.z - bottomGap);
        }

        
        // Play modundaysak kaldırımları hemen yeniden yerleştiriyoruz ayar yaparken sonucu anında görüyoruz
        private void OnValidate()
        {
            if (Application.isPlaying && _layout != null)
                Place(_layout);
        }
    }
}