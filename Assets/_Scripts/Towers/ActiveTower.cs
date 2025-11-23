using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveTower : MonoBehaviour, IHasProgress
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

    private float activeTimer;
    
    private WaveSpawner waveSpawner;

    private void Start()
    {
        SphereCollider rangeCollider = GetComponent<SphereCollider>();
        if (rangeCollider != null)
        {
            rangeCollider.radius = towerData.attackRadius;
            rangeCollider.isTrigger = true;
        }

        activeTimer = activeDuration;

        waveSpawner = FindFirstObjectByType<WaveSpawner>();

        StartCoroutine(DeactivateAfterTime());
    }

    private void Update()
    {
        HandleActiveTimer();
        
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
    
    private void HandleActiveTimer()
    {
        activeTimer -= Time.deltaTime;

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = activeTimer / activeDuration
        });
    }

    private IEnumerator DeactivateAfterTime()
    {
        yield return new WaitForSeconds(activeDuration);
        Instantiate(emptyTowerPrefab, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    private void Attack()
    {
        if (currentTarget == null) return;

        GameObject projectileGO = ObjectPooler.Instance.SpawnFromPool(
            towerData.projectilePoolTag,
            firePoint.position,
            firePoint.rotation
        );

        if (projectileGO != null)
        {
            Projectile projectile = projectileGO.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Seek(currentTarget, towerData);
            }
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
            if (distanceToTower > towerData.attackRadius) continue; // only consider enemies in range

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
