using UnityEngine;

public enum IngredientType
{
    None,
    A,
    B,
    C,
    D
}

[CreateAssetMenu()]
public class KitchenObjectSO : ScriptableObject
{
    public Transform prefab;
    public Sprite sprite;
    public string objectName;
    public IngredientType ingredientType;
}



// [CreateAssetMenu()]
// public class KitchenObjectSO : ScriptableObject
// {
//     public Transform prefab;
//     public Sprite sprite;
//     public string objectName;
// }
