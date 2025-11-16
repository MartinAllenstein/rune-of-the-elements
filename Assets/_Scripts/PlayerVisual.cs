using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisual : MonoBehaviour {


    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    

    public void SetPlayerVisual(PlayerVisualSO playerVisualSO) 
    {
        animator.runtimeAnimatorController = playerVisualSO.animatorController;
        spriteRenderer.sprite = playerVisualSO.sprite;
    }

}