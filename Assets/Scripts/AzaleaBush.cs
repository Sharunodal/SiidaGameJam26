using UnityEngine;

namespace SiidaGameJam.BerryPicking
{
    public sealed class AzaleaBush : MonoBehaviour
    {
        [SerializeField] private GameObject[] flowers;
        [Min(0)]
        [SerializeField] private int minimumFlowerCount;
        [Min(0)]
        [SerializeField] private int maximumFlowerCount;

        private void Awake()
        {
            foreach (GameObject flower in flowers)
            {
                flower.SetActive(false);
            }

            int activeFlowerCount = Random.Range(
                minimumFlowerCount,
                maximumFlowerCount + 1);

            for (int index = 0; index < activeFlowerCount; index++)
            {
                int selectedIndex = Random.Range(index, flowers.Length);
                GameObject selectedFlower = flowers[selectedIndex];
                flowers[selectedIndex] = flowers[index];
                flowers[index] = selectedFlower;
                flowers[index].SetActive(true);
            }
        }
    }
}
