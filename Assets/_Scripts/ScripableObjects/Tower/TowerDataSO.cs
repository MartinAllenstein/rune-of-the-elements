using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/TowerDataSO")]
public class TowerDataSO : ScriptableObject
{
    public enum AttackType
    {
        Projectile, // ยิงกระสุนปกติ
        AreaOfEffect, // ระเบิดวงกว้าง
        DamageZone    // สร้างพื้นที่ดาเมจต่อเนื่อง (บ่อพิษ/น้ำแข็ง)
    }
    
    [Header("Tower Identity")]
    public string towerName;
    public GameObject towerPrefab;

    [Header("Attack Stats")]
    public AttackType attackType;
    public float damage;
    public DamageTypeSO damageType;
    public float fireRate;
    public float attackRadius;
    
    [Header("Projectile (For Projectile Type)")]
    public GameObject projectilePrefab;
    public string projectilePoolTag; // Tag for Object Pooler
    
    [Header("Trajectory (For Projectile Type)")]
    public bool useArc;
    public AnimationCurve arcCurve;
    public float arcHeight = 2f; // ความสูงสูงสุดของส่วนโค้ง
    
    [Header("Explosion (For AoE Type)")]
    public float chargeTime = 1.0f; // เวลาชาร์จก่อนระเบิด
    public float explosionRadius = 3.0f; // รัศมีระเบิด
    public GameObject chargeVfxPrefab; // Effect ตอนชาร์จ
    public GameObject explosionVfxPrefab;
    
    [Header("Damage Zone (Puddle/Field)")]
    public GameObject zonePrefab;
    public float zoneDuration = 3f;    // บ่ออยู่นานกี่วินาที
    public float zoneTickRate = 0.5f;  // ทำดาเมจทุกๆ กี่วินาที
    [Range(0f, 1f)]
    public float slowMultiplier = 0.5f;
    
    [Header("Chain Lightning (For Projectile Type)")]
    public int chainBounces = 0; // จำนวนครั้งที่จะชิ่ง (0 = ปกติ, 1+ = ชิ่ง)
    public float chainRange = 5f; // ระยะค้นหาศัตรูตัวถัดไปที่จะชิ่งไปหา
    public float chainDamageMultiplier = 0.8f; // (Optional) ดาเมจลดลงทุกครั้งที่ชิ่ง (เช่น เหลือ 80%)
}
