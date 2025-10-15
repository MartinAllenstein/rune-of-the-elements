using UnityEngine;
using System.Collections;

[System.Serializable]
public class EnemyWave
{
    public GameObject enemyPrefab;
    public int count;
    public float spawnInterval;
}

public class WaveSpawner : MonoBehaviour
{
    public EnemyWave[] waves;
    public WaypointPath path;
    public TheBase baseHealth;
    public Transform spawnPoint;

    private int currentWaveIndex = 0;

    private void Start()
    {
        StartCoroutine(SpawnWaves());
    }

    private IEnumerator SpawnWaves()
    {
        while (currentWaveIndex < waves.Length)
        {
            var wave = waves[currentWaveIndex];
            Debug.Log($"Spawning Wave {currentWaveIndex + 1}");

            for (int i = 0; i < wave.count; i++)
            {
                var enemyObj = Instantiate(wave.enemyPrefab, spawnPoint.position, Quaternion.identity);
                var enemy = enemyObj.GetComponent<Enemies>();
                enemy.Initialize(path, baseHealth);
                yield return new WaitForSeconds(wave.spawnInterval);
            }

            currentWaveIndex++;
            yield return new WaitForSeconds(5f); // delay before next wave
        }
    }
}
