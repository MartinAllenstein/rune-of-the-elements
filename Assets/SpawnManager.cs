using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Wave Configuration")]
    [SerializeField] private List<WaveConfigSO> waves; // ลิสต์ของเวฟทั้งหมด
    [SerializeField] private float timeBetweenWaves = 5f; // เวลาพักระหว่างเวฟ

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints; // ตำแหน่งที่จะสปอว์นศัตรู

    private int currentWaveIndex = 0;
    private int enemiesRemaining;
    private bool isSpawning = false;

    private void OnEnable()
    {
        // สมัครรับ event เมื่อศัตรูตาย
        TestEnemy.OnEnemyKilled += HandleEnemyKilled;
    }

    private void OnDisable()
    {
        // ยกเลิกการรับ event
        TestEnemy.OnEnemyKilled -= HandleEnemyKilled;
    }

    private void Start()
    {
        StartCoroutine(StartNextWave());
    }

    private void HandleEnemyKilled(TestEnemy enemy)
    {
        enemiesRemaining--;

        // ถ้าศัตรูหมดเวฟแล้ว และยังไม่ถึงเวฟสุดท้าย
        if (enemiesRemaining <= 0 && !isSpawning && currentWaveIndex < waves.Count)
        {
            StartCoroutine(StartNextWave());
        }
    }

    private IEnumerator StartNextWave()
    {
        Debug.Log("Preparing for Wave " + (currentWaveIndex + 1));
        
        // รอเวลาพักระหว่างเวฟ
        yield return new WaitForSeconds(timeBetweenWaves);

        // เริ่มสปอว์นเวฟปัจจุบัน
        StartCoroutine(SpawnWave(waves[currentWaveIndex]));
    }

    private IEnumerator SpawnWave(WaveConfigSO wave)
    {
        Debug.Log("Wave " + (currentWaveIndex + 1) + " starting!");
        isSpawning = true;
        enemiesRemaining = wave.enemyCount;

        for (int i = 0; i < wave.enemyCount; i++)
        {
            // สุ่มตำแหน่งเกิดจากลิสต์ spawnPoints
            Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // สร้างศัตรู
            Instantiate(wave.enemyPrefab, randomSpawnPoint.position, randomSpawnPoint.rotation);

            // รอตามเวลาที่กำหนดก่อนจะสปอว์นตัวถัดไป
            yield return new WaitForSeconds(wave.spawnInterval);
        }

        isSpawning = false;
        currentWaveIndex++;

        // ถ้าจบทุกเวฟแล้ว
        if (currentWaveIndex >= waves.Count)
        {
            Debug.Log("All waves completed!");
        }
    }
}