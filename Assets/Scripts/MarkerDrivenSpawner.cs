using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

/// <summary>
/// Instancia notas según marcadores leídos desde JSON y las configura con tipo e input aleatorios.
/// La lógica de anticipación se usa para generar cada nota con el tiempo justo antes del target.
/// </summary>
public class MarkerDrivenSpawner : MonoBehaviour
{
    
    [Header("Spawn Config")]
    [SerializeField] private GameObject notePrefab;
    [SerializeField] private float anticipationSeconds = 4.0f; // Tiempo de antelación para spawn

    [Header("Managers")]
    [SerializeField] private TargetZoneManager targetManager;

    private EventInstance musicInstance;
    private List<NoteSpawnData> pendingNotes = new();
    private FMODMusicConductor conductor;

	

    void Start()
    {
		conductor = FMODMusicConductor.Instance;
        // Cargar datos desde el archivo JSON
        string markerPath = System.IO.Path.Combine(Application.streamingAssetsPath, "markers.json");
        if (!System.IO.File.Exists(markerPath))
        {
            Debug.LogError("Archivo de marcadores no encontrado.");
            return;
        }

        string json = System.IO.File.ReadAllText(markerPath);
        MarkerExport data = JsonUtility.FromJson<MarkerExport>(json);

        MarkerData[] markers = null;
        foreach (var e in data.events)
        {
            if (e.eventPath == conductor.musicEvent.Path)
            {
                markers = e.markers;
                break;
            }
        }

        if (markers == null)
        {
            Debug.LogError("No se encontraron marcadores para el evento.");
            return;
        }

        // Agrega todos los marcadores a la lista de espera
        foreach (var marker in markers)
        {
            float markerTime = marker.time / 1000f;
            NoteSpawnData spawnData = new(
                Vector3.zero,
                markerTime,
                marker.name
            );

            pendingNotes.Add(spawnData);
        }

        // Comienza la rutina de spawn
        StartCoroutine(SpawnScheduler());
    }

    /// <summary>
    /// Revisa continuamente si alguna nota debe ser instanciada pronto.
    /// </summary>
    IEnumerator SpawnScheduler()
    {
        while (true)
        {
            float currentTime = FMODMusicConductor.Instance.CurrentSongTime;

            for (int i = pendingNotes.Count - 1; i >= 0; i--)
            {
                var note = pendingNotes[i];
                if (note.arrivalTime - currentTime <= anticipationSeconds)
                {
                    SpawnNote(note);
                    pendingNotes.RemoveAt(i);
                }
            }

            yield return null;
        }
    }

    /// <summary>
    /// Instancia y configura una nueva nota.
    /// </summary>
    void SpawnNote(NoteSpawnData note)
    {
        // 1. Seleccionar un tipo aleatorio
        NoteInputType type = GetRandomNoteType();

        // 2. Obtener target libre (compatible con ese tipo)
        Transform zone = targetManager.RequestTarget(type);
        if (zone == null)
        {
            Debug.LogWarning($"No available targets for {note.markerName}");
            return;
        }

        // 3. Instanciar la nota y configurar su trayectoria
        GameObject go = Instantiate(notePrefab, GetSpawnPosition(type), Quaternion.identity);
        NoteBehavior behavior = go.GetComponent<NoteBehavior>();

        PlayerButton btn = zone.GetComponent<PlayerButton>();
        behavior.Initialize(btn, targetManager, note.arrivalTime, FMODMusicConductor.Instance.CurrentSongTime, type, GetSpawnSide(type));
    }

    /// <summary>
    /// Determina una posición de aparición basada en el tipo.
    /// </summary>
    private Vector3 GetSpawnPosition(NoteInputType type)
    {
        float x = (type == NoteInputType.LB || type == NoteInputType.LT) ? -10f : 10f;
        float y = Random.Range(-3f, 3f);
        return new Vector3(x, y, 0f);
    }

    private string GetSpawnSide(NoteInputType type)
    {
        return type == NoteInputType.LB || type == NoteInputType.LT ? "left" : "right";
    }
    
    
    /// <summary>
    /// Selección aleatoria del tipo de input requerido por la nota.
    /// </summary>
    private NoteInputType GetRandomNoteType()
    {
        var values = System.Enum.GetValues(typeof(NoteInputType));
        return (NoteInputType)values.GetValue(Random.Range(0, values.Length));
    }


    
}
