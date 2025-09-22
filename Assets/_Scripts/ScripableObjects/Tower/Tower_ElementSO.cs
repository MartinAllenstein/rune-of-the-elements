using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Tower-ElementSO")]
public class Tower_ElementSO : ScriptableObject
{
    [System.Serializable]
    public class Mapping
    {
        public KitchenObjectSO elementType;
        public TowerDataSO resultingTower;
    }

    public List<Mapping> towerMappings;

    // Find Tower
    public bool TryGetTowerData(KitchenObjectSO element, out TowerDataSO towerData)
    {
        foreach (var map in towerMappings)
        {
            if (map.elementType == element)
            {
                towerData = map.resultingTower;
                return true;
            }
        }
        towerData = null;
        return false;
    }
}
