using System.Collections.Generic;
using BusJam.Core;
using UnityEngine;

namespace BusJam.Gameplay.Presentation
{
    
    public sealed class BusView : MonoBehaviour
    {
        [SerializeField] private PassengerColor color;
        [SerializeField] private Transform[] seats;

        public PassengerColor Color => color;
        public int SeatCount => seats.Length;

        public Transform GetSeat(int index) => seats[index];

        public void Place(Vector3 position)
        {
            transform.position = position;
        }

        
        private void Reset()
        {
            var found = new List<Transform>();
            foreach (Transform t in GetComponentsInChildren<Transform>(true))
            {
                if (t.name.StartsWith("SeatAnchor_"))
                    found.Add(t);
            }
            found.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            seats = found.ToArray();
        }
    }
}
