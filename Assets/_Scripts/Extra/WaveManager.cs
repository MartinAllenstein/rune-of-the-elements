using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Unity.Netcode;

public class WaveManager : NetworkBehaviour
{
    [Header("References")]
    public WaveSpawner waveSpawner;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI countdownText;

    private float currentCountdownTime;
    private float totalCountdownTime;
    private bool isCountingDown;
    
    private int nextWaveIndex = 0;
    private bool isWaveSystemStarted = false;

    private void Start()
    {
        waveSpawner.OnWaveStarted += HandleWaveStarted;
        waveSpawner.OnWaveCompleted += HandleWaveCompleted;
    }

    private void Update()
    {
        if (isCountingDown)
        {
            currentCountdownTime -= Time.deltaTime;
            countdownText.text = $"Next Wave In: {Mathf.Max(0, currentCountdownTime):0.0}s";
        }
    }

    private IEnumerator WaveCountdownCoroutine(float delay, int nextWave)
    {
        isCountingDown = true;
        currentCountdownTime = delay;
        totalCountdownTime = delay;
        nextWaveIndex = nextWave;

        while (currentCountdownTime > 0)
        {
            yield return null;
        }

        isCountingDown = false;
        countdownText.text = "Wave Spawning!";

        if (nextWave == 0)
            waveSpawner.StartFirstWave();
        else
            waveSpawner.SpawnNextWave();
    }

    private void HandleWaveStarted(int waveIndex, EnemyWave wave)
    {
        waveText.text = $"Wave {waveIndex}";
        countdownText.text = "Wave In Progress!";
        
        isCountingDown = false;
    }

    private void HandleWaveCompleted(int waveIndex)
    {
        if (!waveSpawner.AllWavesCompleted)
        {
            float nextDelay = waveSpawner.waves[waveIndex - 1].postDelay; // use post delay of completed wave
            StartCoroutine(WaveCountdownCoroutine(nextDelay, waveIndex));
        }
        else
        {
            countdownText.text = "All Waves Deployed!";
            isCountingDown = false;

            nextWaveIndex = waveSpawner.waves.Length;
        }
    }
    public void StartWaveSystem()
    {
        isWaveSystemStarted = true;
        
        StopAllCoroutines();
        StartCoroutine(WaveCountdownCoroutine(waveSpawner.waves[0].startDelay, 0));
    }
    
    public float GetTimeProgressNormalized()
    {
        if (!isWaveSystemStarted) return 0f;
        
        int totalWaves = waveSpawner.waves.Length;
        if (totalWaves == 0) return 0f;
        
        float segmentSize = 1f / totalWaves;
        float baseProgress = nextWaveIndex * segmentSize;

        if (isCountingDown && totalCountdownTime > 0)
        {
           
            float timeRatio = 1f - (currentCountdownTime / totalCountdownTime);
            
            return baseProgress + (timeRatio * segmentSize);
        }
        else if (!isCountingDown && nextWaveIndex < totalWaves) 
        {
            //return (nextWaveIndex + 1) * segmentSize; 
            
            return baseProgress + segmentSize;
        }
        else
        {
            return baseProgress;
        }
    }
    
    public int GetTotalWaves()
    {
        return waveSpawner.waves.Length;
    }

}
