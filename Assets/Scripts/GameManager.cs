using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
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

    [Header("Timed Gameplay")]
    [SerializeField] private bool timedGameplayIsEnabled;
    [Min(1f)]
    [SerializeField] private float timeLimitInSeconds = 60f;
    [SerializeField] private TMP_Text timeValueText;
    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject stageClearScreen;

    private InputAction escapeAction;
    private IDisposable anyButtonPressSubscription;
    private bool gameIsPaused;
    private bool waitingForStart;
    private bool startWasRequested;
    private bool timeHasExpired;
    private float timeRemaining;
    private int displayedSeconds = -1;

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

        if (timedGameplayIsEnabled)
        {
            timeRemaining = timeLimitInSeconds;
            UpdateTimerDisplay();

            startScreen.SetActive(true);
            stageClearScreen.SetActive(false);
            waitingForStart = true;
            Time.timeScale = 0f;
            SetGameplayBehavioursEnabled(false);

            anyButtonPressSubscription =
                InputSystem.onAnyButtonPress.CallOnce(OnAnyButtonPressed);
        }
    }

    private void Update()
    {
        if (!timedGameplayIsEnabled)
        {
            return;
        }

        if (startWasRequested)
        {
            StartTimedGameplay();
        }

        if (waitingForStart || gameIsPaused || timeHasExpired)
        {
            return;
        }

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            UpdateTimerDisplay();
            FinishTimedGameplay();
            return;
        }

        UpdateTimerDisplay();
    }

    private void OnDisable()
    {
        escapeAction.performed -= OnEscapePerformed;
        escapeAction.Disable();

        if (anyButtonPressSubscription != null)
        {
            anyButtonPressSubscription.Dispose();
            anyButtonPressSubscription = null;
        }
    }

    private void OnEscapePerformed(InputAction.CallbackContext context)
    {
        if (!pausingIsEnabled || waitingForStart || timeHasExpired)
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

    private void OnAnyButtonPressed(InputControl button)
    {
        startWasRequested = true;
    }

    private void StartTimedGameplay()
    {
        startWasRequested = false;
        waitingForStart = false;
        gameIsPaused = false;
        startScreen.SetActive(false);
        Time.timeScale = 1f;
        SetGameplayBehavioursEnabled(true);
    }

    private void FinishTimedGameplay()
    {
        timeHasExpired = true;
        Time.timeScale = 0f;
        SetGameplayBehavioursEnabled(false);
        pauseScreen.SetActive(false);
        stageClearScreen.SetActive(true);
    }

    private void UpdateTimerDisplay()
    {
        int secondsRemaining = Mathf.CeilToInt(timeRemaining);

        if (secondsRemaining == displayedSeconds)
        {
            return;
        }

        displayedSeconds = secondsRemaining;
        timeValueText.text = secondsRemaining.ToString();
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
        waitingForStart = false;
        startWasRequested = false;
        timeHasExpired = false;
    }

    private void SetGameplayBehavioursEnabled(bool behavioursAreEnabled)
    {
        foreach (Behaviour gameplayBehaviour in gameplayBehaviours)
        {
            gameplayBehaviour.enabled = behavioursAreEnabled;
        }
    }
}
