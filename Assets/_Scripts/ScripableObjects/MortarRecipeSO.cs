using UnityEngine;

[CreateAssetMenu()]
public class MortarRecipeSO : ScriptableObject
{
    public KitchenObjectSO input;
    public KitchenObjectSO output;
    public float grindingTimerMax;
}
