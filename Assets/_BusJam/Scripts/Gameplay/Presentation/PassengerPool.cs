using System;
using System.Collections.Generic;
using BusJam.Core;
using UnityEngine;

namespace BusJam.Gameplay.Presentation
{
    
    public sealed class PassengerPool : MonoBehaviour
    {
        
        private const int ColorSlotCount = (int)PassengerColor.Purple + 1;

        [SerializeField] private PassengerView[] prefabs;
        [SerializeField] private int prewarmPerColor = 6;

        private readonly PassengerView[] _prefabByColor = new PassengerView[ColorSlotCount];
        private readonly Stack<PassengerView>[] _shelves = new Stack<PassengerView>[ColorSlotCount];

        private void Awake()
        {
            foreach (PassengerView prefab in prefabs)
            {
                int i = (int)prefab.Color;
                if (_prefabByColor[i] != null)
                {
                    Debug.LogError($"Two passenger prefabs are set to {prefab.Color}.", this);
                    continue;
                }

                _prefabByColor[i] = prefab;
                _shelves[i] = new Stack<PassengerView>(prewarmPerColor);
                for (int n = 0; n < prewarmPerColor; n++)
                    _shelves[i].Push(Create(prefab));
            }
        }

        public PassengerView Get(PassengerColor color)
        {
            Stack<PassengerView> shelf = _shelves[(int)color];
            if (shelf == null)
                throw new InvalidOperationException($"No passenger prefab for {color}.");

       
            PassengerView view = shelf.Count > 0 ? shelf.Pop() : Create(_prefabByColor[(int)color]);
            view.gameObject.SetActive(true);
            return view;
        }

        public void Release(PassengerView view)
        {
            view.ResetForReuse();
            view.gameObject.SetActive(false);
            view.transform.SetParent(transform, false);
            _shelves[(int)view.Color].Push(view);
        }

        private PassengerView Create(PassengerView prefab)
        {
            PassengerView view = Instantiate(prefab, transform);
            view.gameObject.SetActive(false);
            return view;
        }
    }
}