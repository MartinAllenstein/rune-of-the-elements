using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    private Transform target;
    private TowerDataSO towerData;
    private float moveSpeed = 20f;

    private Vector3 startPosition;
    private float totalDistance;
    private float progress;
    
    public void Seek(Transform _target, TowerDataSO _towerData)
    {
        target = _target;
        towerData = _towerData;
        
        startPosition = transform.position;
        totalDistance = Vector3.Distance(startPosition, target.position);
        progress = 0f;
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

        if (towerData.useArc)
        {
            MoveArc();
        }
        else 
        {
            MoveLinear();
        }
    }
    
    private void MoveLinear()
    {
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
    
    private void MoveArc()
    {
        progress += (moveSpeed * Time.deltaTime) / totalDistance;

        if (progress >= 1f)
        {
            HitTarget();
            return;
        }

        Vector3 nextPosition = Vector3.Lerp(startPosition, target.position, progress);

        float heightOffset = towerData.arcCurve.Evaluate(progress) * towerData.arcHeight;

        nextPosition.y += heightOffset;

        transform.LookAt(nextPosition);
        
        transform.position = nextPosition;
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