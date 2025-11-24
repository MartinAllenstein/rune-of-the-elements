using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

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

public class WaveSpawner : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] public EnemyWave[] waves;
    [SerializeField] private WaypointPath[] paths;
    [SerializeField] private TheBase baseHealth; // Old not use
    //[SerializeField] private Transform spawnPoint;

    private int currentWaveIndex = -1;
    private bool isSpawning = false;
    
    private List<Enemy> activeEnemies = new List<Enemy>();

    public event Action<int, EnemyWave> OnWaveStarted;
    public event Action<int> OnWaveCompleted;
    public event Action OnAllEnemiesCleared;

    public bool IsSpawning => isSpawning;
    public bool AllWavesSpawned => currentWaveIndex >= waves.Length - 1;
    public bool AllWavesCompleted => AllWavesSpawned;
    public int CurrentWave => currentWaveIndex + 1;

    // --- Server Side Logic ---
    public void StartFirstWave()
    {
        if (!IsServer) return;
        currentWaveIndex = -1;
        SpawnNextWave();
        Debug.Log("Starting first wave");
    }

    public void SpawnNextWave()
    {
        if (!IsServer) return;
        if (AllWavesSpawned || isSpawning) return;

        Debug.Log("Spawning next wave");
        currentWaveIndex++;
        StartCoroutine(SpawnWaveCoroutine(waves[currentWaveIndex]));
    }

    private IEnumerator SpawnWaveCoroutine(EnemyWave wave)
    {
        isSpawning = true;
        
        OnWaveStarted?.Invoke(currentWaveIndex + 1, wave); 
        OnWaveStartedClientRpc(currentWaveIndex); 

        foreach (var group in wave.enemyGroups)
        {
            for (int i = 0; i < group.count; i++)
            {
                WaypointPath chosenPath = paths[Random.Range(0, paths.Length)];
                
                Vector3 spawnPos = chosenPath.GetWaypoint(0).position;
                
                GameObject enemyObj = Instantiate(group.enemyPrefab, spawnPos, Quaternion.identity);
                
                NetworkObject enemyNetObj = enemyObj.GetComponent<NetworkObject>();
                if (enemyNetObj != null)
                {
                    enemyNetObj.Spawn(true); 
                }

                Enemy enemy = enemyObj.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.Initialize(chosenPath, baseHealth);
                    
                    activeEnemies.Add(enemy);
                    enemy.OnDeath += HandleEnemyDeath;
                }

                yield return new WaitForSeconds(group.spawnInterval);
            }
        }

        isSpawning = false;
        
        OnWaveCompleted?.Invoke(currentWaveIndex + 1);
        OnWaveCompletedClientRpc(currentWaveIndex);
    }

    private void HandleEnemyDeath(Enemy enemy)
    {
        enemy.OnDeath -= HandleEnemyDeath;
        activeEnemies.Remove(enemy);

        if (AllWavesSpawned && activeEnemies.Count == 0)
        {
            OnAllEnemiesCleared?.Invoke();
            OnAllEnemiesClearedClientRpc();
        }
    }
    
    [ClientRpc]
    private void OnWaveStartedClientRpc(int waveIndex)
    {
        if (IsHost) return;
        
        if (waveIndex >= 0 && waveIndex < waves.Length)
        {
            OnWaveStarted?.Invoke(waveIndex + 1, waves[waveIndex]);
        }
    }

    [ClientRpc]
    private void OnWaveCompletedClientRpc(int waveIndex)
    {
        if (IsHost) return;
        OnWaveCompleted?.Invoke(waveIndex + 1);
    }

    [ClientRpc]
    private void OnAllEnemiesClearedClientRpc()
    {
        if (IsHost) return;
        OnAllEnemiesCleared?.Invoke();
    }

    public IReadOnlyList<Enemy> GetActiveEnemies()
    {
        return activeEnemies;
    }
}