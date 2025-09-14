using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CauldronCounter : BaseCounter
{
    public enum State
    {
        Normal,
        Hot,
        Cold
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
    private float cookHoldTimer;
    private const float COOK_HOLD_DURATION = 2f;

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

        if (Keyboard.current.tKey.isPressed)
        {
            cookHoldTimer += Time.deltaTime;
            if (cookHoldTimer >= COOK_HOLD_DURATION)
            {
                cookHoldTimer = 0;
                Cook();
            }
        }
        else
        {
            cookHoldTimer = 0;
        }
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
                        if (kitchenObjectSO.ingredientType != IngredientType.A && kitchenObjectSO.ingredientType != IngredientType.B)
                        {
                            AddIngredient(player);
                        }
                        else
                        {
                            Debug.Log("Cannot add this ingredient while the cauldron is hot!");
                        }
                        break;
                    case State.Cold:
                        if (kitchenObjectSO.ingredientType != IngredientType.C && kitchenObjectSO.ingredientType != IngredientType.D)
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
        player.GetKitchenObject().DestroySelf();
        Debug.Log("Added " + kitchenObjectSO.objectName);
    }

    private void Cook()
    {
        // Only cook if the cauldron is empty (no previous output sitting there)
        if (!HasKitchenObject())
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
        }
    }
}











// public class CauldronCounter : BaseCounter
// {
//     // Events สำหรับการแสดงผล (ถ้าต้องการทำอนิเมชันหรือเสียงประกอบ)
//     public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
//     public class OnStateChangedEventArgs : EventArgs
//     {
//         public State currentState;
//     }
//
//     // Enum สำหรับจัดการสถานะของหม้อ
//     public enum State
//     {
//         Normal,
//         Hot,
//         Cold,
//     }
//
//     [Header("State Management")]
//     [SerializeField] private float stateChangeInterval = 10f; // เวลาในการเปลี่ยนสถานะ (วินาที)
//     
//     [Header("Ingredient Rules")]
//     [SerializeField] private KitchenObjectSO forbiddenIngredientHot; // วัตถุดิบชนิด A ที่ห้ามใส่ตอนร้อน
//     [SerializeField] private KitchenObjectSO forbiddenIngredientCold; // วัตถุดิบชนิด B ที่ห้ามใส่ตอนเย็น
//
//     [Header("Recipe")]
//     [SerializeField] private List<RecipeSO> validRecipes; // รายการสูตรอาหารที่ถูกต้องทั้งหมด
//
//     private State currentState;
//     private float stateChangeTimer;
//     private float recipeCheckHoldTimer;
//     private const float RECIPE_CHECK_HOLD_TIME = 3f; // เวลาที่ต้องกด T ค้างไว้ (วินาที)
//     
//     private List<KitchenObjectSO> ingredientsInCauldron;
//
//     private void Start()
//     {
//         // เริ่มต้นที่สถานะ Normal และสร้าง List ไว้เก็บวัตถุดิบ
//         ingredientsInCauldron = new List<KitchenObjectSO>();
//         ResetCauldron();
//         
//         currentState = State.Normal;
//     }
//
//     private void Update()
//     {
//         // จัดการการเปลี่ยนสถานะอัตโนมัติทุก 10 วินาที
//         //HandleStateSwitching();
//         
//         // จัดการการกดปุ่ม 'T' ค้างไว้เพื่อตรวจสอบสูตร
//         //HandleRecipeCheckInput();
//         if (Input.GetKeyDown(KeyCode.Space))
//         {
//             //CheckRecipe();
//         }
//     }
//
//     private void HandleStateSwitching()
//     {
//         stateChangeTimer += Time.deltaTime;
//         if (stateChangeTimer >= stateChangeInterval)
//         {
//             stateChangeTimer = 0f;
//
//             // วนสถานะ Normal -> Hot -> Cold -> Normal ...
//             switch (currentState)
//             {
//                 case State.Normal:
//                     currentState = State.Hot;
//                     break;
//                 case State.Hot:
//                     currentState = State.Cold;
//                     break;
//                 case State.Cold:
//                     currentState = State.Normal;
//                     break;
//             }
//             
//             // ส่ง Event บอกว่าสถานะเปลี่ยนไปแล้ว (สำหรับ Visual/Sound)
//             OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { currentState = this.currentState });
//             Debug.Log($"Cauldron state changed to: {currentState}");
//         }
//     }
//     
//     private void HandleRecipeCheckInput()
//     {
//         // ตรวจสอบการกดปุ่ม 'T' (สามารถเปลี่ยนไปใช้ GameInput.cs ได้หากต้องการ)
//         if (Input.GetKey(KeyCode.T))
//         {
//             recipeCheckHoldTimer += Time.deltaTime;
//             if (recipeCheckHoldTimer >= RECIPE_CHECK_HOLD_TIME)
//             {
//                 // เมื่อกดค้างครบ 3 วิ ให้ตรวจสอบสูตรและรีเซ็ต timer
//                 recipeCheckHoldTimer = 0f;
//                 //CheckRecipe();
//             }
//         }
//         else
//         {
//             // ถ้าปล่อยปุ่ม 'T' ให้รีเซ็ต timer
//             recipeCheckHoldTimer = 0f;
//         }
//     }
//
//     public override void Interact(Player player)
//     {
//         // ถ้าผู้เล่นไม่มีของในมือ ก็ไม่ต้องทำอะไร
//         if (player.HasKitchenObject())
//         {
//             KitchenObjectSO playerIngredient = player.GetKitchenObject().GetKitchenObjectSO();
//
//             switch (currentState)
//             {
//                 case State.Normal:
//                     // สถานะปกติ: รับวัตถุดิบได้ทุกชนิด
//                     AddIngredient(player, playerIngredient);
//                     break;
//
//                 case State.Hot:
//                     // สถานะร้อน: รับวัตถุดิบชนิด A ไม่ได้
//                     if (playerIngredient == forbiddenIngredientHot)
//                     {
//                         // ถ้าเป็นวัตถุดิบต้องห้าม ให้ทำลายทิ้ง
//                         Debug.Log($"Cannot add {playerIngredient.objectName} when the cauldron is Hot!");
//                         player.GetKitchenObject().DestroySelf();
//                     }
//                     else
//                     {
//                         AddIngredient(player, playerIngredient);
//                     }
//                     break;
//
//                 case State.Cold:
//                     // สถานะเย็น: รับวัตถุดิบชนิด B ไม่ได้
//                     if (playerIngredient == forbiddenIngredientCold)
//                     {
//                         // ถ้าเป็นวัตถุดิบต้องห้าม ให้ทำลายทิ้ง
//                         Debug.Log($"Cannot add {playerIngredient.objectName} when the cauldron is Cold!");
//                         player.GetKitchenObject().DestroySelf();
//                     }
//                     else
//                     {
//                         AddIngredient(player, playerIngredient);
//                     }
//                     break;
//             }
//         }
//
//         
//     }
//
//     private void AddIngredient(Player player, KitchenObjectSO ingredient)
//     {
//         Debug.Log($"Added {ingredient.objectName} to the cauldron.");
//         ingredientsInCauldron.Add(ingredient);
//         
//         // ทำลายวัตถุในมือผู้เล่นหลังจากใส่ลงหม้อแล้ว
//         player.GetKitchenObject().DestroySelf();
//     }
//
//     private void CheckRecipe()
//     {
//         Debug.Log("Checking recipe...");
//         
//         bool matchFound = false;
//         foreach (RecipeSO recipe in validRecipes)
//         {
//             // 1. ตรวจสอบว่าจำนวนวัตถุดิบตรงกันหรือไม่
//             if (recipe.kitchenObjectsSOList.Count != ingredientsInCauldron.Count)
//             {
//                 continue; // ถ้าจำนวนไม่ตรง ให้ข้ามไปเช็คสูตรถัดไป
//             }
//
//             // 2. ตรวจสอบว่าวัตถุดิบทุกชิ้นตรงกันหรือไม่ (ไม่สนลำดับ)
//             bool allIngredientsMatch = true;
//             List<KitchenObjectSO> cauldronIngredientsCopy = new List<KitchenObjectSO>(ingredientsInCauldron);
//
//             foreach (KitchenObjectSO recipeIngredient in recipe.kitchenObjectsSOList)
//             {
//                 if (cauldronIngredientsCopy.Contains(recipeIngredient))
//                 {
//                     cauldronIngredientsCopy.Remove(recipeIngredient);
//                 }
//                 else
//                 {
//                     // มีวัตถุดิบในสูตรที่ไม่พบในหม้อ
//                     allIngredientsMatch = false;
//                     break;
//                 }
//             }
//
//             if (allIngredientsMatch)
//             {
//                 matchFound = true;
//                 break; // เจอสูตรที่ตรงแล้ว ออกจาก loop
//             }
//         }
//
//         if (matchFound)
//         {
//             Debug.Log("<color=green>SUCCESS! The ingredients match a recipe!</color>");
//         }
//         else
//         {
//             Debug.Log("<color=red>FAILED! The ingredients do not match any recipe.</color>");
//         }
//         
//         // กลับสู่สถานะเริ่มต้น
//         //ResetCauldron();
//     }
//
//     private void ResetCauldron()
//     {
//         Debug.Log("Cauldron has been reset.");
//         
//         // ล้างวัตถุดิบทั้งหมด
//         ingredientsInCauldron.Clear();
//         
//         // กลับไปสถานะ Normal และรีเซ็ต timer
//         currentState = State.Normal;
//         stateChangeTimer = 0f;
//         
//         // ส่ง Event บอกว่าสถานะเปลี่ยนไปแล้ว (สำหรับ Visual/Sound)
//         OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { currentState = this.currentState });
//     }
// }
    
    
    // private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO)
    // {
    //     CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
    //     return cuttingRecipeSO != null;
    // }
    
    
    
    
    
    // private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO)
    // {
    //     CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
    //     if (cuttingRecipeSO != null)
    //     {
    //         return cuttingRecipeSO.output;
    //     }
    //     else
    //     {
    //         return null;
    //     }
    // }
    
    // private CuttingRecipeSO GetCuttingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    // {
    //     foreach (CuttingRecipeSO cuttingRecipeSO  in cuttingRecipeSOArray)
    //     {
    //         if (cuttingRecipeSO.input == inputKitchenObjectSO)
    //         {
    //             return cuttingRecipeSO;
    //         }
    //     }
    //     return null;
    // }


