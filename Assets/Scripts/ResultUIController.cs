using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ResultUIController : MonoBehaviour
{
    [SerializeField] private CanvasGroup resultMenu;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Button firstSelectedButton; 
    [SerializeField] bool debugging = false;

    // Cooldown para evitar saltos dobles en la navegación
    [SerializeField] private float navigationCooldown = 0.25f;
    private float nextNavigationTime = 0f;

    private void Awake()
    {
        LevelEndController.Instance.OnLevelEnd.AddListener(ShowResults);
        if (debugging)
        {
            inputReader.DebugButton  += ShowResults;    
        }
    }

    private void Start()
    {
        HideResults();
    }

    private void ShowResults()
    {
        resultMenu.alpha = 1;
        resultMenu.interactable = true;
        resultMenu.blocksRaycasts = true;

        // 1. Cambiar Action Map
        inputReader.ChangeActionMap(InputReader.ActionMapType.UI);

        // 2. Suscribirse a eventos de navegación manual
        inputReader.UI_Navigate += HandleNavigate;
        inputReader.UI_Submit += HandleSubmit;

        // 3. Forzar selección inicial
        if (firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
        }
    }
    
    private void HideResults()
    {
        resultMenu.alpha = 0;
        resultMenu.interactable = false;
        resultMenu.blocksRaycasts = false;

        inputReader.UI_Navigate -= HandleNavigate;
        inputReader.UI_Submit -= HandleSubmit;
    }

    // --- Navegación Manual --- //
    
    private void HandleNavigate(Vector2 direction)
    {
        // Verificar cooldown (usamos unscaledTime por si el juego está pausado)
        if (Time.unscaledTime < nextNavigationTime) return;

        // Filtrar ruido de inputs muy pequeños (deadzone manual)
        if (direction.magnitude < 0.5f) return;

        if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null)
        {
            if (firstSelectedButton != null)
                EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
            return;
        }

        AxisEventData data = new AxisEventData(EventSystem.current);
        data.moveDir = MoveDirection.None;

        if (direction.y > 0.5f) data.moveDir = MoveDirection.Up;
        else if (direction.y < -0.5f) data.moveDir = MoveDirection.Down;
        else if (direction.x > 0.5f) data.moveDir = MoveDirection.Right;
        else if (direction.x < -0.5f) data.moveDir = MoveDirection.Left;

        if (data.moveDir != MoveDirection.None)
        {
            ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, data, ExecuteEvents.moveHandler);
            // Aplicar cooldown tras un movimiento exitoso
            nextNavigationTime = Time.unscaledTime + navigationCooldown;
        }
    }

    private void HandleSubmit()
    {
        // También podemos poner cooldown al submit si fuera necesario, pero usualmente no hace falta
        var current = EventSystem.current.currentSelectedGameObject;
        if (current != null)
        {
            ExecuteEvents.Execute(current, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
        }
    }

    // --- Métodos Públicos --- //

    public void RetryLevel(LevelConfig levelConfig)
    {
        inputReader.UI_Navigate -= HandleNavigate;
        inputReader.UI_Submit -= HandleSubmit;
        
        GameManager.Instance.currentConfig = levelConfig;
        GameManager.Instance.LoadLevel(levelConfig);
    }

    public void MainMenu()
    {
        inputReader.UI_Navigate -= HandleNavigate;
        inputReader.UI_Submit -= HandleSubmit;

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
