using UnityEngine;

public enum IngredientType
{
    None,
    Solid,
    Fine,
    Powder,
    Liquid,
    MagicLiquid
}

[CreateAssetMenu(menuName = "Scriptable Objects/KitchenObjectSO")]
public class KitchenObjectSO : ScriptableObject
{
    public Transform prefab;
    public Sprite sprite;
    public string objectName;
    public IngredientType ingredientType;
}
