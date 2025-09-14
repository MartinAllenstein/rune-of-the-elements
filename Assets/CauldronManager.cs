using System;
using System.Collections.Generic;
using UnityEngine;

public class CauldronManager : MonoBehaviour
{
    public static CauldronManager Instance { get; private set; }

    [SerializeField] private List<CauldronRecipeSO> cauldronRecipeSOList;

    private void Awake()
    {
        Instance = this;
    }

    public bool TryGetRecipe(List<KitchenObjectSO> kitchenObjectSOList, out CauldronRecipeSO recipe)
    {
        foreach (var cauldronRecipe in cauldronRecipeSOList)
        {
            if (cauldronRecipe.kitchenObjectsSOList.Count != kitchenObjectSOList.Count)
            {
                continue;
            }

            bool isMatch = true;
            foreach (var recipeIngredient in cauldronRecipe.kitchenObjectsSOList)
            {
                if (!kitchenObjectSOList.Contains(recipeIngredient))
                {
                    isMatch = false;
                    break;
                }
            }

            if (isMatch)
            {
                recipe = cauldronRecipe;
                return true;
            }
        }

        recipe = null;
        return false;
    }
}
