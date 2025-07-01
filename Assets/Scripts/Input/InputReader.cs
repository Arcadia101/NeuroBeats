using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "NeuroBeats/Input Reader")]
public class InputReader : ScriptableObject, InputSystem_Actions.IPlayerActions
{
    public event UnityAction<Vector2> Direction = delegate { };
    public event UnityAction LeftTrigger = delegate { };
    public event UnityAction LeftButton = delegate { };
    public event UnityAction RightTrigger = delegate { };
    public event UnityAction RightButton = delegate { };

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
        inputActions.Enable();
    }

    public void DisableGameplayInput()
    {
        inputActions.Disable();
    }

    // === Callbacks from Input System ===
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
}
