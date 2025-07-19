using System;
using System.Collections;
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
        Debug.Log("[FMODMusicConductor] Awake called on " + gameObject.name);
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        RestartWith(musicEventPath);
    }

    /// <summary>
    /// Interpola suavemente el parámetro dado al valor target en duration segundos.
    /// </summary>
    public void RampParameter(string paramName, float targetValue, float duration)
    {
        // Si ya hay una rampa en curso, la detenemos
        StopCoroutine(ParameterLerp(paramName, 0, duration));
        StartCoroutine(ParameterLerp(paramName, targetValue, duration));
    }

    private IEnumerator ParameterLerp(string paramName, float target, float duration)
    {
        // 1) Obtenemos el valor de inicio
        FMODUnity.RuntimeManager.StudioSystem.getParameterByName(paramName, out float startValue);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float current = Mathf.Lerp(startValue, target, t);
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName(paramName, current);
            yield return null;
        }

        // Aseguramos valor final exacto
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName(paramName, target);
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


	