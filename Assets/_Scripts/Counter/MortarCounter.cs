using System;
using Unity.Netcode;
using UnityEngine;

public class MortarCounter : BaseCounter, IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler OnGrind; // สำหรับเล่นเสียง/Animation

    [SerializeField] private MortarRecipeSO[] mortarRecipeSOArray;

    // เปลี่ยนตัวแปรธรรมดาเป็น NetworkVariable เพื่อซิงค์ข้อมูล
    private NetworkVariable<float> grindingTimer = new NetworkVariable<float>(0f);
    private NetworkVariable<bool> isGrinding = new NetworkVariable<bool>(false);

    private void Start()
    {
        GameInput.Instance.OnInteractAlternateActionStarted += GameInput_OnInteractAlternateActionStarted;
        GameInput.Instance.OnInteractAlternateActionCanceled += GameInput_OnInteractAlternateActionCanceled;
    }

    public override void OnNetworkSpawn()
    {
        // เมื่อค่า Timer เปลี่ยน ให้ Client อัปเดต UI
        grindingTimer.OnValueChanged += GrindingTimer_OnValueChanged;
    }

    public override void OnNetworkDespawn()
    {
        grindingTimer.OnValueChanged -= GrindingTimer_OnValueChanged;
    }

    private void GrindingTimer_OnValueChanged(float previousValue, float newValue)
    {
        // คำนวณ Progress Bar ฝั่ง Client
        float grindingTimerMax = 1f;
        if (HasKitchenObject())
        {
            MortarRecipeSO recipe = GetMortarRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
            if (recipe != null)
            {
                grindingTimerMax = recipe.grindingTimerMax;
            }
        }

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = newValue / grindingTimerMax
        });
    }

    private void Update()
    {
        // 1. Client Logic: เล่นเสียง/Animation ถ้ากำลังบดอยู่
        if (isGrinding.Value)
        {
            OnGrind?.Invoke(this, EventArgs.Empty);
        }

        // 2. Server Logic: คำนวณเวลาและการเปลี่ยนร่างวัตถุดิบ
        if (!IsServer) return;

        if (isGrinding.Value && HasKitchenObject())
        {
            MortarRecipeSO recipe = GetMortarRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
            
            if (recipe != null)
            {
                grindingTimer.Value += Time.deltaTime;

                if (grindingTimer.Value >= recipe.grindingTimerMax)
                {
                    // บดเสร็จแล้ว -> เปลี่ยนวัตถุ
                    KitchenObjectSO outputKitchenObjectSO = recipe.output;
                    
                    KitchenObject.DestroyKitchenObject(GetKitchenObject());
                    KitchenObject.SpawnKitchenObject(outputKitchenObjectSO, this);
                    
                    // รีเซ็ตค่า
                    grindingTimer.Value = 0f;
                    isGrinding.Value = false;
                }
            }
            else
            {
                // ถ้าของบนโต๊ะบดไม่ได้ ให้หยุดบด
                isGrinding.Value = false;
                grindingTimer.Value = 0f;
            }
        }
    }

    private void GameInput_OnInteractAlternateActionStarted(object sender, EventArgs e)
    {
        // ตรวจสอบว่าเป็นผู้เล่นคนนี้จริงๆ ที่กด และกำลังเลือก Counter นี้อยู่
        if (Player.LocalInstance != null && Player.LocalInstance.GetSelectedCounter() == this)
        {
            SetIsGrindingServerRpc(true);
        }
    }

    private void GameInput_OnInteractAlternateActionCanceled(object sender, EventArgs e)
    {
        SetIsGrindingServerRpc(false);
    }

    // ส่งคำสั่งไป Server ว่าเริ่ม/หยุดบด
    [ServerRpc(RequireOwnership = false)]
    private void SetIsGrindingServerRpc(bool isGrinding)
    {
        if (isGrinding)
        {
            // ตรวจสอบเงื่อนไขก่อนเริ่มบด (กันโปร)
            if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO()))
            {
                this.isGrinding.Value = true;
            }
        }
        else
        {
            this.isGrinding.Value = false;
        }
    }

    // การ Interact ปกติ (หยิบ/วาง)
    public override void Interact(Player player)
    {
        // ส่งคำสั่ง Interact ไปที่ Server โดยระบุ ID ผู้เล่น
        InteractServerRpc(player.OwnerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractServerRpc(ulong clientId)
    {
        // Server หาตัว Player จาก ID
        Player player = KitchenGameMultiplayer.Instance.GetPlayerFromClientId(clientId);
        
        if (player == null) return;

        if (!HasKitchenObject())
        {
            // Counter ว่าง
            if (player.HasKitchenObject())
            {
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO()))
                {
                    // วางของลงบน Counter
                    KitchenObject kitchenObject = player.GetKitchenObject();
                    kitchenObject.SetKitchenObjectParent(this);
                    
                    // รีเซ็ตค่าการบดเมื่อวางของใหม่
                    grindingTimer.Value = 0f;
                }
            }
        }
        else
        {
            // Counter มีของ
            if (player.HasKitchenObject())
            {
                // ผู้เล่นถือของ -> เช็คว่าเป็นจานไหม
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                    }
                }
            }
            else
            {
                // ผู้เล่นมือเปล่า -> หยิบของ
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }
    
    // --- Helper Functions (Logic เดิม) ---
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
    
    private void OnDestroy()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnInteractAlternateActionStarted -= GameInput_OnInteractAlternateActionStarted;
            GameInput.Instance.OnInteractAlternateActionCanceled -= GameInput_OnInteractAlternateActionCanceled;
        }
    }
}