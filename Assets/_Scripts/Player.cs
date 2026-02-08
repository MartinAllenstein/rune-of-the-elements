using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class Player : NetworkBehaviour, IKitchenObjectParent
{
    public static event EventHandler OnAnyPlayerSpawned;
    public static event EventHandler OnAnyPickedSomething;

    public static void ResetStaticData()
    {
        OnAnyPlayerSpawned = null;
    }
    
    public static Player LocalInstance { get; private set; }
    
    public event EventHandler OnPickedSomething; 
    public event EventHandler <OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    // send data
    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }
    
    [SerializeField] private float moveSpeed;
    [SerializeField] private LayerMask countersLayerMask;
    [SerializeField] private LayerMask collisionsLayerMask;
    [SerializeField] private Transform kitchenObjectHoldPoint;
    //[SerializeField] private List<Vector3> spawnPositionList;
    [SerializeField] private PlayerVisual playerVisual;
    
    private bool isWalking;
    private Vector3 lastInteractDir;
    private BaseCounter selectedCounter;
    private KitchenObject kitchenObject;

    // Network Variable
    private NetworkVariable<float> playerVisualScaleX = new NetworkVariable<float>(1f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    [SerializeField] private Transform playerVisualTransform; 
    public Animator animator;

    private const string IS_WALKING = "IsWalking";
    
    // --- Network ---
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
        }

        int playerIndex = KitchenGameMultiplayer.Instance.GetPlayerDataIndexFromClientId(OwnerClientId);
        
        if (PlayerSpawnPoints.Instance != null) 
        {
            transform.position = PlayerSpawnPoints.Instance.GetSpawnPosition(playerIndex);
        }
        
        playerVisualScaleX.OnValueChanged += OnPlayerVisualScaleXChanged;

        if (playerVisualTransform != null)
        {
            playerVisualTransform.localScale = new Vector3(playerVisualScaleX.Value, 1, 1);
        }

        OnAnyPlayerSpawned?.Invoke(this, EventArgs.Empty);

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (clientId == OwnerClientId && HasKitchenObject())
        {
            KitchenObject.DestroyKitchenObject(GetKitchenObject());
        }
    }

    public override void OnNetworkDespawn()
    {
        playerVisualScaleX.OnValueChanged -= OnPlayerVisualScaleXChanged;
    }
    private void OnPlayerVisualScaleXChanged(float previousValue, float newValue)
    {
        if (playerVisualTransform != null)
        {
            playerVisualTransform.localScale = new Vector3(newValue, 1, 1);
        }
    }
    
    
    
    public void Awake()
    {

        if (playerVisualTransform == null)
        {
            playerVisualTransform = transform;
        }
    }

    private void Start()
    {
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        GameInput.Instance.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
        
        PlayerData playerData = KitchenGameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerVisual.SetPlayerVisual(KitchenGameMultiplayer.Instance.GetPlayerVisual(playerData.visualId));
    }
    
    private void GameInput_OnInteractAlternateAction(object sender, System.EventArgs e)
    {
        if (!GameManager.Instance.IsGamePlaying()) return; //Can not interact if Not playing
        
        if (selectedCounter != null)
        {
            selectedCounter.InteractAlternate(this);
        }
    }

    // sub to Interact event
    private void GameInput_OnInteractAction(object sender, System.EventArgs e)
    {
        if (!GameManager.Instance.IsGamePlaying()) return; //Can not interact if Not playing

        if (selectedCounter != null)
        {
            selectedCounter.Interact(this);
        }
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        
        HandleMovement();
        HandleInteractions();
    }

    public bool IsWalking()
    {
        return isWalking;
    }
    
    private void HandleInteractions()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();

        Vector3 moveDirection = new Vector3(inputVector.x, 0, inputVector.y);

        if (moveDirection != Vector3.zero)
        {
            lastInteractDir = moveDirection;
        }

        float interactDistance = 2f;
        if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycastHit, interactDistance, collisionsLayerMask, QueryTriggerInteraction.Ignore))
        {
            if (raycastHit.transform.TryGetComponent(out BaseCounter baseCounter))
            {
                // Has ClearCounter
                if (baseCounter != selectedCounter)
                {
                    SetSelectedCounter (baseCounter);
                }
            }
            else
            {
                SetSelectedCounter(null);
            }
        }
        else
        {
            SetSelectedCounter(null);
        }
    }

    private void HandleMovement()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();

        Vector3 moveDirection = new Vector3(inputVector.x, 0, inputVector.y);

        float moveDistance = moveSpeed * Time.deltaTime;
        float playerHeight = 2f;
        float playerRadius = 0.7f;
        bool canMove = !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirection, Quaternion.identity, moveDistance, collisionsLayerMask, QueryTriggerInteraction.Ignore);

        if (!canMove)
        {
            // Cannot move towards moveDir

            // Attempt only X movement
            Vector3 moveDirX = new Vector3(moveDirection.x, 0, 0).normalized;
            canMove = (moveDirection.x < -0.5f || moveDirection.x > +0.5f) && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirX, Quaternion.identity, moveDistance, collisionsLayerMask, QueryTriggerInteraction.Ignore);

            if (canMove)
            {
                // Can move only on the X
                moveDirection = moveDirX;
            }
            else
            {
                // Cannot move only on the X

                // Attempt only Z movement
                Vector3 moveDirZ = new Vector3(0, 0, moveDirection.z).normalized;
                canMove = (moveDirection.z < -0.5f || moveDirection.z > +0.5f) && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirZ, Quaternion.identity, moveDistance, countersLayerMask, QueryTriggerInteraction.Ignore);

                if (canMove)
                {
                    // Can move only on the Z
                    moveDirection = moveDirZ;
                }
                else
                {
                    // Cannot move   
                }
            }
        }

        if (canMove)
        {
            transform.position += moveDirection * moveDistance;
        }


        isWalking = moveDirection != Vector3.zero;

        float rotateSpeed = 10f;
        transform.forward += Vector3.Slerp(transform.forward, moveDirection, Time.deltaTime * rotateSpeed); // For the rotation

        
        // Sprite flip
        if (inputVector.x > 0)
        {
            playerVisualScaleX.Value = -1f;
            //playerVisual.localScale = new Vector3(-1, 1, 1);
        }
        else if (inputVector.x < 0)
        {
            playerVisualScaleX.Value = 1f;
            //playerVisual.localScale = new Vector3(1, 1, 1);
        }

        animator.SetBool(IS_WALKING, isWalking);
    }

    private void SetSelectedCounter(BaseCounter selectedCounter)
    {
        this.selectedCounter = selectedCounter;
        
        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs { selectedCounter = selectedCounter });
    }
    
    public BaseCounter GetSelectedCounter() {
        return selectedCounter;
    }

    public Transform GetKitchenObjectFollowTransform()
    {
        return kitchenObjectHoldPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        // Animation Holding
        this.kitchenObject = kitchenObject;

        if (kitchenObject != null)
        {
            OnPickedSomething?.Invoke(this, EventArgs.Empty);
            OnAnyPickedSomething?.Invoke(this, EventArgs.Empty);
        }
    }

    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }

    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }


    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}