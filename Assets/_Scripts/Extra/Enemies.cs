using System;
using System.Collections.Generic;
using UnityEngine;

public class Enemies : MonoBehaviour
{

    [System.Serializable]
    public class DamageResistance
    {
        public DamageTypeSO damageType;
        [Range(-1f, 1f)] // -100% (Weakness) to 100% (Resistance)
        public float resistancePercentage;
    }

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    private int currentWaypoint = 0;
    private Transform targetWaypoint;
    private WaypointPath path;
    public SpriteRenderer sR;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;
    [SerializeField] private List<DamageResistance> resistances; // Damage Resistance Lis

    [Header("Attack Settings")]
    public float damage = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    private float attackTimer;
    private TheBase baseHealth;


    public event Action<Enemies> OnDeath;

    public void Initialize(WaypointPath assignedPath, TheBase baseRef)
    {
        path = assignedPath;
        baseHealth = baseRef;
        currentWaypoint = 0;
        targetWaypoint = path.GetWaypoint(0);
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (baseHealth == null) return;

        float dist = Vector3.Distance(transform.position, baseHealth.transform.position);
        if (dist <= attackRange)
        {
            AttackBase();
            return;
        }

        MoveAlongPath();
    }
    private void MoveAlongPath()
    {
        if (targetWaypoint == null) return;

        Vector3 dir = (targetWaypoint.position - transform.position).normalized;

        // Move toward target
        transform.position += dir * moveSpeed * Time.deltaTime;
        transform.forward = Vector3.Lerp(transform.forward, dir, Time.deltaTime * 10f);

        //Flip sprite based on movement direction (X-axis)
        if (dir.x > 0.05f)
            sR.flipX = true; // Facing right
        else if (dir.x < -0.05f)
            sR.flipX = false; // Facing left

        // Check if close enough to next waypoint
        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.2f)
        {
            currentWaypoint++;
            if (currentWaypoint < path.WaypointCount)
                targetWaypoint = path.GetWaypoint(currentWaypoint);
            else
                ReachBase();
        }
    }

    private void ReachBase()
    {
        // Optional: deal instant damage when reaching base
        //baseHealth.TakeDamage(damage);
        //Die();
    }

    private void AttackBase()
    {
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            baseHealth.TakeDamage(damage);
            attackTimer = attackCooldown;
        }
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
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        OnDeath?.Invoke(this);
        Destroy(gameObject);
    }
}
