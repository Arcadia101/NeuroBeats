using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton que controla la exportación del historial de notas a JSON
/// y genera el nombre de archivo según nivel, fecha y hora.
/// </summary>
public class LevelExportController : MonoBehaviour
{
    public static LevelExportController Instance { get; private set; }
    
    [Tooltip("Ruta de carpeta donde se guardará el JSON. Si está vacío, usa persistentDataPath.")]
    [SerializeField] private string outputDirectory = "";

    // Para evitar múltiples exportaciones
    private bool hasExported = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDisable()
    {
        TryExport();
    }

    private void OnApplicationQuit()
    {
        TryExport();
    }

    /// <summary>
    /// Invoca manualmente la exportación.
    /// </summary>
    public void ExportNow()
    {
        TryExport();
    }

    /// <summary>Resetea el estado para permitir nueva exportación.</summary>
    public void Reset()
    {
        hasExported = false;
    }

    /// <summary>
    /// Realiza la exportación si no se ha hecho antes y el recorder existe.
    /// </summary>
    private void TryExport()
    {
        if (hasExported) return;

        if (NoteHistoryRecorder.Instance == null)
        {
            Debug.LogWarning("LevelExportController: NoteHistoryRecorder no encontrado.");
            return;
        }

        string levelName = SceneManager.GetActiveScene().name;
        if (levelName == "Menu" || levelName == "LevelMenu") return;

        string timestamp = DateTime.Now.ToString("yyMMdd_HHmm");
        string fileName = $"Evaluacion_{levelName}_{timestamp}.json";

        // Determinar la ruta según el entorno
        string directory;

#if UNITY_EDITOR
        // En el editor, guardar dentro del proyecto (por ejemplo, en Assets/Exports)
        directory = string.IsNullOrEmpty(outputDirectory)
            ? Path.Combine(Application.dataPath, "Exports")
            : outputDirectory;
#else
        // En la build, usar el directorio ejecutable o la ruta configurada
        directory = string.IsNullOrEmpty(outputDirectory)
            ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExportedTests")
            : outputDirectory;
#endif

        // Crear carpeta si no existe
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        string fullPath = Path.Combine(directory, fileName);

        // Exportar
        NoteHistoryRecorder.Instance.ExportToJson(fullPath);

        Debug.Log($"LevelExportController: Resultados exportados a {fullPath}");
        hasExported = true;
    }
}
