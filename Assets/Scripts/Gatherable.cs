using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace SiidaGameJam.BerryPicking
{
    public sealed class Gatherable : SceneInteractable
    {
        public static event Action<int> AnyGathered;

        [Min(1)]
        [SerializeField] private int amount = 1;
        [SerializeField] private UnityEvent<int> gathered;

        public override bool BeginInteraction(Vector2 pointerWorldPosition)
        {
            SortingGroup bushSortingGroup = GetComponentInParent<SortingGroup>();
            BerryBush berryBush = bushSortingGroup.GetComponentInChildren<BerryBush>();

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
