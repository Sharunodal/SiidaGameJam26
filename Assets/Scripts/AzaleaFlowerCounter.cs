using TMPro;
using UnityEngine;

namespace SiidaGameJam.BerryPicking
{
    public sealed class AzaleaFlowerCounter : MonoBehaviour
    {
        [SerializeField] private TMP_Text azaleaFlowersPickedValue;

        public int AzaleaFlowersPicked { get; private set; }

        private void Awake()
        {
            AzaleaFlowersPicked = 0;
            UpdateDisplay();
        }

        private void OnEnable()
        {
            Gatherable.AzaleaFlowerGathered += AddAzaleaFlowers;
        }

        private void OnDisable()
        {
            Gatherable.AzaleaFlowerGathered -= AddAzaleaFlowers;
        }

        private void AddAzaleaFlowers(int amount)
        {
            AzaleaFlowersPicked += amount;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            azaleaFlowersPickedValue.text = AzaleaFlowersPicked.ToString();
        }
    }
}
