using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Level1Behavior : MonoBehaviour
{
    [SerializeField] private TutorialInfo gamepad;
    [SerializeField] private TutorialInfo keyboard;
    public TutorialInfo currentScheme;
    public Sprite spriteBLeft;
    public Sprite spriteTLeft;
    public Sprite spriteBRight;
    public Sprite spriteTRight;
    bool isGamepadActive = false;
    bool isMouseActive = false;
    PlayerInput playerInput;
    
    
    public event UnityAction controlsChanged;
    private void OnEnable()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInput.onControlsChanged += UpdateIcons;
    }
    private void OnDisable(){
        playerInput.onControlsChanged -= UpdateIcons;
    }
   
    public void UpdateIcons(PlayerInput input)
    {
        var scheme = input.currentControlScheme;
        isGamepadActive = scheme == "Gamepad";
        isMouseActive = scheme == "Keyboard&Mouse";
        if (isMouseActive)
        {
            currentScheme = keyboard;
        }

        if (isGamepadActive)
        {
            currentScheme = gamepad;
        }
        controlsChanged?.Invoke();
    }

}
