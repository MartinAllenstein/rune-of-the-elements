using System;
using UnityEngine;

public class TestEnemy : MonoBehaviour
{
    public static event Action<TestEnemy> OnEnemyKilled;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float obstacleCheckDistance = 0.6f;
    [SerializeField] private LayerMask obstacleLayerMask; // Layer เดินชนแล้วหยุด

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    private Rigidbody rb;
    private bool isWalking = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Enemy requires a Rigidbody component!", this);
        }
        currentHealth = maxHealth;
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (IsObstacleInFront())
        {
            isWalking = false;
        }
        else
        {
            isWalking = true;
        }

        if (isWalking)
        {
            Vector3 newPosition = rb.position + transform.right * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);
        }
    }

    private bool IsObstacleInFront()
    {
        // ยิง Raycast จากตำแหน่งของ Enemy ไปข้างหน้า
        // เรายกตำแหน่งเริ่มต้นของ Raycast ขึ้นมาเล็กน้อยเพื่อไม่ให้ยิงลงพื้น
        Vector3 rayStartPoint = transform.position + Vector3.up * 0.5f; 
        
        return Physics.Raycast(rayStartPoint, transform.forward, obstacleCheckDistance, obstacleLayerMask);
    }

    // ฟังก์ชันสำหรับรับความเสียหาย (เพื่อให้ TestBase เรียกใช้ได้)
    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        //Debug.Log(gameObject.name + " took " + damageAmount + " damage. Health is now " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        //Debug.Log(gameObject.name + " has died.");
        // เพิ่มเอฟเฟกต์ตอนตาย หรือทำลาย GameObject ทิ้ง
        OnEnemyKilled?.Invoke(this);
        
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 rayStartPoint = transform.position + Vector3.up * 0.5f;
        Gizmos.DrawLine(rayStartPoint, rayStartPoint + transform.forward * obstacleCheckDistance);
    }
}