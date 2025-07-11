using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class FMODMusicConductor : MonoBehaviour
{
    public static FMODMusicConductor Instance { get; private set; }

    [EventRef] [SerializeField] private string musicEventPath;
    private EventInstance musicInstance;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Para la música anterior (si la hay) y arranca el nuevo evento.
    /// </summary>
    public void RestartWith(string newEventPath)
    {
        if (string.IsNullOrEmpty(newEventPath))
        {
            Debug.LogWarning("FMODMusicConductor.RestartWith: newEventPath vacío.");
            return;
        }
        // Detener / liberar anterior
        if (musicInstance.isValid())
            musicInstance.stop(STOP_MODE.IMMEDIATE);
        musicInstance.release();

        // Nuevo
        musicEventPath = newEventPath;
        musicInstance = RuntimeManager.CreateInstance(musicEventPath);
        var res = musicInstance.start();
        if (res != FMOD.RESULT.OK)
            Debug.LogWarning($"FMODMusicConductor: fallo al iniciar {musicEventPath}: {res}");

        // Forzar update apenas arranca para evitar starvation
        RuntimeManager.StudioSystem.update();
    }

    public float CurrentSongTime
    {
        get
        {
            if (!musicInstance.isValid()) return 0;
            musicInstance.getTimelinePosition(out int ms);
            return ms / 1000f;
        }
    }

    private void OnDestroy()
    {
        if (musicInstance.isValid())
            musicInstance.stop(STOP_MODE.IMMEDIATE);
    }
}


	