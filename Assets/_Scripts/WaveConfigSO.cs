using UnityEngine;

[CreateAssetMenu(fileName = "New Wave Config", menuName = "Wave Config")]
public class WaveConfigSO : ScriptableObject
{
    [Header("Wave Settings")]
    public GameObject enemyPrefab;
    public int enemyCount;
    public float spawnInterval;    // spawn per second
}