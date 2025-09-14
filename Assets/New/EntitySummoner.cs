using System.Collections.Generic;
using UnityEngine;

public class EntitySummoner : MonoBehaviour
{

    private static List<Enemy> enemyInGame;
    private static Dictionary<int, GameObject> enemyPrefabs;
    private static Dictionary<int, Queue<Enemy>> enemyObjectPools;
    
    private static bool isInitialized;
    
    public static void Init()
    {
        if (!isInitialized)
        {
            enemyPrefabs = new Dictionary<int, GameObject>();
            enemyObjectPools = new Dictionary<int, Queue<Enemy>>();
            enemyInGame = new List<Enemy>();
        
            EnemySummonDataSO[] Enemies = Resources.LoadAll<EnemySummonDataSO>("Enemies");

            foreach (EnemySummonDataSO enemy in Enemies)
            {
                enemyPrefabs.Add(enemy.enemyID, enemy.enemyPrefab);
                enemyObjectPools.Add(enemy.enemyID, new Queue<Enemy>());
            }
            isInitialized = true;
        }
        else
        {
            Debug.Log("Already Initialized");   
        }
    }

    private static Enemy SummonEnemy(int enemyID)
    {
        Enemy summonEnemy = null;

        if (enemyPrefabs.ContainsKey(enemyID))
        {
            Queue<Enemy> referencedQueue = enemyObjectPools[enemyID];

            if (referencedQueue.Count > 0)
            {
                // Dequeue Enemy and initialized
                
                summonEnemy = referencedQueue.Dequeue();
                summonEnemy.Init();
            }
            else
            {
                // Instantiate new instance of enemy and initialized
                GameObject newEnemy = Instantiate(enemyPrefabs[enemyID], Vector3.zero, Quaternion.identity);
                summonEnemy = newEnemy.GetComponent<Enemy>();
                summonEnemy.Init();
            }
        }
        else
        {
            Debug.Log($"Enemy ID {enemyID} don't exist");   
            return null;
        }

        return summonEnemy;
    }
    
}
