using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GamePauseController : MonoBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private CanvasGroup pausePanel;
    [SerializeField] private Button firstSelectedButton;

    private bool isPaused = false;

    private void Start()
    {
        if (pausePanel != null)
        {
            pausePanel.alpha = 0;
            pausePanel.interactable = false;
            pausePanel.blocksRaycasts = false;
            pausePanel.gameObject.SetActive(true);
        }
    }

    private void OnEnable()
    {
        if (inputReader != null)
            inputReader.MenuButton += TogglePause;
    }

    private void OnDisable()
    {
        if (inputReader != null)
            inputReader.MenuButton -= TogglePause;
    }

    public void TogglePause()
    {
        // No pausar si ya terminó el nivel (LevelEndController)
        // (Opcional: verificar si LevelEndController.Instance.HasEnded si expones esa propiedad)
        
        isPaused = !isPaused;

        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
        
        // Pausar FMOD
        if (FMODMusicConductor.Instance != null && FMODMusicConductor.Instance.musicInstance.isValid())
        {
            FMODMusicConductor.Instance.musicInstance.setPaused(true);
        }

        if (pausePanel != null)
        {
            pausePanel.alpha = 1;
            pausePanel.interactable = true;
            pausePanel.blocksRaycasts = true;

            // Navegación UI
            if (inputReader != null) inputReader.ChangeActionMap(InputReader.ActionMapType.UI);
            
            if (firstSelectedButton != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
            }
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        // Reanudar FMOD
        if (FMODMusicConductor.Instance != null && FMODMusicConductor.Instance.musicInstance.isValid())
        {
            FMODMusicConductor.Instance.musicInstance.setPaused(false);
        }

        if (pausePanel != null)
        {
            pausePanel.alpha = 0;
            pausePanel.interactable = false;
            pausePanel.blocksRaycasts = false;
            
            // Volver a controles de juego
            if (inputReader != null) inputReader.ChangeActionMap(InputReader.ActionMapType.Player);
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        // Asegurar que FMOD se despausa antes de recargar (aunque RestartWith lo reinicia)
        if (FMODMusicConductor.Instance != null && FMODMusicConductor.Instance.musicInstance.isValid())
        {
            FMODMusicConductor.Instance.musicInstance.setPaused(false);
        }
        
        // Recargar escena actual
        if (GameManager.Instance != null && GameManager.Instance.currentConfig != null)
        {
            GameManager.Instance.LoadLevel(GameManager.Instance.currentConfig);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        if (FMODMusicConductor.Instance != null && FMODMusicConductor.Instance.musicInstance.isValid())
        {
            FMODMusicConductor.Instance.musicInstance.setPaused(false);
        }
        
        SceneManager.LoadScene("MainMenu");
    }
}
