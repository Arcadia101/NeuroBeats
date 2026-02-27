// NoteHistoryRecorder.cs
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// Singleton que acumula todos los NoteResult durante el nivel
/// y los exporta a un archivo JSON al finalizar.
/// </summary>
public class NoteHistoryRecorder : MonoBehaviour
{
    public static NoteHistoryRecorder Instance { get; private set; }

    private List<NoteResult> results = new List<NoteResult>();

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

    /// <summary>
    /// Registra un nuevo resultado de nota.
    /// </summary>
    public void Record(NoteResult result)
    {
        results.Add(result);
    }
    
    /// <summary>Resetea el historial de notas.</summary>
    public void Reset()
    {
        results.Clear(); // asumiendo List<NoteResult> results
    }

    /// <summary>
    /// Exporta todos los resultados a un JSON en la ruta especificada.
    /// Si pathOrFileName es solo un nombre, usa persistentDataPath (comportamiento legacy).
    /// Si es una ruta absoluta, usa esa ruta.
    /// </summary>
    public void ExportToJson(string pathOrFileName = "NoteResults.json")
    {
        string json = JsonUtility.ToJson(new SerializationWrapper<NoteResult>(results), true);
        
        string finalPath = pathOrFileName;
        
        // Si no es una ruta absoluta, asumimos que es solo un nombre de archivo y usamos persistentDataPath
        if (!Path.IsPathRooted(pathOrFileName))
        {
            finalPath = Path.Combine(Application.persistentDataPath, pathOrFileName);
        }

        File.WriteAllText(finalPath, json);
        Debug.Log($"Note results exported to: {finalPath}");
    }

    // Helper para serializar listas con JsonUtility
    [System.Serializable]
    private class SerializationWrapper<T>
    {
        public List<T> list;
        public SerializationWrapper(List<T> list) { this.list = list; }
    }
}
