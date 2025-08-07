using UnityEngine;

/// <summary>
/// Singleton que expone el tiempo actual de reproducción de FMODMusicConductor.
/// No crea ni arranca su propio EventInstance.
/// </summary>
public class MusicTimeTracker : MonoBehaviour
{
    public static MusicTimeTracker Instance { get; private set; }

    /// <summary>
    /// Tiempo de canción en segundos, obtenido de FMODMusicConductor.
    /// </summary>
    public float CurrentSongTime => FMODMusicConductor.Instance != null
        ? FMODMusicConductor.Instance.CurrentSongTime
        : 0f;

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
}