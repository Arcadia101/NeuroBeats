using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance;

    [Header("UI References")]
    [SerializeField] private GameObject MM_Buttons;
    [SerializeField] private GameObject OptionsInterface;
    [SerializeField] private GameObject TitleScreen;
    [SerializeField] private GameObject GameTitle;
    [SerializeField] private GameObject LoadMenu;
    [SerializeField] private GameObject NewGame_panel; 

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
    }

    /// <summary>
    /// Muestra la interfaz de nuevo juego.
    /// </summary>
    public void NewGame() 
    {
        NewGame_panel.SetActive(true); 
    }

    /// <summary>
    /// Inicia un nuevo juego cargando el nivel especificado.
    /// </summary>
    public void StartNewGame(LevelConfig config)
    {
       if (GameManager.Instance != null)
       {
           GameManager.Instance.LoadLevel(config);
       }
       else
       {
           GameScenesManager.Instance.ChangeScene(SceneName.LevelMenu);
       }
    }

    /// <summary>
    /// Inicia el tutorial.
    /// </summary>
    public void StartTutorial(LevelConfig config)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadLevel(config);
        }
        else
        {
            GameScenesManager.Instance.ChangeScene(SceneName.Tutorial);
        }
    }

    /// <summary>
    /// Muestra el menú de cargar partida.
    /// </summary>
    public void LoadGame()
    {
        MM_Buttons.gameObject.SetActive(false);
        LoadMenu.gameObject.SetActive(true);
    }

    /// <summary>
    /// Muestra el menú de opciones.
    /// </summary>
    public void OptionsMenu()
    {
        MM_Buttons.gameObject.SetActive(false);
        OptionsInterface.SetActive(true);
    }

    /// <summary>
    /// Carga la escena de créditos.
    /// </summary>
    public void CreditsMenu()
    {
        GameScenesManager.Instance.ChangeScene(SceneName.Credits);
    }

    /// <summary>
    /// Muestra el menú de novedades (aún no implementado).
    /// </summary>
    public void WhatsNewMenu()
    {
        // TODO: Implementar menú de novedades
    }

    /// <summary>
    /// Cierra la aplicación.
    /// </summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    /// <summary>
    /// Resetea y muestra la interfaz principal del menú.
    /// </summary>
    public void MainMenu_Interface()
    {
        MM_Buttons.gameObject.SetActive(false); // Reset visual
        MM_Buttons.gameObject.SetActive(true);
    }
}
