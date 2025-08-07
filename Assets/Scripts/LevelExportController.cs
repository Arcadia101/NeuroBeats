using System;
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

        string directory = string.IsNullOrEmpty(outputDirectory)
            ? Application.persistentDataPath
            : outputDirectory;

        string fullPath = System.IO.Path.Combine(directory, fileName);

        NoteHistoryRecorder.Instance.ExportToJson(fullPath);
        Debug.Log($"LevelExportController: Resultados exportados a {fullPath}");
        hasExported = true;
    }
}
