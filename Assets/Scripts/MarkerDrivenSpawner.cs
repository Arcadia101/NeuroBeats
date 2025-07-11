using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Random = UnityEngine.Random;

/// <summary>
/// Instancia notas basadas en marcadores de FMOD, adaptando spawn dinámico,
/// filtrado por nivel y modo de juego, y evita duplicar corutinas.
/// </summary>
public class MarkerDrivenSpawner : MonoBehaviour
{
    [Header("FMOD Music Event")]
    [EventRef]
    [Tooltip("Path FMOD del evento de música actual.")]
    [SerializeField] private string musicEventPath;

    [Header("Spawn Config")]
    [Tooltip("Prefab de nota a instanciar.")]
    [SerializeField] private GameObject notePrefab;
    [Tooltip("Segundos de anticipación antes del marker.")]
    [SerializeField] private float anticipationSeconds = 4f;
    [Tooltip("espacio entre el cual una nota se puede spawnear verticalmente.")]
    [SerializeField] private float verticalOffset = 3f;
    [Tooltip("distancia desde la cual una nota spawnea horizontalmente.")]
    [SerializeField] private float horizontalOffset = 10f;

    [Header("Markers JSON")]
    [Tooltip("Nombre de archivo JSON (en StreamingAssets) con marcadores.")]
    [SerializeField] private string markerJsonFileName;

    [Header("Managers")]
    [Tooltip("Referencia al TargetZoneManager para asignar pulsadores.")]
    [SerializeField] private TargetZoneManager targetManager;

    // Lista interna de notas pendientes de spawn
    private List<NoteSpawnData> pendingNotes = new List<NoteSpawnData>();
    private Coroutine schedulerCoroutine;

    private void Awake()
    {
        // No hacemos nada en Start; esperamos a SetMarkerFile
    }

    /// <summary>
    /// Establece el evento FMOD para este spawner.
    /// Debe llamarse antes de SetMarkerFile.
    /// </summary>
    public void SetMusicEvent(string newEventPath)
    {
        musicEventPath = newEventPath;
    }

    /// <summary>
    /// Carga y procesa el JSON de marcadores.
    /// Luego reinicia la corutina de spawn.
    /// </summary>
    public void SetMarkerFile(string fileName)
    {
        // Asegurar extensión .json
        markerJsonFileName = fileName.EndsWith(".json") ? fileName : fileName + ".json";
        pendingNotes.Clear();

        // Ruta completa
        string path = Path.Combine(Application.streamingAssetsPath, markerJsonFileName);
        if (!File.Exists(path))
        {
            Debug.LogError("MarkerDrivenSpawner: archivo de marcadores no encontrado: " + path);
            return;
        }

        // Leer texto
        string json = File.ReadAllText(path);

        // Deserializar en lista genérica
        List<MarkerData> markersList = new List<MarkerData>();
        if (json.Contains("\"events\""))
        {
            // Formato antiguo: wrapper.events
            MarkerExport wrapper = JsonUtility.FromJson<MarkerExport>(json);
            foreach (var ev in wrapper.events)
            {
                if (ev.eventPath == musicEventPath)
                {
                    markersList.AddRange(ev.markers);
                    break;
                }
            }
        }
        else
        {
            // Formato individual
            SingleEventExport single = JsonUtility.FromJson<SingleEventExport>(json);
            if (single != null && single.eventPath == musicEventPath)
                markersList.AddRange(single.markers);
        }

        // Orden y llenar pendingNotes
        markersList.Sort((a, b) => a.time.CompareTo(b.time));
        foreach (var m in markersList)
        {
            float t = m.time / 1000f;
            pendingNotes.Add(new NoteSpawnData(Vector3.zero, t, m.name));
        }

        // Iniciar/reiniciar la rutina de spawn
        RestartScheduler();
    }

    /// <summary>
    /// Detiene corutina previa (si existe) y arranca SpawnScheduler.
    /// </summary>
    private void RestartScheduler()
    {
        if (schedulerCoroutine != null)
            StopCoroutine(schedulerCoroutine);
        schedulerCoroutine = StartCoroutine(SpawnScheduler());
    }

    /// <summary>
    /// Corutina que revisa cada frame si alguna nota debe spawnear.
    /// </summary>
    private IEnumerator SpawnScheduler()
    {
        const float scheduleInterval = 0.05f; // 50 ms
        while (true)
        {
            yield return new WaitForSeconds(scheduleInterval);
            // Tiempo actual de la canción
            float currentTime = FMODMusicConductor.Instance.CurrentSongTime;

            for (int i = pendingNotes.Count - 1; i >= 0; i--)
            {
                var note = pendingNotes[i];
                if (note.arrivalTime - currentTime <= anticipationSeconds)
                {
                    SpawnNoteIfAllowed(note);
                    pendingNotes.RemoveAt(i);
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// Comprueba reglas de spawn (ComboX, EndSong, GameMode) y spawnea.
    /// </summary>
    private void SpawnNoteIfAllowed(NoteSpawnData note)
    {
        bool shouldSpawn = false;
        string name = note.markerName;

        // Combo markers
        if (name.StartsWith("Combo"))
        {
            if (int.TryParse(name.Substring(5), out int x))
            {
                if (x == 1)
                {
                    // Combo1 siempre
                    shouldSpawn = true;
                }
                else
                {
                    int req = x - 1;
                    if (GameState.Instance.CurrentMode == GameMode.Normal &&
                        ComboManager.Instance.CurrentCombo >= req)
                        shouldSpawn = true;
                }
            }
        }
        // EndSong marker
        else if (name == "EndSong")
        {
            LevelExportController.Instance.ExportNow();
            LevelEndController.Instance.TriggerEnd();
            return;
        }
        else
        {
            // Otros markers
            shouldSpawn = true;
        }

        if (shouldSpawn)
            SpawnNote(note);
    }

    /// <summary>
    /// Instancia la nota y la inicializa con toda la información.
    /// </summary>
    private void SpawnNote(NoteSpawnData note)
    {
        // Elegir tipo aleatorio
        NoteInputType type = GetRandomNoteType();
        // Pedir pulsador libre
        Transform zone = targetManager.RequestRandomTarget(type);
        if (zone == null)
        {
            Debug.LogWarning($"No available targets for {note.markerName}");
            return;
        }
        // Determinar posición de aparición lateral
        Vector3 spawnPos = GetSpawnPosition(type);
        // Instanciar prefab
        var go = Instantiate(notePrefab, spawnPos, Quaternion.identity);
        var behavior = go.GetComponent<NoteBehavior>();
        var btn = zone.GetComponent<PlayerButton>();

        // Inicializar note (movimiento + evaluación)
        behavior.Initialize(btn, targetManager, note.arrivalTime,
                             FMODMusicConductor.Instance.CurrentSongTime, type);
    }

    /// <summary>Tipos de export JSON individual.</summary>
    [Serializable]
    private class SingleEventExport
    {
        public string eventPath;
        public MarkerData[] markers;
    }
    /// <summary>
    /// Selección aleatoria del tipo de input requerido por la nota.
    /// </summary>
    private NoteInputType GetRandomNoteType()
    {
        var values = System.Enum.GetValues(typeof(NoteInputType));
        return (NoteInputType)values.GetValue(Random.Range(0, values.Length));
    }
    
    
    /// <summary>
    /// Selección aleatoria de la ubiacion de inicio requerida por la nota.
    /// </summary>
    private Vector3 GetSpawnPosition(NoteInputType type)
    {
        float x = 0f;
        float y = Random.Range(-verticalOffset, verticalOffset); // Altura aleatoria opcional

        switch (type)
        {
            case NoteInputType.LB:
            case NoteInputType.LT:
                x = -horizontalOffset; // Lado izquierdo
                break;
            case NoteInputType.RB:
            case NoteInputType.RT:
                x = horizontalOffset; // Lado derecho
                break;
        }

        return new Vector3(x, y, 0f);
    }

    
}

