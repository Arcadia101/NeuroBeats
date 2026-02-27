using UnityEngine;
using UnityEngine.SceneManagement;
using STOP_MODE = FMOD.Studio.STOP_MODE;

/// <summary>
/// Singleton que gestiona el flujo multi-nivel y mantiene los managers persistentes.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Level Configuration")]
    [Tooltip("Config ScriptableObject para el nivel actual.")]
    public LevelConfig currentConfig;

    private void Awake()
    {
        // Singleton básico
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Subscribir al evento de carga de escena
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// Instancia todo al cargar la escena de juego.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Si volvemos al menú principal, asegurarnos de poner la música del menú
        if (scene.name == "MainMenu" || scene.name == "MenuLevel")
        {
             if (FMODMusicConductor.Instance != null)
                FMODMusicConductor.Instance.RestartWith("event:/Music/MainMenu");
             return;
        }

        // Solo procesar si configuraste currentConfig y la escena coincide
        if (currentConfig == null || scene.name != currentConfig.sceneName)
            return;

        // 1) Reiniciar música
        if (FMODMusicConductor.Instance != null)
            FMODMusicConductor.Instance.RestartWith(currentConfig.musicEvent);
    
        // 2) Reconfigurar spawner de marcadores
        var spawner = FindAnyObjectByType<MarkerDrivenSpawner>();
        if (spawner != null)
        {
            string markerFile = currentConfig.markerJsonFileName;
            if (!markerFile.EndsWith(".json"))
                markerFile += ".json";
            spawner.SetMusicEvent(currentConfig.musicEvent);
            spawner.SetMarkerFile(markerFile);
        }
    
        // 3) Cambiar fondo si lo designaste
        if (currentConfig.background != null)
        {
            GameObject bg = null;
            try { bg = GameObject.FindWithTag("Background"); }
            catch { /* Si no existe la tag, ignorar */ }
            if (bg != null)
            {
                var sr = bg.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.sprite = currentConfig.background;
            }
        }
    
        // 4) Resetear managers de forma segura
        ComboManager.Instance?.Reset();
        NoteHistoryRecorder.Instance?.Reset();
        LevelExportController.Instance?.Reset();
        LevelEndController.Instance?.Reset();
    }


    /// <summary>
    /// Invocado por el menú para iniciar un nivel.
    /// </summary>
    public void LoadLevel(LevelConfig config)
    {
        currentConfig = config;
        Debug.Log($"[GameManager] Loading level: {config.sceneName}");
        try
        {
            SceneManager.LoadScene(config.sceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] Failed to load scene {config.sceneName}: {e.Message}");
        }
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}
