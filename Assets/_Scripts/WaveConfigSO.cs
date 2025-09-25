using UnityEngine;

[CreateAssetMenu(fileName = "New Wave Config", menuName = "Wave Config")]
public class WaveConfigSO : ScriptableObject
{
    [Header("Wave Settings")]
    public GameObject enemyPrefab; // Prefab ของศัตรูที่จะสปอว์นในเวฟนี้
    public int enemyCount;         // จำนวนศัตรูในเวฟนี้
    public float spawnInterval;    // ความเร็วในการสปอว์นแต่ละตัว (วินาที)
}