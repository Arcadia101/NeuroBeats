using UnityEngine;
using FMODUnity;

/// <summary>
/// ScriptableObject que encapsula la configuración de un nivel:
/// - Nombre de escena
/// - Evento de FMOD para la música
/// - Nombre del archivo JSON de marcadores
/// - Arte de fondo opcional
/// </summary>
[CreateAssetMenu(fileName = "LevelConfig", menuName = "Configs/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    [Tooltip("Nombre de la escena asociada a este nivel.")]
    public string sceneName;

    [EventRef]
    [Tooltip("Ruta del evento de música FMOD para este nivel.")]
    public string musicEvent;

    [Tooltip("Nombre del archivo JSON de marcadores en StreamingAssets.")]
    public string markerJsonFileName;

    [Tooltip("Sprite de fondo para este nivel (opcional)." )]
    public Sprite background;
}

