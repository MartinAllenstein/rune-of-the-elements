using System;
using System.Collections.Generic;
using UnityEngine;

public class TheBase : MonoBehaviour
{
    public static List<TheBase> BaseList { get; private set; } = new List<TheBase>();
    public event EventHandler OnHealthChanged;
    public static event EventHandler OnBaseDestroyed;

    [SerializeField] private float maxHealth = 1000f;
    private float currentHealth;

    private void Awake()
    {
        BaseList.Add(this);
        currentHealth = maxHealth;
    }
    
    private void OnDestroy()
    {
        BaseList.Remove(this);
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        
        OnHealthChanged?.Invoke(this, EventArgs.Empty);
        
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            
            // Destroyed!!
            OnBaseDestroyed?.Invoke(this, EventArgs.Empty);
            Debug.Log("Base has been destroyed! GAME OVER.");
            
            // gameObject.SetActive(false); 
        }
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

    public float GetHealthNormalized()
    {
        return currentHealth / maxHealth;
    }
}