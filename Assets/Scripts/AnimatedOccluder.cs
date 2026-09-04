using UnityEngine;

namespace SiidaGameJam.BerryPicking
{
    public sealed class AnimatedOccluder : SceneInteractable
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Collider2D blockingCollider;
        [SerializeField] private string pushAsideTriggerName = "PushAside";

        private bool hasBeenPushedAside;

        public override bool BeginInteraction(Vector2 pointerWorldPosition)
        {
            if (hasBeenPushedAside)
            {
                return false;
            }

            hasBeenPushedAside = true;
            animator.SetTrigger(pushAsideTriggerName);
            return false;
        }

        public void StopBlocking()
        {
            blockingCollider.enabled = false;
        }
    }
}
