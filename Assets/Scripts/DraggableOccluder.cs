using UnityEngine;

namespace SiidaGameJam.BerryPicking
{
    public sealed class DraggableOccluder : SceneInteractable
    {
        [Min(0f)]
        [SerializeField] private float maximumDistanceFromStart = 1.5f;

        private Vector2 startingPosition;
        private Vector2 pointerOffset;

        private void Awake()
        {
            startingPosition = transform.position;
        }

        public override bool BeginInteraction(Vector2 pointerWorldPosition)
        {
            pointerOffset = (Vector2)transform.position - pointerWorldPosition;
            return true;
        }

        public override void ContinueInteraction(Vector2 pointerWorldPosition)
        {
            Vector2 requestedPosition = pointerWorldPosition + pointerOffset;
            Vector2 displacement = Vector2.ClampMagnitude(
                requestedPosition - startingPosition,
                maximumDistanceFromStart);

            Vector2 constrainedPosition = startingPosition + displacement;
            transform.position = new Vector3(
                constrainedPosition.x,
                constrainedPosition.y,
                transform.position.z);
        }
    }
}
