using UnityEngine;

namespace BusJam.Gameplay.Presentation
{
    
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFitter : MonoBehaviour
    {
        [SerializeField, Range(30f, 89f)] private float pitch = 60f;

        
        [SerializeField, Range(0f, 0.3f)] private float padding = 0.08f;

        
        [SerializeField] private float contentHeight = 1.5f;

        private Camera _camera;
        private Bounds _area;
        private bool _hasArea;
        private int _lastWidth;
        private int _lastHeight;

        private void Awake()
        {
            _camera = GetComponent<Camera>();

        }
        // Editor only: re-fit immediately when a value is changed in the Inspector during Play.
        private void OnValidate()
        {
            if (_hasArea && _camera != null)
                Apply();
        }

        public void Fit(Bounds playArea)
        {
            _area = playArea;
            _hasArea = true;
            Apply();
        }

        private void LateUpdate()
        {
          
            if (_hasArea && (Screen.width != _lastWidth || Screen.height != _lastHeight))
                Apply();
        }

        private void Apply()
        {
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;

            Quaternion rotation = Quaternion.Euler(pitch, 0f, 0f);
            Quaternion worldToCamera = Quaternion.Inverse(rotation);

            float tanVertical = Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * (1f - padding);
            float tanHorizontal = tanVertical * _camera.aspect;

            Vector3 extents = _area.extents;
            float distance = 0f;

            
            for (int i = 0; i < 8; i++)
            {
                var corner = new Vector3(
                    (i & 1) == 0 ? -extents.x : extents.x,
                    (i & 2) == 0 ? 0f : contentHeight,
                    (i & 4) == 0 ? -extents.z : extents.z);

                
                Vector3 local = worldToCamera * corner;

                float neededForWidth = Mathf.Abs(local.x) / tanHorizontal - local.z;
                float neededForHeight = Mathf.Abs(local.y) / tanVertical - local.z;
                distance = Mathf.Max(distance, neededForWidth, neededForHeight);
            }

            transform.rotation = rotation;
            transform.position = _area.center - rotation * Vector3.forward * distance;
        }
    }
}