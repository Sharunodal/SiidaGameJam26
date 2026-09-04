using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace SiidaGameJam.BerryPicking
{
    public sealed class LayeredRowSpawner : MonoBehaviour
    {
        [Header("Berry Bushes")]
        [FormerlySerializedAs("plantPrefabs")]
        [SerializeField] private GameObject[] berryBushPrefabs;

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

        [Header("Birches")]
        [SerializeField] private GameObject[] birchPrefabs;
        [Range(0f, 1f)]
        [SerializeField] private float birchSpawnChance = 0.5f;
        [Min(0)]
        [SerializeField] private int maximumBirchesPerRow = 1;
        [Min(0.01f)]
        [SerializeField] private float minimumDistanceFromBirch = 3.5f;
        [SerializeField] private float birchForbiddenMinimumX = -4f;
        [SerializeField] private float birchForbiddenMaximumX = 4f;

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
            List<float> birchPositions = new List<float>();
            float y = firstRowY - rowIndex * rowSpacing;

            SpawnBirches(row, birchPositions, y, rowIndex);

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

            for (int plantIndex = 0; plantIndex < plantCount; plantIndex++)
            {
                currentX = MovePastBirches(currentX, birchPositions);

                if (currentX > maximumX)
                {
                    break;
                }

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

                if (largestGapThatFits < minimumHorizontalSpacing)
                {
                    break;
                }

                float gap = Random.Range(
                    minimumHorizontalSpacing,
                    Mathf.Min(maximumHorizontalSpacing, largestGapThatFits));

                currentX += gap;
            }

            return row;
        }

        private GameObject SpawnPlant(Vector3 position, int rowIndex)
        {
            GameObject prefab = berryBushPrefabs[Random.Range(0, berryBushPrefabs.Length)];
            GameObject plant = Instantiate(prefab, position, Quaternion.identity, transform);
            plant.GetComponent<SortingGroup>().sortingOrder = rowIndex;
            return plant;
        }

        private void SpawnBirches(
            List<GameObject> row,
            List<float> birchPositions,
            float y,
            int rowIndex)
        {
            for (int birchIndex = 0; birchIndex < maximumBirchesPerRow; birchIndex++)
            {
                if (Random.value > birchSpawnChance)
                {
                    continue;
                }

                TrySpawnBirch(row, birchPositions, y, rowIndex);
            }

            birchPositions.Sort();
        }

        private void TrySpawnBirch(
            List<GameObject> row,
            List<float> birchPositions,
            float y,
            int rowIndex)
        {
            const int maximumAttempts = 30;

            for (int attempt = 0; attempt < maximumAttempts; attempt++)
            {
                float x = Random.Range(minimumX, maximumX);

                if (x >= birchForbiddenMinimumX && x <= birchForbiddenMaximumX)
                {
                    continue;
                }

                if (!PositionIsClearOfBirches(x, birchPositions))
                {
                    continue;
                }

                GameObject prefab = birchPrefabs[Random.Range(0, birchPrefabs.Length)];
                GameObject birch = Instantiate(
                    prefab,
                    new Vector3(x, y, transform.position.z),
                    Quaternion.identity,
                    transform);

                birch.GetComponent<SortingGroup>().sortingOrder = rowIndex;
                row.Add(birch);
                birchPositions.Add(x);
                return;
            }
        }

        private bool PositionIsClearOfBirches(
            float x,
            List<float> birchPositions)
        {
            foreach (float birchPosition in birchPositions)
            {
                float distance = Mathf.Abs(x - birchPosition);

                if (distance < minimumDistanceFromBirch)
                {
                    return false;
                }
            }

            return true;
        }

        private float MovePastBirches(float x, List<float> birchPositions)
        {
            foreach (float birchPosition in birchPositions)
            {
                float distance = Mathf.Abs(x - birchPosition);

                if (distance < minimumDistanceFromBirch)
                {
                    x = birchPosition + minimumDistanceFromBirch;
                }
            }

            return x;
        }
    }
}
