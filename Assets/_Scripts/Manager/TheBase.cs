using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TheBase : NetworkBehaviour
{
    public static List<TheBase> BaseList { get; private set; } = new List<TheBase>();
    public event EventHandler OnHealthChanged;
    public static event EventHandler OnBaseDestroyed;

    [SerializeField] private float maxHealth = 1000f;
    
    private NetworkVariable<float> currentHealth = new NetworkVariable<float>(1000f);

    private void Awake()
    {
        BaseList.Add(this);
    }
    
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }

        currentHealth.OnValueChanged += OnCurrentHealthChanged;
    }
    
    public override void OnNetworkDespawn()
    {
        currentHealth.OnValueChanged -= OnCurrentHealthChanged;
    }
    
    private void OnDestroy()
    {
        BaseList.Remove(this);
        base.OnDestroy();
    }

    private void OnCurrentHealthChanged(float previousValue, float newValue)
    {
        OnHealthChanged?.Invoke(this, EventArgs.Empty);
    }
    
    public void TakeDamage(float damageAmount)
    {
        if (!IsServer) return;
        
        currentHealth.Value -= damageAmount;
        
        if (currentHealth.Value <= 0)
        {
            currentHealth.Value = 0;
            
            // Destroyed!!
            OnBaseDestroyed?.Invoke(this, EventArgs.Empty);
            Debug.Log("Base has been destroyed! GAME OVER.");
            
        }
    }
    
    public float GetHealthNormalized()
    {
        return currentHealth.Value / maxHealth;
    }
    
    public static TheBase GetNearestBase(Vector3 position)
    {
        TheBase nearestBase = null;
        float minDistance = float.MaxValue;

        foreach (TheBase baseObj in BaseList)
        {
            if (baseObj == null) continue;
            
            float distance = Vector3.Distance(position, baseObj.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestBase = baseObj;
            }
        }
        return nearestBase;
    }

    
}