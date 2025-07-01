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

    [Header("FMOD Music Event")]
    public EventReference musicEvent;

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
        // Crea y arranca la instancia de música
        musicInstance = RuntimeManager.CreateInstance(musicEvent.Path);
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
        musicInstance.setParameterByName("ComboLevel", level);
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

    private void OnDestroy()
    {
        musicInstance.stop(STOP_MODE.IMMEDIATE);
        musicInstance.release();
    }
}
