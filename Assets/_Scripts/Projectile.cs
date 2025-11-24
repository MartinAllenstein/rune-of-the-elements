using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    private Transform target;
    private TowerDataSO towerData;
    private float moveSpeed = 20f;

    public void Seek(Transform _target, TowerDataSO _towerData)
    {
        target = _target;
        towerData = _towerData;
    }

    void Update()
    {
        if (!IsServer) return;
        
        // If the target is lost destroy the ammo
        if (target == null)
        {
            GetComponent<NetworkObject>().Despawn();
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

        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(towerData.damage, towerData.damageType);
        }

        GetComponent<NetworkObject>().Despawn();
    }
}