using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // REQUIRED for TextMeshPro UI components

public class BombMissionManager : MonoBehaviour
{
    public static BombMissionManager Instance { get; private set; }

    // Stores the game outcome ("WIN" or "LOOSE") across scenes
    public static string finalStatus = ""; 

    [Header("Mission Settings")]
    [SerializeField] private int bombsRequiredToWin = 2;
    [SerializeField] private float surviveAfterFuseTime = 5f;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI timerText;       // Drag your Timer Text object here
    [SerializeField] private TextMeshProUGUI bombCounterText;  // Drag your Bomb Counter Text object here

    [Header("UI Juice Settings")]
    [SerializeField] private Color safeTimerColor = Color.green;
    [SerializeField] private Color dangerTimerColor = Color.red;
    [SerializeField] private float dangerTimeThreshold = 10f; // Seconds left to trigger red flashing/pulsing
    [SerializeField] private float punchScaleAmount = 1.3f;   // How big the pop animation is
    [SerializeField] private float punchDuration = 0.15f;     // How fast the pop animation snaps back

    private Bomb[] bombs;
    private int bombsDefused;
    private bool missionFinished;
    private bool playerDead;

    private float totalMissionDuration;
    private float missionTimeRemaining;

    // References to running animation routines to prevent overlapping glitches
    private Coroutine timerAnimationRoutine;
    private Coroutine bombAnimationRoutine;

    public event Action OnMissionSuccess;
    public event Action OnMissionFailed;

    public int BombsDefused => bombsDefused;
    public int BombsRequired => bombsRequiredToWin;
    public bool MissionFinished => missionFinished;
    public bool ObjectiveCompleted => bombsDefused >= bombsRequiredToWin;
    public float MissionTimeRemaining => missionTimeRemaining;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Automatically find every bomb in the scene
        bombs = FindObjectsOfType<Bomb>();

        if (bombs.Length == 0)
        {
            Debug.LogError("BombMissionManager: No Bomb objects found in the scene.");
        }
    }

    private void Start()
    {
        foreach (Bomb bomb in bombs)
        {
            bomb.OnDefused += HandleBombDefused;
        }

        // Initialize the UI counter display right at the start
        UpdateBombUI();

        if (bombs.Length > 0)
        {
            // Grabs the first bomb's fuse time safely using index 0
            totalMissionDuration = bombs[0].fuseTime + surviveAfterFuseTime;

            Debug.Log($"Mission Duration: {totalMissionDuration} seconds");

            StartCoroutine(MissionTimer(totalMissionDuration));
        }
    }

    private void OnDestroy()
    {
        foreach (Bomb bomb in bombs)
        {
            if (bomb != null)
                bomb.OnDefused -= HandleBombDefused;
        }
    }

    private void HandleBombDefused()
    {
        if (missionFinished)
            return;

        bombsDefused++;
        Debug.Log($"Bomb Defused ({bombsDefused}/{bombsRequiredToWin})");
        
        // Update the numbers on screen whenever a bomb is defused
        UpdateBombUI();

        // Trigger a visual pop animation on the bomb counter text
        TriggerTextPop(ref bombAnimationRoutine, bombCounterText);
    }

    private IEnumerator MissionTimer(float duration)
    {
        missionTimeRemaining = duration;

        while (missionTimeRemaining > 0f)
        {
            missionTimeRemaining -= Time.deltaTime;
            
            // Format the time and handle dynamic color updates every frame
            UpdateTimerUI();
            
            yield return null;
        }

        missionTimeRemaining = 0f;
        UpdateTimerUI();

        if (missionFinished)
            yield break;

        if (bombsDefused >= bombsRequiredToWin)
            MissionSuccess();
        else
            MissionFailed();
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;

        // 1. Text Formatting into Minutes:Seconds
        int minutes = Mathf.FloorToInt(missionTimeRemaining / 60F);
        int seconds = Mathf.FloorToInt(missionTimeRemaining % 60F);
        timerText.text = string.Format("Time Left: {0:0}:{1:00}", minutes, seconds);

        // 2. Dynamic Color Transition from green to red based on remaining time
        float timeRatio = Mathf.Clamp01(missionTimeRemaining / totalMissionDuration);
        timerText.color = Color.Lerp(dangerTimerColor, safeTimerColor, timeRatio);

        // 3. Heartbeat Pulse when time is dangerously low
        if (missionTimeRemaining <= dangerTimeThreshold && missionTimeRemaining > 0)
        {
            // Speeds up the pulsing rhythm as time gets closer to 0
            float pulseSpeed = missionTimeRemaining <= 5f ? 15f : 8f;
            float pulseModifier = 1f + (Mathf.Sin(Time.time * pulseSpeed) * 0.1f);
            timerText.transform.localScale = Vector3.one * pulseModifier;
        }
    }

    private void UpdateBombUI()
    {
        if (bombCounterText == null) return;

        // Displays current progress (e.g., "Bombs: 0 / 2")
        bombCounterText.text = $"Bombs: {bombsDefused} / {bombsRequiredToWin}";
    }

    // Reuseable visual feedback pop engine
    private void TriggerTextPop(ref Coroutine routine, TextMeshProUGUI targetText)
    {
        if (targetText == null) return;
        
        if (routine != null)
            StopCoroutine(routine);
            
        routine = StartCoroutine(AnimateTextPop(targetText));
    }

    private IEnumerator AnimateTextPop(TextMeshProUGUI targetText)
    {
        Transform textTransform = targetText.transform;
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = Vector3.one * punchScaleAmount;

        float elapsed = 0f;
        float halfDuration = punchDuration / 2f;
        
        // Scale Up quickly
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            textTransform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / halfDuration);
            yield return null;
        }

        // Scale Back down smoothly
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            textTransform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / halfDuration);
            yield return null;
        }

        textTransform.localScale = originalScale;
    }

    public void PlayerDied()
    {
        if (missionFinished)
            return;

        playerDead = true;
        Debug.Log("Mission Failed - Player Died");
        MissionFailed();
    }

    private void MissionSuccess()
    {
        if (missionFinished)
            return;

        missionFinished = true;
        Debug.Log("MISSION COMPLETE");
        OnMissionSuccess?.Invoke();

        finalStatus = "WIN"; // Save state to read on next screen
        SceneManager.LoadScene("GameOver");
    }

    private void MissionFailed()
    {
        if (missionFinished)
            return;

        missionFinished = true;
        Debug.Log("MISSION FAILED");
        OnMissionFailed?.Invoke();

        finalStatus = "LOOSE"; // Save state to read on next screen
        SceneManager.LoadScene("GameOver");
    }
}