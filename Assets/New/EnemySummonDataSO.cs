using UnityEngine;

[CreateAssetMenu()]
public class EnemySummonDataSO : ScriptableObject
{
    public GameObject enemyPrefab;
    public int enemyID;
    public string enemyName;
}
