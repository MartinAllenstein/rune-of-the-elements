using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class DamageZone : NetworkBehaviour
{
    private float damage;
    private DamageTypeSO damageType;
    private float slowMultiplier;
    private float tickRate;
    
    private float timer;
    private List<Enemy> enemiesInside = new List<Enemy>();

    public void Initialize(float _damage, DamageTypeSO _damageType, float _slowMultiplier, float _duration, float _tickRate)
    {
        damage = _damage;
        damageType = _damageType;
        slowMultiplier = _slowMultiplier;
        tickRate = _tickRate;

        if (IsServer)
        {
            Destroy(gameObject, _duration);
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = tickRate;
            ApplyEffects();
        }
    }

    private void ApplyEffects()
    {
        for (int i = enemiesInside.Count - 1; i >= 0; i--)
        {
            Enemy enemy = enemiesInside[i];
            if (enemy == null)
            {
                enemiesInside.RemoveAt(i);
                continue;
            }

            enemy.TakeDamage(damage, damageType);
            
            enemy.ApplySlow(slowMultiplier, tickRate + 0.1f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            if (!enemiesInside.Contains(enemy))
            {
                enemiesInside.Add(enemy);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            if (enemiesInside.Contains(enemy))
            {
                enemiesInside.Remove(enemy);
            }
        }
    }
}