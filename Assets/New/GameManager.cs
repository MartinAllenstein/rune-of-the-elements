using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static Queue<int> enemyIDToSummon;
    
    private bool loopShouldEnd;
    void Start()
    {
        enemyIDToSummon = new Queue<int>();
        EntitySummoner.Init();
    }

    IEnumerable GameLoop()
    {
        while (loopShouldEnd == false)
        {
            // spawn enemy
            
            // spawn towers
            
            // move enemy
            
            // tick towers
            
            // apply effects
            
            // damage enemy
            
            // remove enemy
            
            // remove towers
            
            yield return null;
        }
    }
    
    //public static void Enq
}
