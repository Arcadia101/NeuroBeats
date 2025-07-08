using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;
using STOP_MODE = FMOD.Studio.STOP_MODE;

/// <summary>
/// Controla la reproducción de la música principal y ajusta el parámetro
/// "ComboLevel" en FMOD según thresholds configurables desde el Inspector.
/// </summary>
public class FMODMusicConductor : MonoBehaviour
{
    public static FMODMusicConductor Instance { get; private set; }
    
    public List<NoteSpawnData> upcomingNotes = new List<NoteSpawnData>();

    [Header("FMOD Music Event")]
    //public FMODUnity.EventReference musicEvent;
    
    [EventRef] public string musicEventPath = "event:/Music/Level1";

    [Header("Combo thresholds")]
    [Tooltip("Valores de combo a partir de los cuales se activa cada nivel.")]
    [SerializeField] private List<int> comboThresholds = new List<int> { 0, 4, 7, 10 };

    private EventInstance musicInstance;

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

    private void Start()
    {
        Debug.Log("[FMODMusicConductor] Starting music: " + musicEventPath);
        // Crea y arranca la instancia de música
        musicInstance = RuntimeManager.CreateInstance(musicEventPath);
        musicInstance.start();
    }

    /// <summary>
    /// Ajusta el parámetro "ComboLevel" en FMOD según thresholds.
    /// </summary>
    public void SetComboLevel(int combo)
    {
        // Determina el nivel de combo basado en los thresholds
        int level = 0;
        for (int i = 0; i < comboThresholds.Count; i++)
        {
            if (combo >= comboThresholds[i])
                level = i;
            else
                break;
        }

        // Envía el parámetro por nombre directamente
        RuntimeManager.StudioSystem.setParameterByName("ComboLevel", level);
    }

    /// <summary>
    /// Expone el tiempo actual de la canción (en segundos).
    /// </summary>
    public float CurrentSongTime
    {
        get
        {
            musicInstance.getTimelinePosition(out int ms);
            return ms / 1000f;
        }
    }
    
    private void OnDrawGizmos()
    {
        // Obtén CurrentSongTime y dibuja una línea vertical en X = CurrentSongTime
        float t = Application.isPlaying 
            ? Instance.CurrentSongTime 
            : 0f;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(t, -5, 0), new Vector3(t, 5, 0));

        // Dibuja puntos para cada upcomingNotes[i].arrivalTime
        Gizmos.color = Color.yellow;
        foreach (var note in upcomingNotes)
        {
            float x = note.arrivalTime;
            Gizmos.DrawSphere(new Vector3(x, 0, 0), 0.1f);
        }
    }

    private void OnDestroy()
    {
        musicInstance.stop(STOP_MODE.IMMEDIATE);
        musicInstance.release();
    }
}
