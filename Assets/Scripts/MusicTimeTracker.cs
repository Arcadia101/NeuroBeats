using UnityEngine;
using FMOD.Studio; // Asegúrate de tener esto para acceder a EventDescription

public class MusicTimeTracker : MonoBehaviour
{
    public static MusicTimeTracker Instance { get; private set; }

    /// <summary>
    /// Tiempo de canción en segundos, obtenido de FMODMusicConductor.
    /// </summary>
    public float CurrentSongTime => FMODMusicConductor.Instance != null
        ? FMODMusicConductor.Instance.CurrentSongTime
        : 0f;

    [Header("Song Progress")]
    [SerializeField] private bool songIsPlaying = false; // Setea esto cuando inicies la canción
    private float songDuration = 0f; // Duración total en segundos

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
        // Obtener duración total de la canción desde FMOD
        CalculateSongDuration();
    }

    private void Update()
    {
        if (songIsPlaying && songDuration > 0f)
        {
            float progress = CurrentSongTime / songDuration; // Valor entre 0 y 1
            InfoController.Instance.UpdateProgress(progress);
        }
    }

    private void CalculateSongDuration()
    {
        if (FMODMusicConductor.Instance == null)
            return;

        // Asumiendo que FMODMusicConductor tiene una referencia al EventInstance o EventDescription
        // Si no, expón un método como GetEventDescription() en FMODMusicConductor
        EventDescription eventDesc = FMODMusicConductor.Instance.GetEventDescription(); // Ajusta esto si es necesario

        if (eventDesc.isValid())
        {
            int lengthMs;
            eventDesc.getLength(out lengthMs);
            songDuration = lengthMs / 1000f; // Convertir ms a segundos
        }
        else
        {
            Debug.LogWarning("No se pudo obtener la duración de la canción de FMOD.");
        }
    }

    // Método público para setear si la canción está reproduciéndose (llámalo desde donde inicies el audio)
    public void SetSongPlaying(bool isPlaying)
    {
        songIsPlaying = isPlaying;
        if (isPlaying)
        {
            CalculateSongDuration(); // Recalcular por si cambia la canción
        }
    }
}