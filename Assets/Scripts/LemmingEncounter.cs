using System;
using UnityEngine;

namespace SiidaGameJam.BerryPicking
{
    public sealed class LemmingEncounter : SceneInteractable
    {
        public static event Action EncounterStarted;
        public static event Action GameOverRequested;

        [Header("Jump")]
        [SerializeField] private GameObject exclamationMark;
        [SerializeField] private SpriteRenderer exclamationMarkVisual;
        [Min(0.01f)]
        [SerializeField] private float jumpDuration = 0.8f;
        [Min(0f)]
        [SerializeField] private float jumpHeight = 0.75f;
        [SerializeField] private string jumpSortingLayerName = "Occluders";

        private bool hasBeenTriggered;
        private bool jumpIsPlaying;
        private float jumpElapsedTime;
        private Vector3 jumpStartPosition;

        private void Awake()
        {
            exclamationMark.SetActive(false);
        }

        public override bool BeginInteraction(Vector2 pointerWorldPosition)
        {
            if (hasBeenTriggered)
            {
                return false;
            }

            hasBeenTriggered = true;

            if (EncounterStarted != null)
            {
                EncounterStarted.Invoke();
            }

            transform.SetParent(null, true);
            Visual.sortingLayerName = jumpSortingLayerName;
            exclamationMarkVisual.sortingLayerName = jumpSortingLayerName;
            exclamationMarkVisual.sortingOrder = Visual.sortingOrder + 1;
            exclamationMark.SetActive(true);

            jumpStartPosition = transform.position;
            jumpElapsedTime = 0f;
            jumpIsPlaying = true;

            return false;
        }

        private void Update()
        {
            if (!jumpIsPlaying)
            {
                return;
            }

            jumpElapsedTime += Time.deltaTime;

            float progress = Mathf.Clamp01(jumpElapsedTime / jumpDuration);
            float height = 4f * progress * (1f - progress) * jumpHeight;
            transform.position = jumpStartPosition + Vector3.up * height;

            if (progress < 1f)
            {
                return;
            }

            jumpIsPlaying = false;
            transform.position = jumpStartPosition;
            exclamationMark.SetActive(false);

            if (GameOverRequested != null)
            {
                GameOverRequested.Invoke();
            }

        }
    }
}
