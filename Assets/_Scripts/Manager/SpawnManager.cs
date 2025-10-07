using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    public event EventHandler OnAllWavesCompleted;
    
    [Header("Wave Configuration")]
    [SerializeField] private List<WaveConfigSO> waves; // All Wave
    [SerializeField] private float timeBetweenWaves = 5f;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    private int currentWaveIndex = 0;
    private int enemiesRemaining;
    private bool isSpawning = false;

    private void OnEnable()
    {
        Enemy.OnEnemyKilled += HandleEnemyKilled;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyKilled -= HandleEnemyKilled;
    }

    private void Start()
    {
        StartCoroutine(StartNextWave());
    }

    private void HandleEnemyKilled(Enemy enemy)
    {
        enemiesRemaining--;

        if (enemiesRemaining <= 0 && !isSpawning)
        {
            // Check if Last
            if (currentWaveIndex >= waves.Count)
            {
                OnAllWavesCompleted?.Invoke(this, EventArgs.Empty);
                Debug.Log("All waves cleared! VICTORY!");
            }
            else // Check if not Last
            {
                StartCoroutine(StartNextWave());
            }
        }
    }

    private IEnumerator StartNextWave()
    {
        Debug.Log("Preparing for Wave " + (currentWaveIndex + 1));
        
        yield return new WaitForSeconds(timeBetweenWaves);

        StartCoroutine(SpawnWave(waves[currentWaveIndex]));
    }

    private IEnumerator SpawnWave(WaveConfigSO wave)
    {
        Debug.Log("Wave " + (currentWaveIndex + 1) + " starting!");
        isSpawning = true;
        enemiesRemaining = wave.enemyCount;

        for (int i = 0; i < wave.enemyCount; i++)
        {
            // random spawnPoints
            Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            Instantiate(wave.enemyPrefab, randomSpawnPoint.position, randomSpawnPoint.rotation);

            yield return new WaitForSeconds(wave.spawnInterval);
        }

        isSpawning = false;
        currentWaveIndex++;
        
    }
}