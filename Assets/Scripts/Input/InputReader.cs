using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "NeuroBeats/Input Reader")]
public class InputReader : ScriptableObject, InputSystem_Actions.IPlayerActions, InputSystem_Actions.IUIActions
{
    public event UnityAction<Vector2> Direction = delegate { };
    public event UnityAction LeftTrigger = delegate { };
    public event UnityAction LeftButton = delegate { };
    public event UnityAction RightTrigger = delegate { };
    public event UnityAction RightButton = delegate { };
    public event UnityAction MenuButton = delegate { };

    private InputSystem_Actions inputActions;

    public Vector2 LookDirection => inputActions.Player.Direction.ReadValue<Vector2>();

    private void OnEnable()
    {
        if (inputActions == null)
        {
            inputActions = new InputSystem_Actions();
            inputActions.Player.SetCallbacks(this);
        }
    }

    public void EnableGameplayInput()
    {
        inputActions.Player.Enable();
        inputActions.UI.Disable();
    }

    public void DisableGameplayInput()
    {
        inputActions.Player.Disable();
        inputActions.UI.Enable();
    }

    // === Callbacks from Input System ===
    
    #region Player Callbacks
    public void OnDirection(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed || context.phase == InputActionPhase.Canceled)
            Direction.Invoke(context.ReadValue<Vector2>());
    }

    public void OnLeftTrigger(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
            LeftTrigger.Invoke();
    }

    public void OnLeftButton(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
            LeftButton.Invoke();
    }

    public void OnRightTrigger(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
            RightTrigger.Invoke();
    }

    public void OnRightButton(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
            RightButton.Invoke();
    }
    
    public void OnMenuButton(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
            MenuButton.Invoke();
    }
    
    #endregion

    #region UI Callbacks
    
    public void OnNavigate(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnSubmit(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnCancel(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnPoint(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnRightClick(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnMiddleClick(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnScrollWheel(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnTrackedDevicePosition(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnTrackedDeviceOrientation(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }
    
    #endregion
    
}
