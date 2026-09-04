using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public sealed class GameManager : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string mainSceneName;
    [SerializeField] private string berryPickingSceneName;

    [Header("Pause")]
    [SerializeField] private bool pausingIsEnabled = true;
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private Behaviour[] gameplayBehaviours;

    private InputAction escapeAction;
    private bool gameIsPaused;

    private void Awake()
    {
        escapeAction = InputSystem.actions.FindAction("Player/Escape");
    }

    private void OnEnable()
    {
        escapeAction.Enable();
        escapeAction.performed += OnEscapePerformed;
    }

    private void Start()
    {
        gameIsPaused = false;
        Time.timeScale = 1f;

        if (pausingIsEnabled)
        {
            pauseScreen.SetActive(false);
            SetGameplayBehavioursEnabled(true);
        }
    }

    private void OnDisable()
    {
        escapeAction.performed -= OnEscapePerformed;
        escapeAction.Disable();
    }

    private void OnEscapePerformed(InputAction.CallbackContext context)
    {
        if (!pausingIsEnabled)
        {
            return;
        }

        if (gameIsPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        gameIsPaused = true;
        Time.timeScale = 0f;
        pauseScreen.SetActive(true);
        SetGameplayBehavioursEnabled(false);
    }

    public void ResumeGame()
    {
        gameIsPaused = false;
        Time.timeScale = 1f;
        pauseScreen.SetActive(false);
        SetGameplayBehavioursEnabled(true);
    }

    public void OpenBerryPicking()
    {
        PrepareForSceneChange();
        SceneManager.LoadScene(berryPickingSceneName);
    }

    public void ReturnToMainScreen()
    {
        PrepareForSceneChange();
        SceneManager.LoadScene(mainSceneName);
    }

    private void PrepareForSceneChange()
    {
        Time.timeScale = 1f;
        gameIsPaused = false;
    }

    private void SetGameplayBehavioursEnabled(bool behavioursAreEnabled)
    {
        foreach (Behaviour gameplayBehaviour in gameplayBehaviours)
        {
            gameplayBehaviour.enabled = behavioursAreEnabled;
        }
    }
}
