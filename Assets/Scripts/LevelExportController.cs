using UnityEngine;

/// <summary>
/// Controla la exportación del historial de notas (NoteHistoryRecorder) a un JSON
/// al finalizar el nivel o al invocar manualmente.
/// </summary>
public class LevelExportController : MonoBehaviour
{
    [Tooltip("Nombre del archivo JSON donde se guardarán los resultados.")]
    [SerializeField] private string fileName = "NoteResults.json";

    // Para garantizar que sólo se exporta una vez
    private bool hasExported = false;

    /// <summary>
    /// Llamado automáticamente cuando la escena se desactiva (cambio de nivel, cierre).
    /// </summary>
    private void OnDisable()
    {
        TryExport();
    }

    /// <summary>
    /// Llamado justo antes de que la aplicación se cierre.
    /// </summary>
    private void OnApplicationQuit()
    {
        TryExport();
    }

    /// <summary>
    /// Método público para invocar la exportación desde un botón UI, si lo deseas.
    /// </summary>
    public void ExportNow()
    {
        TryExport();
    }

    /// <summary>
    /// Realiza la exportación sólo si no se ha hecho ya y el recorder está presente.
    /// </summary>
    private void TryExport()
    {
        if (hasExported) return;
        if (NoteHistoryRecorder.Instance == null)
        {
            Debug.LogWarning("LevelExportController: NoteHistoryRecorder no encontrado. No se exportará JSON.");
            return;
        }

        NoteHistoryRecorder.Instance.ExportToJson(fileName);
        Debug.Log($"LevelExportController: Resultados exportados a {fileName}");
        hasExported = true;
    }
}