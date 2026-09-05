using UnityEngine;

namespace SiidaGameJam.BerryPicking
{
    public sealed class RustlingBush : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform rustlingTransform;
        [SerializeField] private GameObject lemmingObject;

        [Header("Timing")]
        [SerializeField] private float minimumRestDuration = 1.5f;
        [SerializeField] private float maximumRestDuration = 4f;
        [SerializeField] private float rustleDuration = 0.4f;
        [SerializeField] private int oscillationsPerRustle = 3;

        [Header("Movement")]
        [SerializeField] private float horizontalMovement = 0.025f;
        [SerializeField] private float rotationAmount = 0.75f;

        private bool hasBeenActivated;
        private bool rustleIsPlaying;
        private bool lemmingHasBeenRevealed;
        private float timeUntilNextRustle;
        private float rustleElapsedTime;
        private Vector3 appliedPositionOffset;
        private float appliedRotationOffset;

        public void Prepare()
        {
            lemmingObject.SetActive(false);
        }

        public void ActivateRustling()
        {
            hasBeenActivated = true;
            timeUntilNextRustle = Random.Range(
                minimumRestDuration,
                maximumRestDuration);
        }

        public void RevealLemming()
        {
            if (!hasBeenActivated || lemmingHasBeenRevealed)
            {
                return;
            }

            lemmingHasBeenRevealed = true;
            lemmingObject.SetActive(true);
        }

        private void Update()
        {
            if (!hasBeenActivated)
            {
                return;
            }

            if (!rustleIsPlaying)
            {
                timeUntilNextRustle -= Time.deltaTime;

                if (timeUntilNextRustle <= 0f)
                {
                    rustleIsPlaying = true;
                    rustleElapsedTime = 0f;
                }

                return;
            }

            rustleElapsedTime += Time.deltaTime;

            float progress = Mathf.Clamp01(rustleElapsedTime / rustleDuration);
            float wave = Mathf.Sin(
                progress * Mathf.PI * 2f * oscillationsPerRustle);
            float fade = Mathf.Sin(progress * Mathf.PI);
            ApplyRustleAmount(wave * fade);

            if (progress >= 1f)
            {
                ApplyRustleAmount(0f);
                rustleIsPlaying = false;
                timeUntilNextRustle = Random.Range(
                    minimumRestDuration,
                    maximumRestDuration);
            }
        }

        private void OnDisable()
        {
            ApplyRustleAmount(0f);
        }

        private void ApplyRustleAmount(float amount)
        {
            Vector3 newPositionOffset = Vector3.right * horizontalMovement * amount;
            float newRotationOffset = rotationAmount * amount;

            rustlingTransform.localPosition +=
                newPositionOffset - appliedPositionOffset;
            rustlingTransform.Rotate(
                0f,
                0f,
                newRotationOffset - appliedRotationOffset,
                Space.Self);

            appliedPositionOffset = newPositionOffset;
            appliedRotationOffset = newRotationOffset;
        }
    }
}
