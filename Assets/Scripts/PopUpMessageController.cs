using System.Collections;
using UnityEngine;

namespace SiidaGameJam.BerryPicking
{
    public sealed class PopUpMessageController : MonoBehaviour
    {
        [SerializeField] private GameObject popUpMessage;
        [SerializeField] private float displayDurationInSeconds = 3f;

        private Coroutine hideMessageCoroutine;

        private void OnEnable()
        {
            BerryBush.LastBerryGatherAttempted += ShowMessage;
        }

        private void OnDisable()
        {
            BerryBush.LastBerryGatherAttempted -= ShowMessage;

            if (hideMessageCoroutine != null)
            {
                StopCoroutine(hideMessageCoroutine);
                hideMessageCoroutine = null;
            }
        }

        private void ShowMessage()
        {
            if (hideMessageCoroutine != null)
            {
                StopCoroutine(hideMessageCoroutine);
            }

            popUpMessage.SetActive(true);
            hideMessageCoroutine = StartCoroutine(HideMessageAfterDelay());
        }

        private IEnumerator HideMessageAfterDelay()
        {
            yield return new WaitForSecondsRealtime(displayDurationInSeconds);
            popUpMessage.SetActive(false);
            hideMessageCoroutine = null;
        }
    }
}
