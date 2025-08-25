using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

///----------------------------------------//
    /// <summary>
    /*
     
     Same definition as the MainMenu Controller:
     
    This is the UI equivalent of "Player Controller"
    It technically is a player controller because translates player inputs into game events.
    but it applies only to the context of UI
    more specifically, the Tutorial scene
    This was set as a singletone because for Neurobeats the tutorial level works purely based on UI
    We technically could have used the same  MainMenu script but to avoid confusion and mess we are creating a separate script.
    */ 
    /// <summary>
///----------------------------------------//
///
/// 
public class TutorialController : MonoBehaviour
{
    [SerializeField] private InputReader inputReader; // Reference to the input reader instance (we should assign this through the inspector)
    //----------------------------------------//
    //    Delegates definition         // ------------------------------ //
    //----------------------------------------//
    
    public static TutorialController Instance;
    
    public event UnityAction leftClick = delegate { }; //This is mostly for the Gamepad
    public event UnityAction rightClick = delegate { };
    public event UnityAction backButton = delegate { };
    public event UnityAction submitButton = delegate { };
    public event UnityAction startButton = delegate { };
    public event UnityAction previousButton = delegate { };
    public event UnityAction nextButton = delegate { };


    public event UnityAction<Vector2> navigation = delegate { };
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
        HandlePointer();
    }

    public enum InputScheme
    {
        Gamepad,
        Keyboard
    }

    public InputScheme currentScheme;
    public event UnityAction<InputScheme> controlsChanged;
    private void OnControlsChanged(PlayerInput input)
    {
        var scheme = input.currentControlScheme;
        isGamepadActive = scheme == "Gamepad";
        isMouseActive = scheme == "Keyboard&Mouse"; 
        Debug.Log($"Control Scheme Changed to: {scheme}");
        
        if (isGamepadActive)
        {
            currentScheme = InputScheme.Gamepad;
            EnableFakePointer();
            controlsChanged?.Invoke(InputScheme.Gamepad);
        }
        if (isMouseActive)
        {
            currentScheme = InputScheme.Keyboard;
            EnableSystemCursor();
            controlsChanged?.Invoke(InputScheme.Keyboard);
        }
    }
    
    private Canvas canvas;
    private RectTransform screenSpace;
    private void OnEnable()
    {
        canvas = transform.parent.GetComponent<Canvas>();
        screenSpace = canvas.GetComponent<RectTransform>();
        inputReader.ChangeActionMap(InputReader.ActionMapType.UI);
        inputReader.UI_Click += HandleClick;
        inputReader.UI_Navigate += HandleNavigate;
        inputReader.UI_Point += OnPointer;
        inputReader.UI_Back += OnBackButton;
        inputReader.UI_Submit += OnSubmitButton;
        inputReader.UI_Continue += OnStartButton;
        inputReader.UI_Previous += OnPreviousButton;
        inputReader.UI_Next += OnNextButton;
    }

    private void OnDisable()
    {
        inputReader.UI_Click -= HandleClick;
        inputReader.UI_Navigate -= HandleNavigate;
        inputReader.UI_Point -= OnPointer;
        inputReader.UI_Back -= OnBackButton;
        inputReader.UI_Submit -= OnSubmitButton;
        inputReader.UI_Continue -= OnStartButton;
        inputReader.UI_Previous -= OnPreviousButton;
        inputReader.UI_Next -= OnNextButton;
    }
    
    private void HandleClick()
    {
        //Debug.Log("Clicked!");
        pointer.GetComponent<FakePointer>().SimulateUIClick();
        leftClick?.Invoke();
    }

    private void HandleNavigate(Vector2 direction)
    {
        //Debug.Log("Navigated: " + direction);
        //cursorPosition = direction;
        navigation?.Invoke(direction);
    }
    //----------------------------------------//
    //    Pointer related methods               // ------------------------------ //
    //----------------------------------------//This requires you to create a pointer object in the UI space (I probably created a prefab for this already)
    public float pointerSpeed = 1f;
    [SerializeField] public GameObject pointer;
    public Vector2 pointerPosition = default;
    [SerializeField] public RectTransform pointerRectTransform;
    /// <summary>
    /// Here we handle the pointer once the inputs have been processed by OnPointer
    /// </summary>
    private void HandlePointer()
    {
        //modify the pointer position every frame (we are supposed to call this on update)
        pointerPosition += pointerDirection * (500f* pointerSpeed * Time.deltaTime);//"But Rafa, conmutative property for multiplication. Why between parenthesis?" because this way we multiply by a float twice instead of 6 times (without parenthesis). This is how Vector2 works.
        // Clamp to screen bounds
        pointerPosition.x = Mathf.Clamp(pointerPosition.x, -screenClampers.x, screenClampers.x);
        pointerPosition.y = Mathf.Clamp(pointerPosition.y, -screenClampers.y, screenClampers.y);
        
        // Apply position to UI
        pointerRectTransform.anchoredPosition = pointerPosition;
    }
    
    //Here we handle the pointer inputs
    Vector2 pointerDirection = new Vector2(0f,0f);
    private void OnPointer(Vector2 input)
    {
        
        if (isGamepadActive) //If the gamepad is active, what we change is the movement direction
        {
            // Delta movement (Vector2: -1, 0, or 1)
            pointerDirection = input;
        }
        if(isMouseActive) //If the mouse and keyboard is active, what we change is the absolute position
        {
            pointerDirection = new Vector2(0f,0f);
            // Mouse gives absolute screen position, assign directly
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                screenSpace, Mouse.current.position.ReadValue(), null, out Vector2 localPoint);
            pointerPosition = localPoint;
        }
        //Assign the corresponding value
    }
    //----------------------------------------//
    //    End of Pointer related methods        // ------------------------------ //
    //----------------------------------------//

    
    private void EnableFakePointer()
    {
        Cursor.visible = false;   
        pointer.SetActive(true);
        pointer.GetComponent<FakePointer>().enabled = true;
    }

    [SerializeField] private Texture2D myCursorTexture;
    private void EnableSystemCursor()
    {
        Cursor.visible = true;
        Cursor.SetCursor(myCursorTexture, default, CursorMode.Auto);
        pointer.GetComponent<FakePointer>().enabled = false;
        pointer.SetActive(false);
    }

    public void OnBackButton()
    {
        backButton?.Invoke();
    }

    public void OnSubmitButton()
    { 
        submitButton?.Invoke();
    }

    public void OnStartButton()
    {
        startButton?.Invoke();
    }

    public void OnNextButton()
    {
        nextButton?.Invoke();
    }

    public void OnPreviousButton()
    {
        previousButton?.Invoke();
    }

    
}
