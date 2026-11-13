using BusJam.Core;
using UnityEngine;

namespace BusJam.Gameplay.Presentation
{
    
    public sealed class PassengerView : MonoBehaviour
    {
        
        private static readonly int IsWalkingId = Animator.StringToHash("IsWalking");
        private static readonly int JumpId = Animator.StringToHash("Jump");

        [SerializeField] private PassengerColor color;
        [SerializeField] private Animator animator;

        public PassengerColor Color => color;

        public void Place(Vector3 position, Vector3 facing)
        {
            transform.SetPositionAndRotation(position, Quaternion.LookRotation(facing));
        }

        public void SetWalking(bool walking)
        {
            animator.SetBool(IsWalkingId, walking);
        }

        public void PlayJump()
        {
            animator.SetTrigger(JumpId);
        }

       
        public void ResetForReuse()
        {
            animator.SetBool(IsWalkingId, false);
            animator.ResetTrigger(JumpId);
        }

      
        private void Reset()
        {
            animator = GetComponentInChildren<Animator>();
        }
    }
}