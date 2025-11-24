using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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
    
    private NetworkVariable<State> currentState = new NetworkVariable<State>(State.Normal);
    
    private List<KitchenObjectSO> kitchenObjectSOList = new List<KitchenObjectSO>();
    
    //private float stateChangeTimer;
    //private const float STATE_CHANGE_INTERVAL = 10f;
    

    // private void Start()
    // {
    //     currentState = State.Normal;
    //     OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = currentState });
    // }
    
    public override void OnNetworkSpawn()
    {
        currentState.OnValueChanged += CurrentState_OnValueChanged;
        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = currentState.Value });
    }

    public override void OnNetworkDespawn()
    {
        currentState.OnValueChanged -= CurrentState_OnValueChanged;
    }

    private void CurrentState_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = newValue });
    }

    private void Update()
    {
        /*
        if (!IsServer) return;

        stateChangeTimer += Time.deltaTime;
        if (stateChangeTimer >= STATE_CHANGE_INTERVAL)
        {
            stateChangeTimer = 0;
            // เปลี่ยน State และซิงค์ผ่าน NetworkVariable
            currentState.Value = (State)(((int)currentState.Value + 1) % 3);
        }
        */
        
    }
    
    public override void InteractAlternate(Player player)
    {
        if (kitchenObjectSOList.Count > 0 && !HasKitchenObject())
        {
            CookServerRpc();
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void CookServerRpc()
    {
        if (CauldronManager.Instance.TryGetRecipe(kitchenObjectSOList, out var recipe))
        {
            KitchenObject.SpawnKitchenObject(recipe.output, this);
            
            // Note (For ClientRpc Effect)
            // CookSuccessClientRpc(); 
        }
        else
        {
            // CookFailedClientRpc();
        }
        ClearIngredientsClientRpc();
    }
    
    [ClientRpc]
    private void ClearIngredientsClientRpc()
    {
        kitchenObjectSOList.Clear();
        OnIngredientAdded?.Invoke(this, new OnIngredientAddedEventArgs { kitchenObjectSO = null });
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
                    KitchenObject.DestroyKitchenObject(GetKitchenObject());
                }
            }
        }
        // If the counter is empty, the player can add an ingredient
        else
        {
            if (player.HasKitchenObject())
            {
                var kitchenObjectSO = player.GetKitchenObject().GetKitchenObjectSO();

                switch (currentState.Value)
                {
                    case State.Normal:
                        ValidateAndAddIngredient(player, kitchenObjectSO);
                        break;
                    case State.Hot:
                        if (kitchenObjectSO.ingredientType != IngredientType.Liquid && kitchenObjectSO.ingredientType != IngredientType.MagicLiquid)
                        {
                            ValidateAndAddIngredient(player, kitchenObjectSO);
                        }
                        else
                        {
                            Debug.Log("Cannot add this ingredient while the cauldron is hot!");
                        }
                        break;
                    case State.Cold:
                        if (kitchenObjectSO.ingredientType != IngredientType.Solid)
                        {
                            ValidateAndAddIngredient(player, kitchenObjectSO);
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

    private void ValidateAndAddIngredient(Player player, KitchenObjectSO kitchenObjectSO)
    {
        int kitchenObjectSOIndex = KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(kitchenObjectSO);
        AddIngredientServerRpc(kitchenObjectSOIndex);
        
        KitchenObject.DestroyKitchenObject(player.GetKitchenObject());
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddIngredientServerRpc(int kitchenObjectSOIndex)
    {
        AddIngredientClientRpc(kitchenObjectSOIndex);
    }

    [ClientRpc]
    private void AddIngredientClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        
        kitchenObjectSOList.Add(kitchenObjectSO);
        
        OnIngredientAdded?.Invoke(this, new OnIngredientAddedEventArgs { kitchenObjectSO = kitchenObjectSO });
        
        // Debug.Log("Added " + kitchenObjectSO.objectName);
    }

    public List<KitchenObjectSO> GetKitchenObjectSOList()
    {
        return kitchenObjectSOList;
    }
}