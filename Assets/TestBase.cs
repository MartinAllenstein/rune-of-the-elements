using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBase : BaseCounter
{
    private enum BaseState
    {
        Idle,
        Firing
    }
    
    [Header("State Management")]
    [SerializeField] private BaseState currentState;
    private Coroutine firingCoroutine;
    
    [Header("Attack Settings")]
    public float attackRadius = 15f;
    public float fireRate = 1f;
    public float damage = 25f;
    public SphereCollider rangeCollider;

    [SerializeField] private List<KitchenObjectSO> acceptedKitchenObjectSOs;
    
    [Header("Targeting")]
    private Transform currentTarget;
    private List<Transform> enemiesInRange = new List<Transform>();
    private float fireCountdown = 0f;
    
    [Header("Visuals")]
    [SerializeField] private GameObject firingSprite;
    [SerializeField] private Transform kitchenObjectHoldPoint;
    
    void Start()
    {
        currentState = BaseState.Idle;
        // Set Sphere Collider (Trigger) = attackRadius
        
        //SphereCollider rangeCollider = GetComponent<SphereCollider>();
        if (rangeCollider != null)
        {
            rangeCollider.radius = attackRadius;
            rangeCollider.isTrigger = true;
        }
        else
        {
            Debug.LogError("Base requires a SphereCollider component for range detection.", this);
        }
        
        if (firingSprite != null)
        {
            firingSprite.SetActive(false);
        }
    }
    
    void Update()
    {
        if (currentState == BaseState.Firing)
        {
            fireCountdown -= Time.deltaTime;
            UpdateTarget();

            if (currentTarget != null && fireCountdown <= 0f)
            {
                Attack();
                fireCountdown = 1f / fireRate;
            }
        }
    }
    
    public override void Interact(Player player)
    {
        //Debug.Log("Interacting with Tower");
        
        // If TheBase doesn't have an Item AND Player have an Item
        if (!HasKitchenObject() && player.HasKitchenObject())
        {
            // Check the item
            if (acceptedKitchenObjectSOs.Contains(player.GetKitchenObject().GetKitchenObjectSO()))
            {
                player.GetKitchenObject().SetKitchenObjectParent(this);

                // Firing
                if (firingCoroutine != null)
                {
                    StopCoroutine(firingCoroutine);
                }
                firingCoroutine = StartCoroutine(FiringCoroutine());
            }
            
        }
    }
    
    private IEnumerator FiringCoroutine()
    {
        // เปลี่ยนสถานะเป็น Firing
        currentState = BaseState.Firing;
        if (firingSprite != null)
        {
            firingSprite.SetActive(true);
        }
        Debug.Log("State changed to: FIRING");

        // รอ 10 วินาที
        yield return new WaitForSeconds(10f);

        // ทำลายไอเท็มที่วางอยู่
        if (HasKitchenObject())
        {
            GetKitchenObject().DestroySelf();
        }

        // กลับสู่สถานะ Idle
        currentState = BaseState.Idle;
        if (firingSprite != null)
        {
            firingSprite.SetActive(false);
        }
        Debug.Log("State changed to: IDLE");
    }

    void UpdateTarget()
    {
        if (currentTarget == null)
        {
            enemiesInRange.RemoveAll(item => item == null);
            
            if (enemiesInRange.Count > 0)
            {
                currentTarget = enemiesInRange[0]; // เลือกตัวแรกในลิสต์เป็นเป้าหมาย
            }
        }
    }
    
    void Attack()
    {
        // Debug.Log("Attacking " + currentTarget.name);
        
        TestEnemy enemyScript = currentTarget.GetComponent<TestEnemy>();
        if (enemyScript != null)
        {
            enemyScript.TakeDamage(damage);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // ตรวจสอบว่าสิ่งที่เข้ามาคือ Enemy หรือไม่ (โดยดูจาก script Enemy)
        if (other.GetComponent<TestEnemy>() != null)
        {
            // เพิ่มศัตรูตัวนั้นเข้าไปในลิสต์
            if (!enemiesInRange.Contains(other.transform))
            {
                enemiesInRange.Add(other.transform);
                // Debug.Log(other.name + " entered range.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // ตรวจสอบว่าสิ่งที่ออกไปคือ Enemy หรือไม่
        if (other.GetComponent<TestEnemy>() != null)
        {
            // นำศัตรูตัวนั้นออกจากลิสต์
            enemiesInRange.Remove(other.transform);
            // Debug.Log(other.name + " exited range.");

            // ถ้าตัวที่ออกไปคือเป้าหมายปัจจุบัน ให้เคลียร์เป้าหมาย
            if (currentTarget == other.transform)
            {
                currentTarget = null;
            }
        }
    }
    
    // แสดงรัศมีการโจมตีใน Scene Editor เพื่อให้เห็นภาพ
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
    
    // public override Transform GetKitchenObjectFollowTransform()
    // {
    //     return kitchenObjectHoldPoint;
    // }
}
