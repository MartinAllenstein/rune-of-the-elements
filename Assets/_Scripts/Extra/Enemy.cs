using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;

public class Enemy : NetworkBehaviour, IHasProgress
{
    [System.Serializable]
    public class DamageResistance
    {
        public DamageTypeSO damageType;
        [Range(-1f, 1f)] // -100% (Weakness) to 100% (Resistance)
        public float resistancePercentage;
    }
    
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform visualTransform; // To Flip Scale (GameObject Sprite)

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    private int currentWaypoint = 0;
    private Transform targetWaypoint;
    private WaypointPath path;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    
    private NetworkVariable<float> currentHealth = new NetworkVariable<float>(100f);
    
    [SerializeField] private List<DamageResistance> resistances;

    [Header("Attack Settings")]
    public float damage = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    private float attackTimer = 0f;
    private TheBase baseHealth;
    private TheBase targetBase;

    private bool isAttackingBase = false;
    private bool isDead = false;
    
    // Animation Parameters
    private const string IS_WALKING = "IsWalking";
    private const string ATTACK_TRIGGER = "Attack";
    private const string DIE_TRIGGER = "Die";

    private NetworkVariable<bool> isFacingRight = new NetworkVariable<bool>(true);

    public event Action<Enemy> OnDeath;
    
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    
    public override void OnNetworkSpawn()
    {
        isFacingRight.OnValueChanged += OnFacingChanged;
        UpdateVisualFlip(isFacingRight.Value);
        
        currentHealth.OnValueChanged += OnHealthChanged;
    }
    
    public override void OnNetworkDespawn()
    {
        isFacingRight.OnValueChanged -= OnFacingChanged;
        currentHealth.OnValueChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(float previousValue, float newValue)
    {
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = newValue / maxHealth
        });
    }

    private void OnFacingChanged(bool prev, bool current)
    {
        UpdateVisualFlip(current);
    }
    
    private void UpdateVisualFlip(bool facingRight)
    {
        if (visualTransform == null) return;

        if (facingRight)
        {
            visualTransform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            visualTransform.localScale = new Vector3(1, 1, 1);
        }
    }

    public void Initialize(WaypointPath assignedPath, TheBase baseRef_Ignored)
    {
        path = assignedPath;
        
        targetBase = TheBase.GetNearestBase(transform.position);
        baseHealth = targetBase;
        
        currentWaypoint = 0;
        targetWaypoint = path.GetWaypoint(0);
        currentHealth.Value = maxHealth;
        
        isAttackingBase = false;
        isDead = false;
        
        UpdateWalkingStateClientRpc(true);
    }

    private void Update()
    {
        if (!IsServer) return;
        if (targetBase == null) return;
        if (isDead) return;

        if (isAttackingBase)
        {
            AttackBase();
            FaceTarget(targetBase.transform.position);
            
            UpdateWalkingStateClientRpc(false);
        }
        else
        {
            float distToBase = Vector3.Distance(transform.position, targetBase.transform.position);

            if (distToBase <= attackRange)
            {
                isAttackingBase = true;
                attackTimer = 0f; // attack instantly
            }
            else
            {
                MoveAlongPath();
                
                UpdateWalkingStateClientRpc(true);
            }
        }
    }

    private void MoveAlongPath()
    {
        if (targetWaypoint == null) return;

        Vector3 dir = (targetWaypoint.position - transform.position).normalized;

        transform.position += dir * moveSpeed * Time.deltaTime;

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
    
    private void FaceTarget(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        if (dir.x > 0.05f)
            isFacingRight.Value = true;
        else if (dir.x < -0.05f)
            isFacingRight.Value = false;
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
            TriggerAttackAnimationClientRpc();
            
            baseHealth.TakeDamage(damage);
            
            attackTimer = attackCooldown;
        }
    }

    public void TakeDamage(float baseDamage, DamageTypeSO damageType)
    {
        if (isDead) return;
        
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
        currentHealth.Value -= finalDamage;

        if (currentHealth.Value <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        
        OnDeath?.Invoke(this);
        
        TriggerDieAnimationClientRpc();
        
        // รอเวลาให้ Animation เล่นจบก่อนค่อยลบ Object
        StartCoroutine(DespawnAfterDelay(1.0f));
    }
    
    private IEnumerator DespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn(true);
        }
    }
    
    [ClientRpc]
    private void UpdateWalkingStateClientRpc(bool isWalking)
    {
        if (animator != null)
        {
            animator.SetBool(IS_WALKING, isWalking);
        }
    }

    [ClientRpc]
    private void TriggerAttackAnimationClientRpc()
    {
        if (animator != null)
        {
            animator.SetTrigger(ATTACK_TRIGGER);
        }
    }

    [ClientRpc]
    private void TriggerDieAnimationClientRpc()
    {
        if (animator != null)
        {
            animator.SetTrigger(DIE_TRIGGER);
        }
    }
}
