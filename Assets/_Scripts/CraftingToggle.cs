using UnityEngine;

public class CraftingToggle : MonoBehaviour
{
    [Header("UI to Toggle")]
    [SerializeField] private GameObject craftingCanvas;

    private void Awake()
    {
        if (craftingCanvas == null)
        {
            Debug.LogError("Crafting Canvas is not assigned in the CraftingToggle script!", this);
            enabled = false; 
            return;
        }

        craftingCanvas.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            craftingCanvas.SetActive(!craftingCanvas.activeSelf);
        }
    }
}