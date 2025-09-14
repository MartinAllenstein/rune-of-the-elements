using UnityEngine;
using System.Collections;

public class TestEnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject enemyPrefab;    // Prefab ของศัตรูที่จะสร้าง
    public float spawnInterval = 3f;  // เวลาหน่วงระหว่างการสร้างแต่ละตัว (วินาที)
    public int maxEnemies = 10;       // จำนวนศัตรูสูงสุดที่จะสร้าง

    [Header("Spawn Area")]
    public Transform[] spawnPoints;

    private int enemiesSpawned = 0;   // ตัวนับจำนวนศัตรูที่สร้างไปแล้ว

    // ฟังก์ชันนี้จะถูกเรียกเมื่อเกมเริ่มต้น
    void Start()
    {
        // ตรวจสอบว่าได้ตั้งค่า Prefab และจุดเกิดหรือยัง
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab not set in the spawner!", this);
            return;
        }
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned to the spawner!", this);
            return;
        }

        // เริ่มกระบวนการสร้างศัตรู
        StartCoroutine(SpawnEnemyRoutine());
    }

    // Coroutine สำหรับจัดการลูปการสร้างศัตรู
    private IEnumerator SpawnEnemyRoutine()
    {
        // วนลูปไปเรื่อยๆ จนกว่าจะสร้างศัตรูครบตามจำนวน
        while (enemiesSpawned < maxEnemies)
        {
            // รอตามเวลาที่กำหนด
            yield return new WaitForSeconds(spawnInterval);

            // ทำการสร้างศัตรู
            SpawnAnEnemy();
        }

        Debug.Log("Spawner has finished spawning all enemies.");
    }

    private void SpawnAnEnemy()
    {
        // สุ่มเลือกจุดเกิดจาก Array ของ spawnPoints
        int spawnPointIndex = Random.Range(0, spawnPoints.Length);
        Transform selectedSpawnPoint = spawnPoints[spawnPointIndex];

        // สร้าง (Instantiate) ศัตรูจาก Prefab ณ ตำแหน่งและทิศทางของจุดเกิด
        Instantiate(enemyPrefab, selectedSpawnPoint.position, selectedSpawnPoint.rotation);

        // เพิ่มจำนวนตัวนับ
        enemiesSpawned++;
        Debug.Log("Spawned enemy number " + enemiesSpawned);
    }
}