using TMPro;
using UnityEngine;

namespace SiidaGameJam.BerryPicking
{
    public sealed class BerryCounter : MonoBehaviour
    {
        [SerializeField] private TMP_Text berriesPickedValue;

        public int BerriesPicked { get; private set; }

        private void Awake()
        {
            BerriesPicked = 0;
            UpdateDisplay();
        }

        private void OnEnable()
        {
            Gatherable.BerryGathered += AddBerries;
        }

        private void OnDisable()
        {
            Gatherable.BerryGathered -= AddBerries;
        }

        private void AddBerries(int amount)
        {
            BerriesPicked += amount;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            berriesPickedValue.text = BerriesPicked.ToString();
        }
    }
}
