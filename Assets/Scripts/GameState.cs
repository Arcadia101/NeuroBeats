using UnityEngine;

// Defines the game modes
public enum GameMode
{
    Normal,
    Investigacion
}

/// <summary>
/// Singleton storing global game state like current mode.
/// </summary>
/// <summary>
/// Singleton storing global game mode set via Inspector (not runtime-changeable).
/// </summary>
public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    [Header("Game Version")]
    [Tooltip("Select the game mode: Normal (public) or Investigacion.")]
    [SerializeField] private GameMode currentMode = GameMode.Normal;

    /// <summary>
    /// Current game mode, set from Inspector.
    /// </summary>
    public GameMode CurrentMode => currentMode;

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
