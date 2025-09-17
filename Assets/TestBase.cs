using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TestBase : BaseCounter
{
    [System.Serializable]
    public class AmmoTypeVisual
    {
        public KitchenObjectSO ammoKitchenObjectSO; // กระสุนที่ใช้
        public GameObject visualGameObject;       // Sprite ที่จะแสดงผล
    }
    
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
    
    [Header("Visuals & Ammo")]
    [SerializeField] private List<AmmoTypeVisual> ammoVisuals; // ลิสต์สำหรับจับคู่กระสุนกับ Sprite
    [SerializeField] private Transform kitchenObjectHoldPoint;
    
    [Header("Targeting")]
    private Transform currentTarget;
    private List<Transform> enemiesInRange = new List<Transform>();
    private float fireCountdown = 0f;
    
    // [Header("Visuals")]
    // [SerializeField] private GameObject firingSprite;
    // [SerializeField] private Transform kitchenObjectHoldPoint;
    
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
        
        UpdateAmmoVisual(null);
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
            KitchenObjectSO placedAmmoSO = player.GetKitchenObject().GetKitchenObjectSO();
            
            // Check the item
            if (ammoVisuals.Any(ammo => ammo.ammoKitchenObjectSO == placedAmmoSO))
            {
                player.GetKitchenObject().SetKitchenObjectParent(this);

                // Firing
                if (firingCoroutine != null)
                {
                    StopCoroutine(firingCoroutine);
                }
                firingCoroutine = StartCoroutine(FiringCoroutine(placedAmmoSO));
            }
            
        }
    }
    
    private IEnumerator FiringCoroutine(KitchenObjectSO activeAmmoSO)
    {
        currentState = BaseState.Firing;
        UpdateAmmoVisual(activeAmmoSO); // เปิด Sprite ที่ถูกต้อง
        Debug.Log("State changed to: FIRING with " + activeAmmoSO.objectName);

        yield return new WaitForSeconds(10f);

        if (HasKitchenObject())
        {
            GetKitchenObject().DestroySelf();
        }

        currentState = BaseState.Idle;
        UpdateAmmoVisual(null); // ปิด Sprite ทั้งหมด
        Debug.Log("State changed to: IDLE");
    }
    
    private void UpdateAmmoVisual(KitchenObjectSO activeAmmoSO)
    {
        foreach (var ammo in ammoVisuals)
        {
            if (ammo.visualGameObject != null)
            {
                // ถ้า SO ของกระสุนตรงกับที่ใช้งานอยู่ ให้เปิด Sprite, ถ้าไม่ตรงให้ปิด
                bool isActive = (activeAmmoSO != null && ammo.ammoKitchenObjectSO == activeAmmoSO);
                ammo.visualGameObject.SetActive(isActive);
            }
        }
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
