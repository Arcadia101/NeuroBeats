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

    /// <summary>
    /// Exporta todos los resultados a un JSON en persistentDataPath.
    /// </summary>
    public void ExportToJson(string fileName = "NoteResults.json")
    {
        string json = JsonUtility.ToJson(new SerializationWrapper<NoteResult>(results), true);
        string path = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllText(path, json);
        Debug.Log($"Note results exported to: {path}");
    }

    // Helper para serializar listas con JsonUtility
    [System.Serializable]
    private class SerializationWrapper<T>
    {
        public List<T> list;
        public SerializationWrapper(List<T> list) { this.list = list; }
    }
}