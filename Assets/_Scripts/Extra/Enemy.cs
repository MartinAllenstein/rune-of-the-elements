using System;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;

public class Enemy : NetworkBehaviour
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
    [SerializeField] private List<DamageResistance> resistances;

    [Header("Attack Settings")]
    public float damage = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    private float attackTimer = 0f;
    private TheBase baseHealth;

    private bool isAttackingBase = false;

    private NetworkVariable<bool> isFacingRight = new NetworkVariable<bool>(true);

    public event Action<Enemy> OnDeath;

    public override void OnNetworkSpawn()
    {
        isFacingRight.OnValueChanged += OnFacingChanged;
        // Initial state
        if (sR != null) sR.flipX = isFacingRight.Value;
    }

    private void OnFacingChanged(bool prev, bool current)
    {
        if (sR != null) sR.flipX = current;
    }

    public void Initialize(WaypointPath assignedPath, TheBase baseRef)
    {
        path = assignedPath;
        baseHealth = baseRef;
        currentWaypoint = 0;
        targetWaypoint = path.GetWaypoint(0);
        currentHealth = maxHealth;
        isAttackingBase = false;
    }

    private void Update()
    {
        if (!IsServer) return;
        if (baseHealth == null) return;

        if (isAttackingBase)
        {
            AttackBase();
            FaceBase();
        }
        else
        {
            float distToBase = Vector3.Distance(transform.position, baseHealth.transform.position);

            if (distToBase <= attackRange)
            {
                isAttackingBase = true;
                attackTimer = 0f; // attack instantly
                AttackBase();
            }
            else
            {
                MoveAlongPath();
            }
        }
    }

    private void MoveAlongPath()
    {
        if (targetWaypoint == null) return;

        Vector3 dir = (targetWaypoint.position - transform.position).normalized;

        transform.position += dir * moveSpeed * Time.deltaTime;
        transform.forward = Vector3.Lerp(transform.forward, dir, Time.deltaTime * 10f);

        // Flip sprite based on movement direction (X-axis)
        if (dir.x > 0.05f)
            isFacingRight.Value = true; // Facing right
        else if (dir.x < -0.05f)
            isFacingRight.Value = false; // Facing left

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
        isAttackingBase = true;
        attackTimer = 0f; // start attacking immediately
    }

    private void AttackBase()
    {
        if (baseHealth == null) return;

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            baseHealth.TakeDamage(damage);
            attackTimer = attackCooldown;
        }
    }

    private void FaceBase()
    {
        if (baseHealth == null || sR == null) return;

        Vector3 dirToBase = baseHealth.transform.position - transform.position;

        if (dirToBase.x > 0.05f)
            isFacingRight.Value = true; // facing right
        else if (dirToBase.x < -0.05f)
            isFacingRight.Value = false; // facing left
    }

    public void TakeDamage(float baseDamage, DamageTypeSO damageType)
    {
        float multiplier = 1f;
        foreach (var res in resistances)
        {
            if (res.damageType == damageType)
            {
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
        OnDeath?.Invoke(this);
        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn(true);
        }
        else
        {
            // Should not happen if logic is server only, but safe fallback
            Destroy(gameObject);
        }
    }
}
