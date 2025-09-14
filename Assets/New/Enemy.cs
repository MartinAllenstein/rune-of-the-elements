using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;
    [SerializeField] private float speed;
    [SerializeField] private int ID;
    
    public void Init()
    {
        currentHealth = maxHealth;
    }
}
