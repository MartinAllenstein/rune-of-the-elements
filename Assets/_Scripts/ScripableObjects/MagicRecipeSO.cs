using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/MagicRecipeSO")]
public class MagicRecipeSO : ScriptableObject
{
    public List<KitchenObjectSO> input;
    public KitchenObjectSO output;
    public string magicName;
}
