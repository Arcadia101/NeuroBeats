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

    #region Action Maps Definition
    public enum ActionMapType //An enum with the action maps we have and how we work with them
    {
        UI,
        Player
    }

    private ActionMapType _currentMap;

    //----------------------------------------//
    //----------------------------------------//
    //----------------------------------------//-------------------------------------------------------------------------------------- //
    //          Initialization                  // -------------------------------------------------------------------------------------- //
    //----------------------------------------//-------------------------------------------------------------------------------------- //
    //----------------------------------------//
    //----------------------------------------//
    
    private void OnEnable()
    {
        if (inputActions == null)
        {
            inputActions = new InputSystem_Actions();
        }

        // Optionally activate a default map here
        //ChangeActionMap(ActionMapType.UI);
    }

    private void OnDisable()
    {
        DisableAllActionMaps();
    }

    //----------------------------------------//
    // Public Action Map Control                //
    //----------------------------------------// Here we set the action map
    public void ChangeActionMap(ActionMapType inputActionMap)
    {
        DisableAllActionMaps();

        _currentMap = inputActionMap;

        switch (inputActionMap)
        {
            case ActionMapType.UI:
                inputActions.UI.SetCallbacks(this);
                inputActions.UI.Enable();
                break;

            case ActionMapType.Player:
                // You'd enable _Actions.Gameplay here and set callbacks when needed
                inputActions.Player.SetCallbacks(this);
                inputActions.Player.Enable();
                break;
            default:
                break;
        }
    }

    private void DisableAllActionMaps()
    {
        inputActions.UI.Disable();
        inputActions.Player.Disable();
        // _Actions.Gameplay.Disable(); // Add when implemented
    }
    
    
    #endregion
    
    //----------- End of Action Maps definition -----------//

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

       //----------------------------------------//
    //----------------------------------------//
    //----------------------------------------//-------------------------------------------------------------------------------------- //
    //                  UI action map         // -------------------------------------------------------------------------------------- //
    //----------------------------------------//-------------------------------------------------------------------------------------- //
    //----------------------------------------//
    //----------------------------------------//
    #region UI Action Map
    
    
    //----------------------------------------//
    //    UI action map delegates         // ------------------------------ //
    //----------------------------------------// Here we define the delegates

    public event UnityAction<Vector2> UI_Point = delegate { };
    public event UnityAction<Vector2> UI_Navigate = delegate { };
    public event UnityAction UI_Submit = delegate { };
    public event UnityAction UI_Click = delegate { };
    public event UnityAction UI_RightClick = delegate { };
    public event UnityAction UI_ScrollWheel = delegate { };
    public event UnityAction UI_Continue = delegate { };
    public event UnityAction UI_Back = delegate { };
    public event UnityAction UI_Next = delegate { };
    public event UnityAction UI_Previous = delegate { };



    
    //----------------------------------------//
    //    UI Action Map Input Callbacks       // ------------------------------ //
    //----------------------------------------// Here we define the methods that will invoke our delegates
    //----------------------------------------// These are determined by the interface. Some of them will be empty methods for now.

    #region UIActionMap Events
    public void OnPoint(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            UI_Point.Invoke(context.ReadValue<Vector2>());
    }

    public void OnPointerPosition(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            UI_Point.Invoke(context.ReadValue<Vector2>());
    }

    public void OnPointerDelta(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            UI_Point.Invoke(context.ReadValue<Vector2>());
    }

    public void OnNavigate(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            UI_Navigate.Invoke(context.ReadValue<Vector2>());
    }

    public void OnSubmit(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            UI_Submit.Invoke();
    }

    public void OnCancel(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            UI_Back.Invoke();
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            UI_Click.Invoke();
    }

    public void OnRightClick(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            UI_RightClick.Invoke();
    }

    public void OnMiddleClick(InputAction.CallbackContext context)
    {
        // Left unimplemented for now
    }

    public void OnScrollWheel(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            UI_ScrollWheel.Invoke();
    }

    public void OnContinue(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            UI_Continue.Invoke();
    }

    public void OnBack(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            UI_Back.Invoke();
    }

    public void OnNext(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            UI_Next.Invoke();
    }

    public void OnPrevious(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            UI_Previous.Invoke();
    }
    
    #endregion
    //------------ End of UI action map events ------------// 
    #endregion
    //------------ End of UI action map ------------// 
    
}
