using System;
using UnityEngine;

public class TheBase : MonoBehaviour
{
    public static TheBase Instance { get; private set; }

    public static event EventHandler OnBaseDestroyed;

    [SerializeField] private float maxHealth = 1000f;
    private float currentHealth;

    private void Awake()
    {
        Instance = this;
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log("Base took damage! Health is now: " + currentHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            
            // Destroyed!!
            OnBaseDestroyed?.Invoke(this, EventArgs.Empty);
            Debug.Log("Base has been destroyed! GAME OVER.");
            
            // gameObject.SetActive(false); 
        }
    }

    public float GetHealthNormalized()
    {
        return currentHealth / maxHealth;
    }
}