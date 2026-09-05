using System;
using SiidaGameJam.BerryPicking;
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
    [SerializeField] private GameObject ingameUi;
    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject stageClearScreen;
    [SerializeField] private TMP_Text stageClearInfoText;
    [TextArea]
    [SerializeField] private string normalStageClearMessage =
        "Game Clear\n\nCollected";
    [TextArea]
    [SerializeField] private string lemmingGameOverMessage =
        "You were chased away by an angry lemming!";
    [SerializeField] private BerryCounter berryCounter;
    [SerializeField] private TMP_Text stageClearBerriesValueText;
    [SerializeField] private AzaleaFlowerCounter azaleaFlowerCounter;
    [SerializeField] private TMP_Text stageClearFlowersValueText;

    private InputAction escapeAction;
    private IDisposable anyButtonPressSubscription;
    private bool gameIsPaused;
    private bool waitingForStart;
    private bool startWasRequested;
    private bool gameplayHasEnded;
    private bool lemmingEncounterIsPlaying;
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
        LemmingEncounter.EncounterStarted += BeginLemmingEncounter;
        LemmingEncounter.GameOverRequested += FinishGameFromLemming;
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

            ingameUi.SetActive(true);
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

        if (waitingForStart ||
            gameIsPaused ||
            gameplayHasEnded ||
            lemmingEncounterIsPlaying)
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
        LemmingEncounter.EncounterStarted -= BeginLemmingEncounter;
        LemmingEncounter.GameOverRequested -= FinishGameFromLemming;

        if (anyButtonPressSubscription != null)
        {
            anyButtonPressSubscription.Dispose();
            anyButtonPressSubscription = null;
        }
    }

    private void OnEscapePerformed(InputAction.CallbackContext context)
    {
        if (!pausingIsEnabled ||
            waitingForStart ||
            gameplayHasEnded ||
            lemmingEncounterIsPlaying)
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
        gameplayHasEnded = true;
        Time.timeScale = 0f;
        SetGameplayBehavioursEnabled(false);
        pauseScreen.SetActive(false);
        ingameUi.SetActive(false);
        stageClearInfoText.text = normalStageClearMessage;
        UpdateStageClearBerriesDisplay();
        UpdateStageClearFlowersDisplay();
        stageClearScreen.SetActive(true);
    }

    private void FinishGameFromLemming()
    {
        if (!timedGameplayIsEnabled ||
            waitingForStart ||
            gameplayHasEnded ||
            !lemmingEncounterIsPlaying)
        {
            return;
        }

        lemmingEncounterIsPlaying = false;
        gameplayHasEnded = true;
        Time.timeScale = 0f;
        SetGameplayBehavioursEnabled(false);
        pauseScreen.SetActive(false);
        ingameUi.SetActive(false);
        stageClearInfoText.text = lemmingGameOverMessage;
        stageClearBerriesValueText.text = "";
        stageClearFlowersValueText.text = "";
        stageClearScreen.SetActive(true);
    }

    private void BeginLemmingEncounter()
    {
        if (!timedGameplayIsEnabled || waitingForStart || gameplayHasEnded)
        {
            return;
        }

        lemmingEncounterIsPlaying = true;
        SetGameplayBehavioursEnabled(false);
    }

    private void UpdateStageClearBerriesDisplay()
    {
        int berriesPicked = berryCounter.BerriesPicked;

        if (berriesPicked == 1)
        {
            stageClearBerriesValueText.text = berriesPicked.ToString() + " berry";
        }
        else
        {
            stageClearBerriesValueText.text = berriesPicked.ToString() + " berries";
        }
    }

    private void UpdateStageClearFlowersDisplay()
    {
        int flowersPicked = azaleaFlowerCounter.AzaleaFlowersPicked;

        if (flowersPicked == 1)
        {
            stageClearFlowersValueText.text = flowersPicked.ToString() + " flower";
        }
        else
        {
            stageClearFlowersValueText.text = flowersPicked.ToString() + " flowers";
        }
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
        gameplayHasEnded = false;
        lemmingEncounterIsPlaying = false;
    }

    private void SetGameplayBehavioursEnabled(bool behavioursAreEnabled)
    {
        foreach (Behaviour gameplayBehaviour in gameplayBehaviours)
        {
            gameplayBehaviour.enabled = behavioursAreEnabled;
        }
    }
}
