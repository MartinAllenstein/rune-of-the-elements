using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[System.Serializable]
public class EnemyGroup
{
    public GameObject enemyPrefab;
    public int count = 5;
    public float spawnInterval = 1f;
}

[System.Serializable]
public class EnemyWave
{
    public string waveName = "Wave";
    public EnemyGroup[] enemyGroups;

    [Header("Wave Timing")]
    public float startDelay = 3f;
    public float postDelay = 5f;
}

public class WaveSpawner : MonoBehaviour
{
    [Header("References")]
    public EnemyWave[] waves;
    public WaypointPath path;
    public TheBase baseHealth;
    public Transform spawnPoint;

    private int currentWaveIndex = -1;
    private bool isSpawning = false;
    private List<Enemies> activeEnemies = new List<Enemies>();

    public event Action<int, EnemyWave> OnWaveStarted;
    public event Action<int> OnWaveCompleted;
    public event Action OnAllEnemiesCleared; // Fired when all enemies gone after last wave

    public bool IsSpawning => isSpawning;

    // Primary property name (keeps your existing logic)
    public bool AllWavesSpawned => currentWaveIndex >= waves.Length - 1;

    // Compatibility alias so other scripts that expect "AllWavesCompleted" compile
    public bool AllWavesCompleted => AllWavesSpawned;

    public int CurrentWave => currentWaveIndex + 1;

    public void StartFirstWave()
    {
        currentWaveIndex = -1;
        SpawnNextWave();
    }

    public void SpawnNextWave()
    {
        if (AllWavesSpawned || isSpawning) return;

        currentWaveIndex++;
        StartCoroutine(SpawnWaveCoroutine(waves[currentWaveIndex]));
    }

    private IEnumerator SpawnWaveCoroutine(EnemyWave wave)
    {
        isSpawning = true;
        OnWaveStarted?.Invoke(currentWaveIndex + 1, wave);

        foreach (var group in wave.enemyGroups)
        {
            for (int i = 0; i < group.count; i++)
            {
                GameObject enemyObj = Instantiate(group.enemyPrefab, spawnPoint.position, Quaternion.identity);
                Enemies enemy = enemyObj.GetComponent<Enemies>();
                enemy.Initialize(path, baseHealth);

                // Track and subscribe
                activeEnemies.Add(enemy);
                enemy.OnDeath += HandleEnemyDeath;

                yield return new WaitForSeconds(group.spawnInterval);
            }
        }

        isSpawning = false;
        OnWaveCompleted?.Invoke(currentWaveIndex + 1);
    }

    public IReadOnlyList<Enemies> GetActiveEnemies()
    {
        return activeEnemies;
    }


    private void HandleEnemyDeath(Enemies enemy)
    {
        enemy.OnDeath -= HandleEnemyDeath;
        activeEnemies.Remove(enemy);

        // If all waves spawned AND no active enemies remain
        if (AllWavesSpawned && activeEnemies.Count == 0)
        {
            OnAllEnemiesCleared?.Invoke();
        }
    }

    public int GetActiveEnemyCount()
    {
        return activeEnemies.Count;
    }
}
