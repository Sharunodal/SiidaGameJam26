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

        private void Awake()
        {
            foreach (GameObject berry in berries)
            {
                berry.SetActive(false);
            }

            int activeBerryCount = Random.Range(minimumBerryCount, maximumBerryCount + 1);

            for (int index = 0; index < activeBerryCount; index++)
            {
                int selectedIndex = Random.Range(index, berries.Length);
                GameObject selectedBerry = berries[selectedIndex];
                berries[selectedIndex] = berries[index];
                berries[index] = selectedBerry;
                berries[index].SetActive(true);
            }
        }
    }
}
