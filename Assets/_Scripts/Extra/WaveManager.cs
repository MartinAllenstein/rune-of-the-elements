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

    private float countdown;
    private bool countingDown;

    private void Start()
    {
        waveSpawner.OnWaveStarted += HandleWaveStarted;
        waveSpawner.OnWaveCompleted += HandleWaveCompleted;
    }

    private void Update()
    {
        if (countingDown)
        {
            countdown -= Time.deltaTime;
            countdownText.text = $"Next Wave In: {Mathf.Max(0, countdown):0.0}s";
        }
    }

    private IEnumerator WaveCountdownCoroutine(float delay, bool isFirstWave = false)
    {
        countingDown = true;
        countdown = delay;

        while (countdown > 0)
        {
            yield return null;
        }

        countingDown = false;
        countdownText.text = "Wave Spawning!";

        if (isFirstWave)
            waveSpawner.StartFirstWave();
        else
            waveSpawner.SpawnNextWave();
    }

    private void HandleWaveStarted(int waveIndex, EnemyWave wave)
    {
        waveText.text = $"Wave {waveIndex}";
        countdownText.text = "Wave In Progress!";
    }

    private void HandleWaveCompleted(int waveIndex)
    {
        if (!waveSpawner.AllWavesCompleted)
        {
            float nextDelay = waveSpawner.waves[waveIndex - 1].postDelay; // use post delay of completed wave
            StartCoroutine(WaveCountdownCoroutine(nextDelay));
        }
        else
        {
            countdownText.text = "All Waves Deployed!";
        }
    }
    public void StartWaveSystem()
    {
        // Prevent starting early; only begins once game is playing
        StopAllCoroutines();
        StartCoroutine(WaveCountdownCoroutine(waveSpawner.waves[0].startDelay, isFirstWave: true));
    }

}
