using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnPoints : MonoBehaviour
{
    public static PlayerSpawnPoints Instance { get; private set; }

    [SerializeField] private List<Transform> spawnPoints;

    private void Awake()
    {
        Instance = this;
    }

    public Vector3 GetSpawnPosition(int playerIndex)
    {
        if (playerIndex < spawnPoints.Count)
        {
            return spawnPoints[playerIndex].position;
        }
        else
        {
            return transform.position; 
        }
    }
}