using UnityEngine;

namespace SiidaGameJam.BerryPicking
{
    public sealed class BerryBush : MonoBehaviour
    {
        [SerializeField] private GameObject[] berries;
        [Min(0)]
        [SerializeField] private int minimumBerryCount;
        [Min(0)]
        [SerializeField] private int maximumBerryCount;
        [SerializeField] private RustlingBush rustlingBush;

        public int ActiveBerryCount { get; private set; }

        private void Awake()
        {
            rustlingBush.Prepare();

            foreach (GameObject berry in berries)
            {
                berry.GetComponent<Gatherable>().SetBerryBush(this);
                berry.SetActive(false);
            }

            ActiveBerryCount = Random.Range(
                minimumBerryCount,
                maximumBerryCount + 1);

            for (int index = 0; index < ActiveBerryCount; index++)
            {
                int selectedIndex = Random.Range(index, berries.Length);
                GameObject selectedBerry = berries[selectedIndex];
                berries[selectedIndex] = berries[index];
                berries[index] = selectedBerry;
                berries[index].SetActive(true);
            }
        }

        public void BerryWasGathered()
        {
            ActiveBerryCount -= 1;
            rustlingBush.RevealLemming();
        }
    }
}
