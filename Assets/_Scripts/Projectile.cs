using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [Header("Visuals")]
    [SerializeField] private GameObject hitVfxPrefab;
    [SerializeField] private float vfxLifetime = 2f; // เอฟเฟกต์อยู่นานแค่ไหน
    
    private Transform target;
    private TowerDataSO towerData;
    private float moveSpeed = 20f;

    private Vector3 startPosition;
    private float totalDistance;
    private float progress;
    
    private int bouncesLeft;
    private List<ulong> hitHistory = new List<ulong>();
    
    public void Seek(Transform _target, TowerDataSO _towerData)
    {
        target = _target;
        towerData = _towerData;
        
        if (hitHistory.Count == 0) 
        {
            bouncesLeft = towerData.chainBounces;
        }
        
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
    
    public void SetBounceData(int _bouncesLeft, List<ulong> _history)
    {
        bouncesLeft = _bouncesLeft;
        hitHistory = new List<ulong>(_history); // Copy list มา
    }

    void HitTarget()
    {
        // Hit VFX
        if (hitVfxPrefab != null)
        {
            SpawnHitVfxClientRpc(transform.position, Quaternion.identity);
        }

        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(towerData.damage, towerData.damageType);
            
            if (enemy.TryGetComponent(out NetworkObject enemyNetObj))
            {
                hitHistory.Add(enemyNetObj.NetworkObjectId);
            }
        }
        
        if (bouncesLeft > 0)
        {
            ChainToNextTarget();
        }

        GetComponent<NetworkObject>().Despawn();
    }
    
    private void ChainToNextTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, towerData.chainRange);
        
        Transform bestTarget = null;
        float closestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                if (enemy.TryGetComponent(out NetworkObject enemyNetObj))
                {
                    if (hitHistory.Contains(enemyNetObj.NetworkObjectId)) continue;
                }

                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    bestTarget = enemy.transform;
                }
            }
        }

        if (bestTarget != null)
        {
            SpawnChainProjectile(bestTarget);
        }
    }
    
    private void SpawnChainProjectile(Transform newTarget)
    {
        GameObject projectileGO = Instantiate(towerData.projectilePrefab, transform.position, Quaternion.identity);
        NetworkObject projectileNetObj = projectileGO.GetComponent<NetworkObject>();
        projectileNetObj.Spawn(true);

        Projectile newProjectile = projectileGO.GetComponent<Projectile>();
        if (newProjectile != null)
        {
            newProjectile.Seek(newTarget, towerData);
            
            newProjectile.SetBounceData(bouncesLeft - 1, hitHistory);
        }
    }
    
    [ClientRpc]
    private void SpawnHitVfxClientRpc(Vector3 position, Quaternion rotation)
    {
        GameObject vfxObj = Instantiate(hitVfxPrefab, position, Quaternion.identity);
        
        Destroy(vfxObj, vfxLifetime);
    }
}