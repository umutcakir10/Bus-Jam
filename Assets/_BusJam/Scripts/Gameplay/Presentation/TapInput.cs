using BusJam.Core;
using UnityEngine;

namespace BusJam.Gameplay.Presentation
{
    
    public sealed class TapInput : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;

        
        [SerializeField] private float tapHeight = 0.5f;

        public bool TryGetTappedCell(BoardLayout layout, out GridPos cell)
        {
            cell = default;
            if (!Input.GetMouseButtonDown(0))
                return false;

            Ray ray = worldCamera.ScreenPointToRay(Input.mousePosition);
            var plane = new Plane(Vector3.up, new Vector3(0f, tapHeight, 0f));
            if (!plane.Raycast(ray, out float distance))
                return false;

            return layout.TryWorldToCell(ray.GetPoint(distance), out cell);
        }
    }
}