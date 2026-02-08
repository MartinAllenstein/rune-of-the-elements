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
    
    [Header("Visuals")]
    [SerializeField] private LineRenderer rangeLineRenderer;
    [SerializeField] private int circleSegments = 50; // ความละเอียดของวงกลม
    [SerializeField] private float lineWidth = 0.1f; // ความหนาของเส้น

    [Header("Targeting")]
    private Transform currentTarget;
    private float fireCountdown = 0f;
    private float targetUpdateInterval = 0.2f; // check for new targets every 0.2s
    private float targetUpdateTimer = 0f;

    private NetworkVariable<float> activeTimer = new NetworkVariable<float>(0f);
    
    private WaveSpawner waveSpawner;
    
    private bool isChargingAttack = false;

    public override void OnNetworkSpawn()
    {
        waveSpawner = FindFirstObjectByType<WaveSpawner>();

        if (IsServer)
        {
            activeTimer.Value = activeDuration;
        }
        SetupRangeCircle();
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
        
        targetUpdateTimer -= Time.deltaTime;

        if (targetUpdateTimer <= 0f)
        {
            targetUpdateTimer = targetUpdateInterval;
            UpdateTargetOptimized();
        }

        fireCountdown -= Time.deltaTime;
        
        if (currentTarget != null && fireCountdown <= 0f)
        {
            if (towerData.attackType == TowerDataSO.AttackType.Projectile)
            {
                ShootProjectile();
                fireCountdown = 1f / towerData.fireRate;
            }
            else if (towerData.attackType == TowerDataSO.AttackType.AreaOfEffect)
            {
                if (!isChargingAttack)
                {
                    StartCoroutine(ExplosionAttackRoutine());
                }
            }
            else if (towerData.attackType == TowerDataSO.AttackType.DamageZone) 
            {
                // --- Damage Zone ---
                SpawnDamageZone();
                fireCountdown = 1f / towerData.fireRate;
            }
        }
    }
    
    private void SpawnDamageZone()
    {
        if (towerData.zonePrefab == null) return;

        GameObject zoneObj = Instantiate(towerData.zonePrefab, currentTarget.position, Quaternion.identity);
        
        zoneObj.GetComponent<NetworkObject>().Spawn(true);

        DamageZone zoneScript = zoneObj.GetComponent<DamageZone>();
        if (zoneScript != null)
        {
            zoneScript.Initialize(
                towerData.damage, 
                towerData.damageType, 
                towerData.slowMultiplier, 
                towerData.zoneDuration, 
                towerData.zoneTickRate
            );
        }
    }
    
    private void ShootProjectile()
    {
        GameObject projectileGO = Instantiate(towerData.projectilePrefab, firePoint.position, firePoint.rotation);
        NetworkObject projectileNetObj = projectileGO.GetComponent<NetworkObject>();
        projectileNetObj.Spawn(true);

        Projectile projectile = projectileGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Seek(currentTarget, towerData);
        }
    }
    
    private IEnumerator ExplosionAttackRoutine()
    {
        isChargingAttack = true;

        PlayChargeVfxClientRpc();

        yield return new WaitForSeconds(towerData.chargeTime);

        if (this == null || !IsSpawned) yield break;

        PlayExplosionVfxClientRpc();

        DealExplosionDamage();

        fireCountdown = 1f / towerData.fireRate;
        isChargingAttack = false;
    }
    
    private void DealExplosionDamage()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, towerData.explosionRadius);

        foreach (var hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(towerData.damage, towerData.damageType);
            }
        }
    }
    
    [ClientRpc]
    private void PlayChargeVfxClientRpc()
    {
        if (towerData.chargeVfxPrefab != null)
        {
            GameObject vfx = Instantiate(towerData.chargeVfxPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, towerData.chargeTime + 0.5f);
        }
    }

    [ClientRpc]
    private void PlayExplosionVfxClientRpc()
    {
        if (towerData.explosionVfxPrefab != null)
        {
            GameObject vfx = Instantiate(towerData.explosionVfxPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 2f);
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
        
        if (towerData.attackType == TowerDataSO.AttackType.AreaOfEffect)
        {
            Gizmos.color = new Color(1, 0.5f, 0, 0.5f);
            Gizmos.DrawSphere(transform.position, towerData.explosionRadius);
        }
    }
    private void SetupRangeCircle()
    {
        if (rangeLineRenderer == null || towerData == null) return;

        rangeLineRenderer.positionCount = circleSegments + 1;
        rangeLineRenderer.useWorldSpace = false;
        rangeLineRenderer.startWidth = lineWidth;
        rangeLineRenderer.endWidth = lineWidth;
        rangeLineRenderer.loop = true;
        
        if (rangeLineRenderer.material == null) 
            rangeLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        
        rangeLineRenderer.startColor = new Color(1f, 1f, 1f, 0.5f);
        rangeLineRenderer.endColor = new Color(1f, 1f, 1f, 0.5f);

        float angle = 0f;
        float radius = towerData.attackRadius;

        for (int i = 0; i <= circleSegments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            rangeLineRenderer.SetPosition(i, new Vector3(x, 0.1f, z));

            angle += (360f / circleSegments);
        }
    }
}
