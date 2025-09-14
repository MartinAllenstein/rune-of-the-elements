using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class CauldronRecipeSO : ScriptableObject
{
    public List<KitchenObjectSO> kitchenObjectsSOList;
    public KitchenObjectSO  output;
    public string RecipeName;
}
