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
    
    [Header("Projectile")]
    public GameObject projectilePrefab;
    public string projectilePoolTag; // Tag for Object Pooler
    
    [Header("Trajectory")]
    public bool useArc;
    public AnimationCurve arcCurve;
    public float arcHeight = 2f; // ความสูงสูงสุดของส่วนโค้ง
}
