using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/TowerDataSO")]
public class TowerDataSO : ScriptableObject
{
    [Header("Tower Identity")]
    public string towerName;
    public GameObject towerPrefab;

    [Header("Attack Stats")]
    public float damage;
    public DamageTypeSO damageType;
    public float fireRate;
    public float attackRadius;
}
