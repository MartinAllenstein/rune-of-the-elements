using UnityEngine;

[CreateAssetMenu(fileName = "PlayerVisualSO", menuName = "ScriptableObjects/PlayerVisualSO")]
public class PlayerVisualSO : ScriptableObject
{
    public RuntimeAnimatorController animatorController;
    public Sprite sprite;
}