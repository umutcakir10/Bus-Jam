using BusJam.Core;
using UnityEngine;

namespace BusJam.Gameplay.Presentation
{
   
    public sealed class BoardLayout
    {
        private readonly BoardLayoutSettings _settings;

        public int Width { get; }
        public int Height { get; }
        public int SlotCount { get; }

        public BoardLayout(int width, int height, int slotCount, BoardLayoutSettings settings)
        {
            Width = width;
            Height = height;
            SlotCount = slotCount;
            _settings = settings;
        }

        public float CellSize => _settings.cellSize;
        public float SlotRowZ => _settings.slotGap;
        public float RoadZ => SlotRowZ + _settings.roadGap;
        public float RoadWidth => _settings.roadWidth;
        public float GroundMargin => _settings.groundMargin;

        public Vector3 BusStopPosition => new Vector3(0f, 0f, RoadZ);

        public Vector3 CellToWorld(GridPos cell)
        {
            float x = (cell.X - (Width - 1) * 0.5f) * _settings.cellSize;
            float z = -cell.Y * _settings.cellSize;
            return new Vector3(x, 0f, z);
        }

       
        public bool TryWorldToCell(Vector3 world, out GridPos cell)
        {
            int x = Mathf.RoundToInt(world.x / _settings.cellSize + (Width - 1) * 0.5f);
            int y = Mathf.RoundToInt(-world.z / _settings.cellSize);
            cell = new GridPos(x, y);
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public Vector3 SlotToWorld(int index)
        {
            float x = (index - (SlotCount - 1) * 0.5f) * _settings.slotSpacing;
            return new Vector3(x, 0f, SlotRowZ);
        }

       
        public Bounds GetPlayAreaBounds()
        {
            float halfGrid = Width * _settings.cellSize * 0.5f;
            float halfSlots = SlotCount * _settings.slotSpacing * 0.5f;
            float halfWidth = Mathf.Max(halfGrid, halfSlots);

            float minZ = -(Height - 0.5f) * _settings.cellSize;
            float maxZ = RoadZ + _settings.roadWidth * 0.5f;

            var bounds = new Bounds();
            bounds.SetMinMax(new Vector3(-halfWidth, 0f, minZ), new Vector3(halfWidth, 0f, maxZ));
            return bounds;
        }
    }
}