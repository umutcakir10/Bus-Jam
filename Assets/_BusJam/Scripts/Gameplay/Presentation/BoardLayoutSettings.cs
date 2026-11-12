using System;

namespace BusJam.Gameplay.Presentation
{
   
    [Serializable]
    public sealed class BoardLayoutSettings
    {
        public float cellSize = 1f;
        public float slotSpacing = 1f;

        
        public float slotGap = 1.5f;

        
        public float roadGap = 2f;

        public float roadWidth = 2.5f;

      
        public float groundMargin = 1.5f;
    }
}
