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

        if (FindFirstObjectByType<SpawnManager>() != null)
        {
            FindFirstObjectByType<SpawnManager>().OnAllWavesCompleted += SpawnManager_OnAllWavesCompleted;
        }

        waveSpawner = FindFirstObjectByType<WaveSpawner>();
        if (waveSpawner != null)
        {
            waveSpawner.OnWaveCompleted += WaveSpawner_OnWaveCompleted;
        }

    }

    private void OnDestroy()
    {
        TheBase.OnBaseDestroyed -= TheBase_OnBaseDestroyed;
        if (FindFirstObjectByType<SpawnManager>() != null)
        {
            FindFirstObjectByType<SpawnManager>().OnAllWavesCompleted -= SpawnManager_OnAllWavesCompleted;
        }

        if (waveSpawner != null)
        {
            waveSpawner.OnWaveCompleted -= WaveSpawner_OnWaveCompleted;
        }
    }

    private void TheBase_OnBaseDestroyed(object sender, EventArgs e)
    {
        state = GameState.GameOver;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SpawnManager_OnAllWavesCompleted(object sender, EventArgs e)
    {
        state = GameState.Victory;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
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

    private void WaveSpawner_OnWaveCompleted(int waveNumber)
    {
        // If all waves are done -> trigger victory
        if (waveSpawner.AllWavesCompleted)
        {
            state = GameState.Victory;
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            // Optionally, start next wave automatically
            waveSpawner.SpawnNextWave();
        }
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

                    if (waveSpawner != null)
                    {
                        waveSpawner.StartFirstWave();
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


