using System;
using UnityEngine;

public class CauldronIconsUI : MonoBehaviour
{
    [SerializeField] private CauldronCounter cauldronCounter;
    [SerializeField] private Transform iconTemplate;

    private void Awake()
    {
        iconTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        cauldronCounter.OnIngredientAdded += CauldronCounter_OnIngredientAdded;
    }

    private void CauldronCounter_OnIngredientAdded(object sender, CauldronCounter.OnIngredientAddedEventArgs e)
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        // Clear all old icon
        foreach (Transform child in transform)
        {
            if (child == iconTemplate) continue;
            Destroy(child.gameObject);
        }
        
        // Crater new icon
        foreach (KitchenObjectSO kitchenObjectSO in cauldronCounter.GetKitchenObjectSOList())
        {
            Transform iconTransform = Instantiate(iconTemplate, transform);
            iconTransform.gameObject.SetActive(true);
            iconTransform.GetComponent<PlateIconsSingleUI>().SetKitchenObjectSO(kitchenObjectSO);
        }
    }
}