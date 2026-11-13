using BusJam.Core;
using UnityEngine;

namespace BusJam.Gameplay.Presentation
{
  
    public sealed class TapInput : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;

       
        [SerializeField] private float tapHeight = 0.5f;

        public float TapHeight => tapHeight;

        public bool TryGetTappedCell(BoardLayout layout, out GridPos cell)
        {
            cell = default;
            return TryGetTapRay(out Ray ray) && TryRayToCell(ray, tapHeight, layout, out cell);
        }

        // Bu karede bir dokunuş varsa kameradan o noktaya giden ışını veriyor
        public bool TryGetTapRay(out Ray ray)
        {
            ray = default;
            if (!Input.GetMouseButtonDown(0))
                return false;

            ray = worldCamera.ScreenPointToRay(Input.mousePosition);
            return true;
        }

        // Işını verilen yükseklikteki düz bir zeminle kesip o noktanın hücresini buluyor
        public static bool TryRayToCell(Ray ray, float height, BoardLayout layout, out GridPos cell)
        {
            cell = default;
            var plane = new Plane(Vector3.up, new Vector3(0f, height, 0f));
            if (!plane.Raycast(ray, out float distance))
                return false;

            return layout.TryWorldToCell(ray.GetPoint(distance), out cell);
        }
    }
}