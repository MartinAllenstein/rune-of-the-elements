using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ActiveTower : NetworkBehaviour, IHasProgress
{
    public event EventHandler <IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    
    [Header("Configuration")]
    [SerializeField] private TowerDataSO towerData;
    [SerializeField] private GameObject emptyTowerPrefab;
    [SerializeField] private float activeDuration = 10f;
    [SerializeField] private Transform firePoint;

    [Header("Targeting")]
    private Transform currentTarget;
    private float fireCountdown = 0f;
    private float targetUpdateInterval = 0.2f; // check for new targets every 0.2s
    private float targetUpdateTimer = 0f;

    private NetworkVariable<float> activeTimer = new NetworkVariable<float>(0f);
    
    private WaveSpawner waveSpawner;

    public override void OnNetworkSpawn()
    {
        waveSpawner = FindFirstObjectByType<WaveSpawner>();

        if (IsServer)
        {
            activeTimer.Value = activeDuration;
        }
    }
    private void Update()
    {
        HandleActiveTimerUI();
        
        if (!IsServer) return;

        activeTimer.Value -= Time.deltaTime;
        if (activeTimer.Value <= 0f)
        {
            HandleDeactivation();
            return;
        }
        
        fireCountdown -= Time.deltaTime;
        targetUpdateTimer -= Time.deltaTime;

        if (targetUpdateTimer <= 0f)
        {
            targetUpdateTimer = targetUpdateInterval;
            UpdateTargetOptimized();
        }

        if (currentTarget != null && fireCountdown <= 0f)
        {
            Attack();
            fireCountdown = 1f / towerData.fireRate;
        }
    }
    
    private void HandleActiveTimerUI()
    {
        float progress = activeTimer.Value / activeDuration;
        
        progress = Mathf.Clamp01(progress);

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = progress
        });
    }
    
    private void HandleDeactivation()
    {
        // build EmptyTower
        GameObject emptyTowerObj = Instantiate(emptyTowerPrefab, transform.position, transform.rotation);
        emptyTowerObj.GetComponent<NetworkObject>().Spawn(true);

        // destroy ActiveTower
        GetComponent<NetworkObject>().Despawn(true);
    }

    private void Attack()
    {
        if (currentTarget == null) return;

        GameObject projectileGO = Instantiate(towerData.projectilePrefab, firePoint.position, firePoint.rotation);
        
        NetworkObject projectileNetObj = projectileGO.GetComponent<NetworkObject>();
        projectileNetObj.Spawn(true);

        Projectile projectile = projectileGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Seek(currentTarget, towerData);
        }
    }

    private void UpdateTargetOptimized()
    {
        if (waveSpawner == null) return;

        IReadOnlyList<Enemy> allEnemies = waveSpawner.GetActiveEnemies();
        if (allEnemies == null || allEnemies.Count == 0)
        {
            currentTarget = null;
            return;
        }

        Transform closestEnemy = null;
        float closestDistanceToBase = Mathf.Infinity;

        foreach (Enemy enemy in allEnemies)
        {
            if (enemy == null) continue;

            float distanceToTower = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToTower > towerData.attackRadius) continue; 

            TheBase targetBase = TheBase.GetNearestBase(enemy.transform.position); 
            
            if (targetBase != null)
            {
                float distanceToBase = Vector3.Distance(enemy.transform.position, targetBase.transform.position);
                
                if (distanceToBase < closestDistanceToBase)
                {
                    closestDistanceToBase = distanceToBase;
                    closestEnemy = enemy.transform;
                }
            }
        }

        currentTarget = closestEnemy;
    }

    private void OnDrawGizmosSelected()
    {
        if (towerData == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, towerData.attackRadius);
    }
}
