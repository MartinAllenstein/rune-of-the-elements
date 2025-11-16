using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Transform target;
    private TowerDataSO towerData;
    private float moveSpeed = 20f; // ความเร็วของกระสุน

    public void Seek(Transform _target, TowerDataSO _towerData)
    {
        target = _target;
        towerData = _towerData;
    }

    void Update()
    {
        // If the target is lost destroy the ammo
        if (target == null)
        {
            gameObject.SetActive(false); // return Pool
            return;
        }

        Vector3 direction = target.position - transform.position;
        float distanceThisFrame = moveSpeed * Time.deltaTime;
        
        if (direction.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(direction.normalized * distanceThisFrame, Space.World);

        transform.LookAt(target);
    }

    void HitTarget()
    {
        // แสดง VFX ที่ตำแหน่งที่ชน (ถ้ามี)
        // Instantiate(impactEffect, transform.position, transform.rotation);

        Enemies enemy = target.GetComponent<Enemies>();
        if (enemy != null)
        {
            enemy.TakeDamage(towerData.damage, towerData.damageType);
        }

        // return Pool
        gameObject.SetActive(false);
    }
}