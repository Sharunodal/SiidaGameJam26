using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace SiidaGameJam.BerryPicking
{
    public enum GatherableResource
    {
        Berry,
        AzaleaFlower
    }

    public sealed class Gatherable : SceneInteractable
    {
        public static event Action<int> BerryGathered;
        public static event Action<int> AzaleaFlowerGathered;

        [SerializeField] private GatherableResource resource;
        [Min(1)]
        [SerializeField] private int amount = 1;
        [SerializeField] private UnityEvent<int> gathered;

        public override bool BeginInteraction(Vector2 pointerWorldPosition)
        {
            if (resource == GatherableResource.Berry)
            {
                if (!GatherBerry())
                {
                    return false;
                }
            }
            else
            {
                GatherAzaleaFlower();
            }

            gathered.Invoke(amount);
            gameObject.SetActive(false);
            return false;
        }

        private bool GatherBerry()
        {
            SortingGroup bushSortingGroup = GetComponentInParent<SortingGroup>();
            BerryBush berryBush = bushSortingGroup.GetComponentInChildren<BerryBush>();

            if (!berryBush.BerryCanBeGathered())
            {
                return false;
            }

            if (BerryGathered != null)
            {
                BerryGathered.Invoke(amount);
            }

            berryBush.BerryWasGathered();
            return true;
        }

        private void GatherAzaleaFlower()
        {
            if (AzaleaFlowerGathered != null)
            {
                AzaleaFlowerGathered.Invoke(amount);
            }
        }
    }
}
