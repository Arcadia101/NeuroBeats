using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI; //For Button class


///----------------------------------------//
    /// <summary>
    /*
    This is the UI equivalent of "Player Controller" 
    It technically is a player controller because translates player inputs into game events.
    but it applies only to the context of UI
    more specifically, the Main Menu
    */ 
    /// <summary>
///----------------------------------------//
    ///
    /// 
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private InputReader inputReader; // Reference to the input reader instance (we should assign this through the inspector)

    //----------------------------------------//
    //    Delegates definition         // ------------------------------ //
    //----------------------------------------//

    public static MainMenuController Instance;

    public event UnityAction leftClick = delegate { }; //This is mostly for the Gamepad
    public event UnityAction rightClick = delegate { };
    public event UnityAction backButton = delegate { };
    public event UnityAction<Vector2> point = delegate { };
    private Vector2 screenClampers;
    
    public Vector2 cursorPosition;
    
    
    
    private PlayerInput _playerInput;
    public bool isGamepadActive { get; private set; } = false;
    public bool isMouseActive { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        _playerInput = GetComponent<PlayerInput>();
        _playerInput.onControlsChanged += OnControlsChanged;
        pointerRectTransform = pointer.GetComponent<RectTransform>();
        screenClampers = new Vector2(
            Mathf.Round(Screen.width / 2f * 10f) / 10f,
            Mathf.Round(Screen.height / 2f * 10f) / 10f
        );
    }

    private void Update()
    {
        //HandlePointer();
    }


    //----------------------------------------//
    //    Methods definition         // ------------------------------ //
    //----------------------------------------//
    private void OnControlsChanged(PlayerInput input)
    {
        var scheme = input.currentControlScheme;
        switch (scheme)
            {
                case "Gamepad":
                    isGamepadActive = scheme == "Gamepad";
                   // SystemCursor(false);
                    GamePadBehavior();
                    break;
                case "Keyboard&Mouse":
                    isMouseActive = scheme == "Keyboard&Mouse"; 
                    SystemCursor(true);
                    //MouseKeyboardBehavior();
                    break;
                default:
                    break;
            }
        Debug.Log($"Control Scheme Changed to: {scheme}");
    }

    private Canvas canvas;
    private RectTransform screenSpace;
    private void OnEnable()
    {
        canvas = transform.parent.GetComponent<Canvas>();
        screenSpace = canvas.GetComponent<RectTransform>();
        inputReader.ChangeActionMap(InputReader.ActionMapType.UI);
        inputReader.UI_Submit += HandleSubmit;
        inputReader.UI_Navigate += HandleNavigate;
        inputReader.UI_Point += HandlePointer;
        inputReader.UI_Back += OnBackButton;
    }

    private void OnDisable()
    {
        inputReader.UI_Submit -= HandleSubmit;
        inputReader.UI_Navigate -= HandleNavigate;
        inputReader.UI_Point -= HandlePointer;
        inputReader.UI_Back -= OnBackButton;
    }
    
    private void HandleSubmit()
    {
        
    }

    [SerializeField] private float cursorOffset;
    private void HandleNavigate(Vector2 direction) //This method doesn't depend on the scheme, but on the way you select buttons
    {
        Debug.Log("We are navigating");
        if (!pointer.activeInHierarchy)
        {
            pointer.SetActive(true);
        }

        if (pointer.activeInHierarchy)
        {

            GameObject current = EventSystem.current.currentSelectedGameObject;
            if (current == null)
            {
                // Set default selection
                EventSystem.current.SetSelectedGameObject(defaultButton.gameObject);
            }
            Vector2 newPosition = new Vector2(current.transform.position.x + cursorOffset,  current.transform.position.y);
            if(MouseOverButtonGroup()){
                SystemCursor(false);
                Mouse.current.WarpCursorPosition(newPosition); //We place the actual mouse cursor somwhere where it doesn't conflict with the buttons
                //Vector2 newMousePosition = new Vector2(defaultButton.transform.position.x + cursorOffset,  defaultButton.transform.position.y);
            }
            pointer.GetComponent<FakePointer>().AdjustPointerPosition(newPosition);
            //We deactivate the mouse cursor

        }
    }

    
    private GameObject MouseOverUIButton()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = mousePosition;
        
        List<RaycastResult> raycastResultList = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResultList);
        for (int i = 0; i < raycastResultList.Count; i++)
        {
            if (raycastResultList[i].gameObject.GetComponent<Button>() != null)
            {
                return raycastResultList[i].gameObject;
            }
        } 
        return null;
    }



    //----------------------------------------//
    //    Pointer related methods               // ------------------------------ //
    //----------------------------------------//
    
    public float pointerSpeed = 1f;
    [SerializeField] public GameObject pointer;
    public Vector2 pointerPosition = default;
    [SerializeField] public RectTransform pointerRectTransform;
    /// <summary>
    /// Here we handle the pointer once the inputs have been processed by OnPointer
    /// </summary>
    private Vector2 mousePosition;
    //Here we handle the pointer inputs
    private void HandlePointer(Vector2 input)
    {
        mousePosition = input;
        //Debug.Log("We are moving through mouse position");
        if (isGamepadActive) //If the gamepad is active, what we change is the movement direction
        {
            // Delta movement (Vector2: -1, 0, or 1)
            //pointerDirection = input;
        }
        if(isMouseActive) //If the mouse and keyboard is active, what we change is the absolute position
        {
            GameObject currentItem = MouseOverUIButton(); //We first check if we are on top of a button
            if (currentItem != null)
            {
                if (MouseOverButtonGroup())
                {
                    pointer.SetActive(false);
                }
                
                /*
                 * 
                if (pointer.activeInHierarchy) //If the fake pointer  is enabled, let's disable it
                {
                    Debug.Log("Pointer Active");
                }
                 */
                //And now let's assign whatever we are on top of
                EventSystem.current.SetSelectedGameObject(currentItem);
            }

            
            if (!Cursor.visible) //just in case our cursor is not visible yet.
            {
                SystemCursor(true);
            }
            
            /*
             * 
            pointerDirection = new Vector2(0f,0f);
            // Mouse gives absolute screen position, assign directly
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                screenSpace, Mouse.current.position.ReadValue(), null, out Vector2 localPoint);
            //pointerPosition = localPoint;
             */
        }
        //Assign the corresponding value
    }
    
    private bool MouseOverButtonGroup()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = mousePosition;
        
        List<RaycastResult> raycastResultList = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResultList);
        for (int i = 0; i < raycastResultList.Count; i++)
        {
            if (raycastResultList[i].gameObject.GetComponent<ButtonGroup>() != null)
            {
                return true;
            }
        } 
        return false;
    }
    //----------------------------------------//
    //    End of Pointer related methods        // ------------------------------ //
    //----------------------------------------//
    
    [SerializeField] private Texture2D myCursorTexture;
    private void SystemCursor(bool cursor = false)
    {
        if (cursor)
        {
            
            Cursor.visible = true;
            Cursor.SetCursor(myCursorTexture, default, CursorMode.Auto);
        }
        else
        {
            Cursor.visible = false;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
    
    [SerializeField] private Button defaultButton;
    private void GamePadBehavior()
    {
        SystemCursor(false);
        pointer.SetActive(true);
    }

    private void MouseKeyboardBehavior()
    {
        pointer.SetActive(false);
        //Clear event system selection
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void OnBackButton()
    {
        backButton?.Invoke();
    }

}
