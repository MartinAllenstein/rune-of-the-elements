using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }
    
    public event EventHandler OnInteractAction;
    public event EventHandler OnInteractAlternateAction;
    public event EventHandler OnPauseAction;
    
    public event EventHandler OnInteractAlternateActionStarted;
    public event EventHandler OnInteractAlternateActionCanceled;
    
    
    private InputSystem_Actions playerInputActions;
    private void Awake()
    {
        Instance = this;
        
        playerInputActions = new InputSystem_Actions();
        playerInputActions.Player.Enable();

        playerInputActions.Player.Interact.performed += Interact_performed;
        playerInputActions.Player.InteractAlternate.performed += InteractAlternate_performed;
        playerInputActions.Player.Pause.performed += Pause_performed;
        
        playerInputActions.Player.InteractAlternate.started += InteractAlternate_started;
        playerInputActions.Player.InteractAlternate.canceled += InteractAlternate_canceled;
    }

    private void OnDestroy()
    {
        playerInputActions.Player.Interact.performed -= Interact_performed;
        playerInputActions.Player.InteractAlternate.performed -= InteractAlternate_performed;
        playerInputActions.Player.Pause.performed -= Pause_performed;
        
        playerInputActions.Player.InteractAlternate.started -= InteractAlternate_started;
        playerInputActions.Player.InteractAlternate.canceled -= InteractAlternate_canceled;
        
        playerInputActions.Dispose();
    }

    private void Pause_performed(InputAction.CallbackContext obj)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    private void InteractAlternate_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnInteractAlternateActionStarted?.Invoke(this, EventArgs.Empty);
    }
    
    private void InteractAlternate_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnInteractAlternateActionCanceled?.Invoke(this, EventArgs.Empty);
    }

    private void InteractAlternate_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnInteractAlternateAction?.Invoke(this, EventArgs.Empty);
    }
    
    private void Interact_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }
    public Vector2 GetMovementVectorNormalized()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        
        inputVector = inputVector.normalized; // Control speed in diagonal
        
        return inputVector;
    }
}
