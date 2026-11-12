using BusJam.Core;
using BusJam.Gameplay.Domain;
using UnityEngine;

namespace BusJam.Gameplay.Presentation
{
    
    public sealed class BoardView : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private Transform tilePrefab;
        [SerializeField] private Transform obstaclePrefab;
        [SerializeField] private Transform slotPrefab;
        [SerializeField] private Transform groundPrefab;
        [SerializeField] private Transform roadPrefab;

        [Header("Sizes")]
        [SerializeField] private float roadLength = 40f;

        private Transform _root;

        public void Build(BoardModel board, BoardLayout layout)
        {
            Clear();
            _root = new GameObject("BoardRoot").transform;
            _root.SetParent(transform, false);

            BuildGround(layout);
            BuildRoad(layout);
            BuildCells(board, layout);
            BuildSlots(layout);

            
            StaticBatchingUtility.Combine(_root.gameObject);
        }

        public void Clear()
        {
            if (_root == null)
                return;

            Destroy(_root.gameObject);
            _root = null;
        }

        private void BuildGround(BoardLayout layout)
        {
            Bounds area = layout.GetPlayAreaBounds();
            float margin = layout.GroundMargin * 2f;

            Transform ground = Spawn(groundPrefab, new Vector3(area.center.x, 0f, area.center.z), "Ground");
            ground.localScale = new Vector3(area.size.x + margin, 1f, area.size.z + margin);
        }

        private void BuildRoad(BoardLayout layout)
        {
            Transform road = Spawn(roadPrefab, layout.BusStopPosition, "Road");
            road.localScale = new Vector3(roadLength, 1f, layout.RoadWidth);
        }

        private void BuildCells(BoardModel board, BoardLayout layout)
        {
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    var cell = new GridPos(x, y);
                    CellType type = board.GetCell(cell);
                    if (type == CellType.Void)
                        continue;

                    Transform prefab = type == CellType.Obstacle ? obstaclePrefab : tilePrefab;
                    Spawn(prefab, layout.CellToWorld(cell), $"Cell {x},{y}");
                }
            }
        }

        private void BuildSlots(BoardLayout layout)
        {
            for (int i = 0; i < layout.SlotCount; i++)
                Spawn(slotPrefab, layout.SlotToWorld(i), $"Slot {i}");
        }

        private Transform Spawn(Transform prefab, Vector3 position, string objectName)
        {
            Transform instance = Instantiate(prefab, position, Quaternion.identity, _root);
            instance.name = objectName;
            return instance;
        }
    }
}
