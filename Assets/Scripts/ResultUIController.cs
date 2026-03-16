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
        // Verificar si LevelEndController existe antes de suscribirse
        if (LevelEndController.Instance != null)
        {
            LevelEndController.Instance.OnLevelEnd.AddListener(ShowResults);
        }
        else
        {
            // En escenas como el Tutorial, puede que no haya LevelEndController.
            // No es un error crítico, simplemente no nos suscribimos.
            // Debug.LogWarning("ResultUIController: LevelEndController not found.");
        }

        if (debugging && inputReader != null)
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
        if (resultMenu == null) return;

        resultMenu.alpha = 1;
        resultMenu.interactable = true;
        resultMenu.blocksRaycasts = true;

        if (inputReader != null)
        {
            inputReader.ChangeActionMap(InputReader.ActionMapType.UI);
            inputReader.UI_Navigate += HandleNavigate;
            inputReader.UI_Submit += HandleSubmit;
        }

        if (firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
        }
    }
    
    private void HideResults()
    {
        if (resultMenu == null) return;

        resultMenu.alpha = 0;
        resultMenu.interactable = false;
        resultMenu.blocksRaycasts = false;

        if (inputReader != null)
        {
            inputReader.UI_Navigate -= HandleNavigate;
            inputReader.UI_Submit -= HandleSubmit;
        }
    }

    // --- Navegación Manual --- //
    
    private void HandleNavigate(Vector2 direction)
    {
        if (Time.unscaledTime < nextNavigationTime) return;
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
            nextNavigationTime = Time.unscaledTime + navigationCooldown;
        }
    }

    private void HandleSubmit()
    {
        var current = EventSystem.current.currentSelectedGameObject;
        if (current != null)
        {
            ExecuteEvents.Execute(current, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
        }
    }

    // --- Métodos Públicos --- //

    public void RetryLevel(LevelConfig levelConfig)
    {
        if (inputReader != null)
        {
            inputReader.UI_Navigate -= HandleNavigate;
            inputReader.UI_Submit -= HandleSubmit;
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentConfig = levelConfig;
            GameManager.Instance.LoadLevel(levelConfig);
        }
    }

    public void MainMenu()
    {
        if (inputReader != null)
        {
            inputReader.UI_Navigate -= HandleNavigate;
            inputReader.UI_Submit -= HandleSubmit;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
