using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event EventHandler OnGamePause;
    public event EventHandler OnGameUnpaused;

    private enum GameState
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
        Victory
    }

    private GameState state;
    private WaveSpawner waveSpawner;
    private WaveManager waveManager;
    private float countdownToStartTimer = 3f;
    private bool isGamePaused = false;

    private void Awake()
    {
        Instance = this;
        state = GameState.WaitingToStart;
    }

    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;

        TheBase.OnBaseDestroyed += TheBase_OnBaseDestroyed;

        // If you still have a SpawnManager that signals all waves completed, keep it (but note: victory should be based on all enemies cleared)
        if (FindFirstObjectByType<SpawnManager>() != null)
        {
            FindFirstObjectByType<SpawnManager>().OnAllWavesCompleted += SpawnManager_OnAllWavesCompleted;
        }

        // cache the WaveSpawner reference
        waveSpawner = FindFirstObjectByType<WaveSpawner>();
        if (waveSpawner != null)
        {
            // subscribe to the proper event that signals "all enemies cleared after all waves"
            waveSpawner.OnAllEnemiesCleared += WaveSpawner_OnAllEnemiesCleared;

            // If you want other info (like UI updates) you can also subscribe to OnWaveStarted / OnWaveCompleted here,
            // but DO NOT set Victory here based on waves spawned only.
            // Example: waveSpawner.OnWaveStarted += SomeHandlerForUI;
        }

        // cache the WaveManager reference (may be null if not in scene)
        waveManager = FindFirstObjectByType<WaveManager>();

        // Don't subscribe to waveManager.waveSpawner.OnWaveCompleted here without checking for null,
        // and avoid setting Victory in that handler. WaveSpawner.OnAllEnemiesCleared is the correct trigger.
        // (If you need to react to OnWaveCompleted for UI, do a guarded subscription as shown below.)
        if (waveManager != null && waveManager.waveSpawner != null)
        {
            // Example: subscribe to OnWaveCompleted for UI purposes (this does NOT set Victory)
            waveManager.waveSpawner.OnWaveCompleted += WaveSpawner_OnWaveCompleted_ForUI;
        }
    }

    private void OnDestroy()
    {
        GameInput.Instance.OnPauseAction -= GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction -= GameInput_OnInteractAction;

        TheBase.OnBaseDestroyed -= TheBase_OnBaseDestroyed;

        if (FindFirstObjectByType<SpawnManager>() != null)
        {
            FindFirstObjectByType<SpawnManager>().OnAllWavesCompleted -= SpawnManager_OnAllWavesCompleted;
        }

        if (waveSpawner != null)
        {
            waveSpawner.OnAllEnemiesCleared -= WaveSpawner_OnAllEnemiesCleared;
            // unsubscribe any additional listeners if you added them (not necessary if you didn't)
            // waveSpawner.OnWaveStarted -= SomeHandlerForUI;
        }

        if (waveManager != null && waveManager.waveSpawner != null)
        {
            waveManager.waveSpawner.OnWaveCompleted -= WaveSpawner_OnWaveCompleted_ForUI;
        }
    }

    private void TheBase_OnBaseDestroyed(object sender, EventArgs e)
    {
        state = GameState.GameOver;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    // If you still use SpawnManager for something else, you can keep this.
    // But do NOT set Victory here unless you also ensure no enemies remain.
    private void SpawnManager_OnAllWavesCompleted(object sender, EventArgs e)
    {
        // Keep this if SpawnManager is used for other logic.
        // Note: do NOT mark victory here — waiting for OnAllEnemiesCleared is the safe approach.
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (state == GameState.WaitingToStart)
        {
            state = GameState.CountdownToStart;
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        TogglePauseGame();
    }

    // This method is kept for UI reactions to wave completion — it should NOT set Victory.
    private void WaveSpawner_OnWaveCompleted_ForUI(int waveNumber)
    {
        // Example: update some UI, play sound, etc.
        // Debug.Log($"Wave {waveNumber} completed.");
    }

    private void Update()
    {
        switch (state)
        {
            case GameState.WaitingToStart:
                break;

            case GameState.CountdownToStart:
                countdownToStartTimer -= Time.deltaTime;
                if (countdownToStartTimer < 0f)
                {
                    state = GameState.GamePlaying;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);

                    // Start wave system when game starts
                    if (waveManager != null)
                    {
                        waveManager.StartWaveSystem();
                    }
                }
                break;

            case GameState.GamePlaying:
                break;

            case GameState.GameOver:
                // For The UI
                break;

            case GameState.Victory:
                // For The UI
                break;
        }
    }

    public bool IsGamePlaying()
    {
        return state == GameState.GamePlaying;
    }

    public bool IsCountdownToStartActive()
    {
        return state == GameState.CountdownToStart;
    }

    public bool IsGameOver()
    {
        return state == GameState.GameOver;
    }

    public bool IsVictory()
    {
        return state == GameState.Victory;
    }

    public float GetCountdownToStartTimer()
    {
        return countdownToStartTimer;
    }

    // This is the correct place to set Victory: after all waves spawned AND all enemies cleared.
    private void WaveSpawner_OnAllEnemiesCleared()
    {
        state = GameState.Victory;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void TogglePauseGame()
    {
        isGamePaused = !isGamePaused;
        if (isGamePaused)
        {
            Time.timeScale = 0f;
            OnGamePause?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;
            OnGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }
}
