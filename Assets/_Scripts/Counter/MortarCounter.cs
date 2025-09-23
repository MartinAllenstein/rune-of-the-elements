using System;
using UnityEngine;

public class MortarCounter : BaseCounter, IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler OnGrind; // for Music/Animation

    [SerializeField] private MortarRecipeSO[] mortarRecipeSOArray;

    private float grindingTimer;
    private bool isGrinding;
    private MortarRecipeSO currentRecipe;

    private void Start()
    {
        GameInput gameInput = FindAnyObjectByType<GameInput>();
        gameInput.OnInteractAlternateActionStarted += GameInput_OnInteractAlternateActionStarted;
        gameInput.OnInteractAlternateActionCanceled += GameInput_OnInteractAlternateActionCanceled;
    }

    private void OnDestroy()
    {
        GameInput gameInput = FindAnyObjectByType<GameInput>();
        if (gameInput != null)
        {
            gameInput.OnInteractAlternateActionStarted -= GameInput_OnInteractAlternateActionStarted;
            gameInput.OnInteractAlternateActionCanceled -= GameInput_OnInteractAlternateActionCanceled;
        }
    }

    private void GameInput_OnInteractAlternateActionStarted(object sender, EventArgs e)
    {
        // Start Grinding
        if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO()))
        {
            isGrinding = true;
            currentRecipe = GetMortarRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
        }
    }

    private void GameInput_OnInteractAlternateActionCanceled(object sender, EventArgs e)
    {
        // Stop Grinding
        isGrinding = false;
    }
    
    private void Update()
    {
        if (isGrinding)
        {
            grindingTimer += Time.deltaTime;

            OnGrind?.Invoke(this, EventArgs.Empty);
            
            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs 
            { 
                progressNormalized = grindingTimer / currentRecipe.grindingTimerMax 
            });

            if (grindingTimer >= currentRecipe.grindingTimerMax)
            {
                KitchenObjectSO outputKitchenObjectSO = GetOutputForInput(GetKitchenObject().GetKitchenObjectSO());
                GetKitchenObject().DestroySelf();
                KitchenObject.SpawnKitchenObject(outputKitchenObjectSO, this);
                
                // Reset
                isGrinding = false;
                grindingTimer = 0f;
                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs { progressNormalized = 0f });
            }
        }
    }

    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {
            if (player.HasKitchenObject())
            {
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO()))
                {
                    player.GetKitchenObject().SetKitchenObjectParent(this);
                    grindingTimer = 0f;
                    
                    MortarRecipeSO mortarRecipeSO = GetMortarRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs 
                    { 
                        progressNormalized = grindingTimer / mortarRecipeSO.grindingTimerMax
                    });
                }
            }
        }
        else
        {
            if (!player.HasKitchenObject())
            {
                GetKitchenObject().SetKitchenObjectParent(player);
                grindingTimer = 0f;
                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs { progressNormalized = 0f });
            }
            else if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
            {
                if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                {
                    GetKitchenObject().DestroySelf();
                }
            }
        }
    }
    

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        return GetMortarRecipeSOWithInput(inputKitchenObjectSO) != null;
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO)
    {
        MortarRecipeSO mortarRecipeSO = GetMortarRecipeSOWithInput(inputKitchenObjectSO);
        return mortarRecipeSO != null ? mortarRecipeSO.output : null;
    }

    private MortarRecipeSO GetMortarRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (MortarRecipeSO mortarRecipeSO in mortarRecipeSOArray)
        {
            if (mortarRecipeSO.input == inputKitchenObjectSO)
            {
                return mortarRecipeSO;
            }
        }
        return null;
    }
}