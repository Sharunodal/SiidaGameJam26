using UnityEngine;

namespace SiidaGameJam.BerryPicking
{
    public abstract class SceneInteractable : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer visual;
        [SerializeField] private int interactionPriority;

        public int SortingLayerValue => SortingLayer.GetLayerValueFromID(visual.sortingLayerID);
        public int SortingOrder => visual.sortingOrder;
        public int InteractionPriority => interactionPriority;

        public abstract bool BeginInteraction(Vector2 pointerWorldPosition);

        public virtual void ContinueInteraction(Vector2 pointerWorldPosition)
        {
        }

        public virtual void EndInteraction(Vector2 pointerWorldPosition)
        {
        }
    }
}
