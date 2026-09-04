using UnityEngine;
using UnityEngine.Events;

namespace SiidaGameJam.BerryPicking
{
    public sealed class Gatherable : SceneInteractable
    {
        [Min(1)]
        [SerializeField] private int amount = 1;
        [SerializeField] private UnityEvent<int> gathered;

        public override bool BeginInteraction(Vector2 pointerWorldPosition)
        {
            gathered.Invoke(amount);
            gameObject.SetActive(false);
            return false;
        }
    }
}
