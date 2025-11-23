using System;
using System.Collections.Generic;
using UnityEngine;

public class Enemy2 : MonoBehaviour
{
    public static event Action<Enemy2> OnEnemyKilled;
    
    [System.Serializable]
    public class DamageResistance
    {
        public DamageTypeSO damageType;
        [Range(-1f, 1f)] // -100% (Weakness) to 100% (Resistance)
        public float resistancePercentage; 
    }
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float obstacleCheckDistance = 0.6f;
    [SerializeField] private LayerMask obstacleLayerMask; // Layer เดินชนแล้วหยุด

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private List<DamageResistance> resistances; // Damage Resistance List
    private float currentHealth;
    
    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 50f;
    [SerializeField] private float attackRate = 1f;
    [SerializeField] private float attackDistance = 1.5f;
    private float attackCountdown = 0f;

    private Rigidbody rb;
    private bool isWalking = true;
    private Transform targetBase;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Enemy requires a Rigidbody component!", this);
        }
        currentHealth = maxHealth;
    }

    private void Start()
    {
        // if (TheBase.Instance != null)
        // {
        //     targetBase = TheBase.Instance.transform;
        // }
    }

    private void FixedUpdate()
    {
        if (targetBase == null) return;
        
        HandleMovement();
    }

    private void Update()
    {
        if (targetBase == null) return;
        
        attackCountdown -= Time.deltaTime;
        
        // Check Attack Distance
        if (Vector3.Distance(transform.position, targetBase.position) <= attackDistance)
        {
            
            isWalking = false;
            
            if (attackCountdown <= 0f)
            {
                AttackBase();
                attackCountdown = 1f / attackRate;
            }
        }
        else
        {
            isWalking = true;
        }    
    }

    private void HandleMovement()
    {
        // if (IsObstacleInFront())
        // {
        //     isWalking = false;
        // }
        // else
        // {
        //     isWalking = true;
        // }

        if (isWalking)
        {
            // Go for TheBase
            Vector3 direction = (targetBase.position - rb.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, lookRotation, Time.fixedDeltaTime * 5f));

            Vector3 newPosition = rb.position + transform.right * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);
        }
    }

    private bool IsObstacleInFront()
    {
        // ยิง Raycast จากตำแหน่งของ Enemy ไปข้างหน้า
        // เรายกตำแหน่งเริ่มต้นของ Raycast ขึ้นมาเล็กน้อยเพื่อไม่ให้ยิงลงพื้น
        Vector3 rayStartPoint = transform.position + Vector3.right * 0.5f; 
        
        return Physics.Raycast(rayStartPoint, transform.forward, obstacleCheckDistance, obstacleLayerMask);
    }
    
    private void AttackBase()
    {
        //TheBase.Instance.TakeDamage(attackDamage);
        // Attack Animation 
    }
    
    public void TakeDamage(float baseDamage, DamageTypeSO damageType)
    {
        float multiplier = 1f;

        foreach (var res in resistances)
        {
            if (res.damageType == damageType)
            {
                // นำค่า % ต้านทานมาคำนวณเป็นตัวคูณ
                // เช่น ต้านทาน 20% (0.2) -> multiplier = 0.8
                // แพ้ทาง 30% (-0.3) -> multiplier = 1.3
                multiplier = 1 - res.resistancePercentage;
                break;
            }
        }
        
        float finalDamage = baseDamage * multiplier;
        currentHealth -= finalDamage;
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        //Debug.Log(gameObject.name + " has died.");
        // Die Efx
        OnEnemyKilled?.Invoke(this);
        
        Destroy(gameObject);
    }

    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.yellow;
    //     Vector3 rayStartPoint = transform.position + Vector3.right * 0.5f;
    //     Gizmos.DrawLine(rayStartPoint, rayStartPoint + transform.forward * obstacleCheckDistance);
    // }
}