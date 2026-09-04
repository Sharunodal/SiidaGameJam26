using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace SiidaGameJam.BerryPicking
{
    public sealed class SceneInteractionController : MonoBehaviour
    {
        [Header("Scene")]
        [SerializeField] private Camera sceneCamera;
        [FormerlySerializedAs("interactableLayers")]
        [SerializeField] private LayerMask blockingLayers;
        [SerializeField] private LayeredRowSpawner[] rowSpawners;

        private InputAction pointAction;
        private InputAction interactAction;
        private InputAction advanceAction;
        private SceneInteractable activeInteraction;
        private readonly List<RaycastResult> uiRaycastResults =
            new List<RaycastResult>();

        private void Awake()
        {
            pointAction = InputSystem.actions.FindAction("Player/Point");
            interactAction = InputSystem.actions.FindAction("Player/Interact");
            advanceAction = InputSystem.actions.FindAction("Player/Advance");
        }

        private void OnEnable()
        {
            pointAction.Enable();
            interactAction.Enable();
            advanceAction.Enable();
            interactAction.started += OnInteractionStarted;
            interactAction.canceled += OnInteractionCanceled;
            advanceAction.performed += OnAdvancePerformed;
        }

        private void OnDisable()
        {
            interactAction.started -= OnInteractionStarted;
            interactAction.canceled -= OnInteractionCanceled;
            advanceAction.performed -= OnAdvancePerformed;
            advanceAction.Disable();
            interactAction.Disable();
            pointAction.Disable();
            activeInteraction = null;
        }

        private void Update()
        {
            if (activeInteraction != null)
            {
                activeInteraction.ContinueInteraction(ReadPointerWorldPosition());
            }
        }

        private void OnInteractionStarted(InputAction.CallbackContext context)
        {
            Vector2 pointerScreenPosition = pointAction.ReadValue<Vector2>();

            if (PointerIsOverUi(pointerScreenPosition))
            {
                return;
            }

            Vector2 pointerWorldPosition = ConvertToWorldPosition(pointerScreenPosition);
            Collider2D topmostHit = FindTopmostHit(pointerWorldPosition);

            if (topmostHit == null)
            {
                return;
            }

            SceneInteractable interactable = topmostHit.GetComponent<SceneInteractable>();

            if (interactable != null && interactable.BeginInteraction(pointerWorldPosition))
            {
                activeInteraction = interactable;
            }
        }

        private void OnInteractionCanceled(InputAction.CallbackContext context)
        {
            if (activeInteraction == null)
            {
                return;
            }

            activeInteraction.EndInteraction(ReadPointerWorldPosition());
            activeInteraction = null;
        }

        private void OnAdvancePerformed(InputAction.CallbackContext context)
        {
            foreach (LayeredRowSpawner rowSpawner in rowSpawners)
            {
                rowSpawner.Advance();
            }
        }

        private Vector2 ReadPointerWorldPosition()
        {
            Vector2 pointerScreenPosition = pointAction.ReadValue<Vector2>();
            return ConvertToWorldPosition(pointerScreenPosition);
        }

        private Vector2 ConvertToWorldPosition(Vector2 pointerScreenPosition)
        {
            float distanceFromCamera = -sceneCamera.transform.position.z;
            return sceneCamera.ScreenToWorldPoint(
                new Vector3(pointerScreenPosition.x, pointerScreenPosition.y, distanceFromCamera));
        }

        private bool PointerIsOverUi(Vector2 pointerScreenPosition)
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = pointerScreenPosition;

            uiRaycastResults.Clear();
            EventSystem.current.RaycastAll(pointerEventData, uiRaycastResults);

            return uiRaycastResults.Count > 0;
        }

        private Collider2D FindTopmostHit(Vector2 pointerWorldPosition)
        {
            Collider2D[] hits = Physics2D.OverlapPointAll(pointerWorldPosition, blockingLayers);
            Collider2D topmostHit = null;
            SpriteRenderer topmostVisual = null;
            int topmostInteractionPriority = 0;

            foreach (Collider2D hit in hits)
            {
                SceneInteractable interactable = hit.GetComponent<SceneInteractable>();
                SpriteRenderer visual;

                if (interactable != null)
                {
                    visual = interactable.Visual;
                }
                else
                {
                    visual = hit.GetComponent<SpriteRenderer>();
                }

                if (visual == null)
                {
                    continue;
                }

                int interactionPriority;

                if (interactable != null)
                {
                    interactionPriority = interactable.InteractionPriority;
                }
                else
                {
                    interactionPriority = 0;
                }

                if (topmostVisual == null || IsRenderedAbove(
                        visual,
                        interactionPriority,
                        topmostVisual,
                        topmostInteractionPriority))
                {
                    topmostHit = hit;
                    topmostVisual = visual;
                    topmostInteractionPriority = interactionPriority;
                }
            }

            return topmostHit;
        }

        private static bool IsRenderedAbove(
            SpriteRenderer candidate,
            int candidateInteractionPriority,
            SpriteRenderer current,
            int currentInteractionPriority)
        {
            SortingGroup candidateGroup = candidate.GetComponentInParent<SortingGroup>();
            SortingGroup currentGroup = current.GetComponentInParent<SortingGroup>();

            if (candidateGroup != currentGroup)
            {
                int candidateGroupLayer = GetSortingLayerValue(candidate, candidateGroup);
                int currentGroupLayer = GetSortingLayerValue(current, currentGroup);

                if (candidateGroupLayer != currentGroupLayer)
                {
                    return candidateGroupLayer > currentGroupLayer;
                }

                int candidateGroupOrder;

                if (candidateGroup != null)
                {
                    candidateGroupOrder = candidateGroup.sortingOrder;
                }
                else
                {
                    candidateGroupOrder = candidate.sortingOrder;
                }

                int currentGroupOrder;

                if (currentGroup != null)
                {
                    currentGroupOrder = currentGroup.sortingOrder;
                }
                else
                {
                    currentGroupOrder = current.sortingOrder;
                }

                if (candidateGroupOrder != currentGroupOrder)
                {
                    return candidateGroupOrder > currentGroupOrder;
                }
            }

            int candidateLayer = SortingLayer.GetLayerValueFromID(candidate.sortingLayerID);
            int currentLayer = SortingLayer.GetLayerValueFromID(current.sortingLayerID);

            if (candidateLayer != currentLayer)
            {
                return candidateLayer > currentLayer;
            }

            if (candidate.sortingOrder != current.sortingOrder)
            {
                return candidate.sortingOrder > current.sortingOrder;
            }

            return candidateInteractionPriority > currentInteractionPriority;
        }

        private static int GetSortingLayerValue(
            SpriteRenderer visual,
            SortingGroup sortingGroup)
        {
            int sortingLayerId;

            if (sortingGroup != null)
            {
                sortingLayerId = sortingGroup.sortingLayerID;
            }
            else
            {
                sortingLayerId = visual.sortingLayerID;
            }

            return SortingLayer.GetLayerValueFromID(sortingLayerId);
        }
    }
}
