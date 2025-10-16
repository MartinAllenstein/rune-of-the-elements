using UnityEngine;
using System.Collections;
using System;

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
    public float startDelay = 3f;  // Wait before wave starts
    public float postDelay = 5f;   // Wait after wave ends
}

[System.Serializable] 
public class WaveSpawner : MonoBehaviour
{
    public EnemyWave[] waves;
    public WaypointPath path;
    public TheBase baseHealth;
    public Transform spawnPoint;

    private int currentWaveIndex = -1;
    private bool isSpawning = false;

    public event Action<int, EnemyWave> OnWaveStarted;
    public event Action<int> OnWaveCompleted;

    public bool IsSpawning => isSpawning;
    public bool AllWavesCompleted => currentWaveIndex >= waves.Length - 1;
    public int CurrentWave => currentWaveIndex + 1;

    public void StartFirstWave()
    {
        currentWaveIndex = -1;
        SpawnNextWave();
    }

    public void SpawnNextWave()
    {
        if (AllWavesCompleted || isSpawning) return;
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
                yield return new WaitForSeconds(group.spawnInterval);
            }
        }

        isSpawning = false;
        OnWaveCompleted?.Invoke(currentWaveIndex + 1);
    }
}
