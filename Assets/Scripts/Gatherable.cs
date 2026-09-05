using System;
using UnityEngine;
using UnityEngine.Events;

namespace SiidaGameJam.BerryPicking
{
    public sealed class Gatherable : SceneInteractable
    {
        public static event Action<int> AnyGathered;

        [Min(1)]
        [SerializeField] private int amount = 1;
        [SerializeField] private UnityEvent<int> gathered;
        private BerryBush berryBush;

        public void SetBerryBush(BerryBush owner)
        {
            berryBush = owner;
        }

        public override bool BeginInteraction(Vector2 pointerWorldPosition)
        {
            if (AnyGathered != null)
            {
                AnyGathered.Invoke(amount);
            }

            gathered.Invoke(amount);
            gameObject.SetActive(false);
            berryBush.BerryWasGathered();
            return false;
        }
    }
}
