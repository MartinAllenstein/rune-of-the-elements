using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveTower : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private TowerDataSO towerData; // This tower
    [SerializeField] private GameObject emptyTowerPrefab;
    [SerializeField] private float activeDuration = 10f;

    [Header("Targeting")]
    private Transform currentTarget;
    private List<Transform> enemiesInRange = new List<Transform>();
    private float fireCountdown = 0f;

    private void Start()
    {
        // ตั้งค่ารัศมีตามข้อมูลจาก TowerDataSO
        SphereCollider rangeCollider = GetComponent<SphereCollider>();
        if (rangeCollider != null)
        {
            rangeCollider.radius = towerData.attackRadius;
            rangeCollider.isTrigger = true;
        }
        
        StartCoroutine(DeactivateAfterTime());
    }

    private void Update()
    {
        fireCountdown -= Time.deltaTime;
        UpdateTarget();

        if (currentTarget != null && fireCountdown <= 0f)
        {
            Attack();
            fireCountdown = 1f / towerData.fireRate;
        }
    }

    private IEnumerator DeactivateAfterTime()
    {
        yield return new WaitForSeconds(activeDuration);
        
        Instantiate(emptyTowerPrefab, transform.position, transform.rotation);
        
        Destroy(gameObject);
    }
    
    void Attack()
    {
        TestEnemy enemyScript = currentTarget.GetComponent<TestEnemy>();
        if (enemyScript != null)
        {
            // send Element to Enemy
            enemyScript.TakeDamage(towerData.damage, towerData.damageType);
        }
    }
    void UpdateTarget()
    {
        // ถ้าเป้าหมายปัจจุบันหายไป (เช่น ตาย หรือออกจากระยะ)
        if (currentTarget == null)
        {
            // ลบศัตรูที่ตายแล้วออกจากลิสต์
            enemiesInRange.RemoveAll(item => item == null);

            // ถ้ายังมีศัตรูในระยะ ให้เลือกตัวแรกเป็นเป้าหมายใหม่
            if (enemiesInRange.Count > 0)
            {
                currentTarget = enemiesInRange[0];
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // ตรวจสอบว่าสิ่งที่เข้ามาคือศัตรูหรือไม่
        if (other.GetComponent<TestEnemy>() != null)
        {
            // เพิ่มศัตรูเข้าลิสต์ถ้ายังไม่มี
            if (!enemiesInRange.Contains(other.transform))
            {
                enemiesInRange.Add(other.transform);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // ตรวจสอบว่าสิ่งที่ออกไปคือศัตรูหรือไม่
        if (other.GetComponent<TestEnemy>() != null)
        {
            // นำศัตรูออกจากลิสต์
            enemiesInRange.Remove(other.transform);

            // ถ้าตัวที่ออกไปคือเป้าหมายปัจจุบัน ให้เคลียร์เป้าหมายเพื่อหาใหม่
            if (currentTarget == other.transform)
            {
                currentTarget = null;
            }
        }
    }

    // (Optional) Scene Editor
    private void OnDrawGizmosSelected()
    {
        if (towerData == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, towerData.attackRadius);
    }
}