using UnityEngine;

public class CraftingToggle : MonoBehaviour
{
    [Header("UI to Toggle")]
    [SerializeField] private GameObject craftingCanvas;
    [SerializeField] private GameObject ingredientsCanvas;
    [SerializeField] private GameObject mixTowerCanvas;

    private void Awake()
    {
        if (craftingCanvas == null)
        {
            Debug.LogError("Crafting Canvas is not assigned in the CraftingToggle script!", this);
            enabled = false; 
            return;
        }
        if (ingredientsCanvas == null)
        {
            Debug.LogError("Ingredients Canvas is not assigned in the CraftingToggle script!", this);
            enabled = false; 
            return;
        }
        
        if (mixTowerCanvas == null)
        {
            Debug.LogError("Mix Tower Canvas is not assigned in the CraftingToggle script!", this);
            enabled = false; 
            return;
        }

        craftingCanvas.SetActive(false);
        ingredientsCanvas.SetActive(false);
        mixTowerCanvas.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            craftingCanvas.SetActive(!craftingCanvas.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            ingredientsCanvas.SetActive(!ingredientsCanvas.activeSelf);
        }
        
        if (Input.GetKeyDown(KeyCode.P))
        {
            mixTowerCanvas.SetActive(!mixTowerCanvas.activeSelf);
        }
    }
}