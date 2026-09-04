using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace SiidaGameJam.BerryPicking
{
    public sealed class SceneInteractionController : MonoBehaviour
    {
        [Header("Scene")]
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private LayerMask interactableLayers;

        private InputAction pointAction;
        private InputAction interactAction;
        private SceneInteractable activeInteraction;

        private void Awake()
        {
            pointAction = InputSystem.actions.FindAction("Player/Point");
            interactAction = InputSystem.actions.FindAction("Player/Interact");
        }

        private void OnEnable()
        {
            pointAction.Enable();
            interactAction.Enable();
            interactAction.started += OnInteractionStarted;
            interactAction.canceled += OnInteractionCanceled;
        }

        private void OnDisable()
        {
            interactAction.started -= OnInteractionStarted;
            interactAction.canceled -= OnInteractionCanceled;
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
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Vector2 pointerWorldPosition = ReadPointerWorldPosition();
            SceneInteractable interactable = FindTopmostInteractable(pointerWorldPosition);

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

        private Vector2 ReadPointerWorldPosition()
        {
            Vector2 pointerScreenPosition = pointAction.ReadValue<Vector2>();
            float distanceFromCamera = -sceneCamera.transform.position.z;
            return sceneCamera.ScreenToWorldPoint(
                new Vector3(pointerScreenPosition.x, pointerScreenPosition.y, distanceFromCamera));
        }

        private SceneInteractable FindTopmostInteractable(Vector2 pointerWorldPosition)
        {
            Collider2D[] hits = Physics2D.OverlapPointAll(pointerWorldPosition, interactableLayers);
            SceneInteractable topmost = null;

            foreach (Collider2D hit in hits)
            {
                SceneInteractable candidate = hit.GetComponent<SceneInteractable>();

                if (candidate != null && IsRenderedAbove(candidate, topmost))
                {
                    topmost = candidate;
                }
            }

            return topmost;
        }

        private static bool IsRenderedAbove(SceneInteractable candidate, SceneInteractable current)
        {
            if (current == null)
            {
                return true;
            }

            if (candidate.SortingLayerValue != current.SortingLayerValue)
            {
                return candidate.SortingLayerValue > current.SortingLayerValue;
            }

            if (candidate.SortingOrder != current.SortingOrder)
            {
                return candidate.SortingOrder > current.SortingOrder;
            }

            return candidate.InteractionPriority > current.InteractionPriority;
        }
    }
}
