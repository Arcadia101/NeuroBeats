using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using STOP_MODE = FMOD.Studio.STOP_MODE;

/// <summary>
/// Gestiona la pista de música principal. Se controla explícitamente con RestartWith().
/// Evita arranques automáticos y duplicaciones.
/// </summary>
public class FMODMusicConductor : MonoBehaviour
{
    public static FMODMusicConductor Instance { get; private set; }

    [Header("Music Event")]
    [EventRef]
    [Tooltip("Path FMOD del evento musical actual.")]
    [SerializeField] private string musicEventPath;

    private EventInstance musicInstance;
    
    [Header("Combo thresholds")]
    [Tooltip("Valores de combo a partir de los cuales se activa cada nivel.")]
    [SerializeField] private List<int> comboThresholds = new List<int> { 0, 4, 7, 10 };

    private void Awake()
    {
        // Singleton persistente
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Detiene y libera la pista actual (si existe) y arranca nueva música.
    /// </summary>
    public void RestartWith(string newEventPath)
    {
        if (string.IsNullOrEmpty(newEventPath))
        {
            Debug.LogWarning("FMODMusicConductor.RestartWith: newEventPath está vacío.");
            return;
        }

        Debug.Log($"FMODMusicConductor: Restarting with event '{newEventPath}'");

        // Detener instancia previa
        try
        {
            if (musicInstance.isValid())
            {
                musicInstance.stop(STOP_MODE.IMMEDIATE);
                musicInstance.release();
            }
        }
        catch { }

        musicEventPath = newEventPath;

        // Crear y arrancar nueva instancia
        musicInstance = RuntimeManager.CreateInstance(musicEventPath);
        FMOD.RESULT result = musicInstance.start();
        if (result != FMOD.RESULT.OK)
            Debug.LogWarning($"FMODMusicConductor: fallo al iniciar evento {musicEventPath}: {result}");

        // Forzar actualización del sistema para prevenir buffer starvation
        RuntimeManager.StudioSystem.update();
    }

    private void OnDestroy()
    {
        // Asegurar detención al destruir
        try
        {
            if (musicInstance.isValid())
                musicInstance.stop(STOP_MODE.IMMEDIATE);
        }
        catch { }
    }

    /// <summary>
    /// Posición actual de la canción en segundos.
    /// </summary>
    public float CurrentSongTime
    {
        get
        {
            try
            {
                if (musicInstance.isValid())
                {
                    musicInstance.getTimelinePosition(out int ms);
                    return ms / 1000f;
                }
            }
            catch { }
            return 0f;
        }
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

    
}

	