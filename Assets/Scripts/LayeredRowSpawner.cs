using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SiidaGameJam.BerryPicking
{
    public sealed class LayeredRowSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject[] plantPrefabs;

        [Header("Rows")]
        [SerializeField] private float firstRowY = 1f;
        [Min(1)]
        [SerializeField] private int rowCount = 10;
        [Min(0.01f)]
        [SerializeField] private float rowSpacing = 1f;

        [Header("Horizontal Range")]
        [SerializeField] private float minimumX = -10f;
        [SerializeField] private float maximumX = 10f;
        [Min(0.01f)]
        [SerializeField] private float minimumHorizontalSpacing = 4f;
        [Min(0.01f)]
        [SerializeField] private float maximumHorizontalSpacing = 10f;

        [Header("Amount Per Row")]
        [Min(0)]
        [SerializeField] private int minimumPlantsPerRow;
        [Min(0)]
        [SerializeField] private int maximumPlantsPerRow = 10;

        private readonly List<List<GameObject>> spawnedRows =
            new List<List<GameObject>>();

        private void Start()
        {
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                spawnedRows.Add(SpawnRow(rowIndex));
            }
        }

        public void Advance()
        {
            List<GameObject> bottomRow = spawnedRows[spawnedRows.Count - 1];

            foreach (GameObject plant in bottomRow)
            {
                Destroy(plant);
            }

            spawnedRows.RemoveAt(spawnedRows.Count - 1);

            for (int rowIndex = 0; rowIndex < spawnedRows.Count; rowIndex++)
            {
                int newRowIndex = rowIndex + 1;

                foreach (GameObject plant in spawnedRows[rowIndex])
                {
                    plant.transform.position += Vector3.down * rowSpacing;
                    plant.GetComponent<SortingGroup>().sortingOrder = newRowIndex;
                }
            }

            spawnedRows.Insert(0, SpawnRow(0));
        }

        private List<GameObject> SpawnRow(int rowIndex)
        {
            List<GameObject> row = new List<GameObject>();
            float availableWidth = maximumX - minimumX;
            int capacity = Mathf.FloorToInt(availableWidth / minimumHorizontalSpacing) + 1;
            int allowedMaximum = Mathf.Min(maximumPlantsPerRow, capacity);
            int plantCount = Random.Range(minimumPlantsPerRow, allowedMaximum + 1);

            if (plantCount == 0)
            {
                return row;
            }

            float minimumRequiredWidth = (plantCount - 1) * minimumHorizontalSpacing;
            float currentX = Random.Range(minimumX, maximumX - minimumRequiredWidth);
            float y = firstRowY - rowIndex * rowSpacing;

            for (int plantIndex = 0; plantIndex < plantCount; plantIndex++)
            {
                row.Add(SpawnPlant(
                    new Vector3(currentX, y, transform.position.z),
                    rowIndex));

                int gapsRemaining = plantCount - plantIndex - 1;

                if (gapsRemaining == 0)
                {
                    continue;
                }

                float minimumWidthAfterNextGap =
                    (gapsRemaining - 1) * minimumHorizontalSpacing;
                float largestGapThatFits =
                    maximumX - currentX - minimumWidthAfterNextGap;
                float gap = Random.Range(
                    minimumHorizontalSpacing,
                    Mathf.Min(maximumHorizontalSpacing, largestGapThatFits));

                currentX += gap;
            }

            return row;
        }

        private GameObject SpawnPlant(Vector3 position, int rowIndex)
        {
            GameObject prefab = plantPrefabs[Random.Range(0, plantPrefabs.Length)];
            GameObject plant = Instantiate(prefab, position, Quaternion.identity, transform);
            plant.GetComponent<SortingGroup>().sortingOrder = rowIndex;
            return plant;
        }
    }
}
