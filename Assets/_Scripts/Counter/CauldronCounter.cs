using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CauldronCounter : BaseCounter
{
    public enum State
    {
        Normal,
        Hot,
        Cold
    }

    public event EventHandler<OnIngredientAddedEventArgs> OnIngredientAdded;
    public class OnIngredientAddedEventArgs : EventArgs
    {
        public KitchenObjectSO kitchenObjectSO;
    }
    
    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
    public class OnStateChangedEventArgs : EventArgs
    {
        public State state;
    }

    private State currentState;
    private float stateChangeTimer;
    private const float STATE_CHANGE_INTERVAL = 10f;
    private List<KitchenObjectSO> kitchenObjectSOList = new List<KitchenObjectSO>();
    

    private void Start()
    {
        currentState = State.Normal;
        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = currentState });
    }

    private void Update()
    {
        // stateChangeTimer += Time.deltaTime;
        // if (stateChangeTimer >= STATE_CHANGE_INTERVAL)
        // {
        //     stateChangeTimer = 0;
        //     currentState = (State)(((int)currentState + 1) % 3);
        //     OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = currentState });
        // }
        
    }
    
    public override void InteractAlternate(Player player)
    {
        Cook();
    }
    
    public override void Interact(Player player)
    {
        // If there's already a cooked item on the counter
        if (HasKitchenObject())
        {
            // If the player has empty hands, they can pick it up
            if (!player.HasKitchenObject())
            {
                GetKitchenObject().SetKitchenObjectParent(player);
            }
            // Handle logic for adding the output to a plate
            else if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
            {
                if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                {
                    GetKitchenObject().DestroySelf();
                }
            }
        }
        // If the counter is empty, the player can add an ingredient
        else
        {
            if (player.HasKitchenObject())
            {
                var kitchenObjectSO = player.GetKitchenObject().GetKitchenObjectSO();

                switch (currentState)
                {
                    case State.Normal:
                        AddIngredient(player);
                        break;
                    case State.Hot:
                        if (kitchenObjectSO.ingredientType != IngredientType.Liquid && kitchenObjectSO.ingredientType != IngredientType.MagicLiquid)
                        {
                            AddIngredient(player);
                        }
                        else
                        {
                            Debug.Log("Cannot add this ingredient while the cauldron is hot!");
                        }
                        break;
                    case State.Cold:
                        if (kitchenObjectSO.ingredientType != IngredientType.Solid)
                        {
                            AddIngredient(player);
                        }
                        else
                        {
                            Debug.Log("Cannot add this ingredient while the cauldron is cold!");
                        }
                        break;
                }
            }
            // If player has no object and counter is empty, do nothing.
        }
    }

    private void AddIngredient(Player player)
    {
        var kitchenObjectSO = player.GetKitchenObject().GetKitchenObjectSO();
        kitchenObjectSOList.Add(kitchenObjectSO);
        
        OnIngredientAdded?.Invoke(this, new OnIngredientAddedEventArgs { kitchenObjectSO = kitchenObjectSO });
        
        player.GetKitchenObject().DestroySelf();
        Debug.Log("Added " + kitchenObjectSO.objectName);
    }

    private void Cook()
    {
        // Only cook if the cauldron is empty (no previous output sitting there)
        if (kitchenObjectSOList.Count > 0 && !HasKitchenObject())
        {
            if (CauldronManager.Instance.TryGetRecipe(kitchenObjectSOList, out var recipe))
            {
                Debug.Log("Recipe success! Creating " + recipe.RecipeName);
                KitchenObject.SpawnKitchenObject(recipe.output, this);
            }
            else
            {
                Debug.Log("Recipe failed!");
            }
            kitchenObjectSOList.Clear();
            
            OnIngredientAdded?.Invoke(this, new OnIngredientAddedEventArgs { kitchenObjectSO = null });
            
        }
    }
    public List<KitchenObjectSO> GetKitchenObjectSOList()
    {
        return kitchenObjectSOList;
    }
}