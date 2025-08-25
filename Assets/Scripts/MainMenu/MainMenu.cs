using UnityEngine;

public class MainMenu : MonoBehaviour
{
    //----------------------------------------//
    //    Reference to the Singletone         //------------------------------ //
    //----------------------------------------//
    public static MainMenu Instance;

    
    //----------------------------------------//
    /*
     
     
     */
    //----------------------------------------//
    
    //----------------------------------------//
    //    Reference to the Buttons and interfaces // ------------------------------ //
    //----------------------------------------//
    [SerializeField] private GameObject MM_Buttons;
    [SerializeField] private GameObject OptionsInterface;
    [SerializeField] private GameObject TitleScreen;
    [SerializeField] private GameObject GameTitle;
    [SerializeField] private GameObject LoadMenu;
    [SerializeField] private GameObject NewGame_panel; 

    //----------------------------------------//
    //               Methods                  // ------------------------------ //
    //----------------------------------------//
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

    //----------------------------------------//
    //      To display the new game interface    // ------------------------------ //
    //----------------------------------------//
    public void NewGame() //this is called from hierarchy MM_Buttons > New Game (the button)
    {
        //Debug.Log("New Game button");
        NewGame_panel.SetActive(true); 
    }

    //----------------------------------------//
    //      To start a new game - PART OF NewGame Interface     // ------------------------------ //
    //----------------------------------------//
    
    public void StartNewGame(LevelConfig config)
    {
        //Debug.Log("Start New Game");
       // GameScenesManager.Instance.ChangeScene(SceneName.GameEnvironment);
       
       if (GameManager.Instance != null)
       {
           GameManager.Instance.LoadLevel(config); //este es el de camilo

       }
       else
       {
           GameScenesManager.Instance.ChangeScene(SceneName.LevelMenu); //Este es el mio
       }
    }

    //----------------------------------------//
    //   To start the tutorial- PART OF NewGame Interface    // ------------------------------ //
    //----------------------------------------//
    public void StartTutorial(LevelConfig config)
    {
        if (GameManager.Instance != null) //este es el de camilo
        {
            GameManager.Instance.LoadLevel(config);
        }
        else
        {
            GameScenesManager.Instance.ChangeScene(SceneName.Tutorial); //Este es el mio
        }
    }

    //----------------------------------------//
    //   To load a saved game.     // ------------------------------ //
    //----------------------------------------//
    public void LoadGame()
    {
        //Debug.Log("Load Game");
        MM_Buttons.gameObject.SetActive(false);
        LoadMenu.gameObject.SetActive(true);
    }

    //----------------------------------------//
    //  To display the options menu    // ------------------------------ //
    //----------------------------------------//
    public void OptionsMenu()
    {
        //Debug.Log("Options Menu");
        MM_Buttons.gameObject.SetActive(false);
        OptionsInterface.SetActive(true);
        
    }

    //----------------------------------------//
    //  To load the credits scene   // ------------------------------ //
    //----------------------------------------//
    public void CreditsMenu()
    {
        //Debug.Log("Credits Menu");
        GameScenesManager.Instance.ChangeScene(SceneName.Credits);
    }

    //----------------------------------------//
    //  to load the What's new scene           // ------------------------------ //
    //it is likely that this scene doesn't exist yet //
    //----------------------------------------//
    public void WhatsNewMenu()
    {
        //Debug.Log("Whats New Menu");
    }

    //----------------------------------------//
    //  To quit the game                        // ------------------------------ //
    //----------------------------------------//
    public void QuitGame()
    {
        //Debug.Log("Quit Game");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #elif UNITY_WEBPLAYER
            Application.OpenURL(webplayerQuitURL);
        #else
            Application.Quit();
        #endif
        
    }

    //----------------------------------------//
    //  To display the main menu interface . Also acts as a reset of the main menu interface             // ------------------------------ //
    //----------------------------------------//
    public void MainMenu_Interface()
    {
        //Debug.Log("Main Menu");
        MM_Buttons.gameObject.SetActive(false); //This is like a reset of the menu
        MM_Buttons.gameObject.SetActive(true);
    }
    

}
