using System;
using FMODUnity;
using UnityEngine;

/// <summary>
/// Gestiona combo, score total, multiplicador, progreso de canción y musicLevel.
/// Combo = aciertos consecutivos.
/// Score = puntuación total acumulada.
/// MusicLevel = nivel que controla las capas de audio.
/// </summary>
public class InfoController : MonoBehaviour
{
    public static InfoController Instance { get; private set; }

    public event Action<int> OnComboChanged;
    public event Action<bool> OnComboVisibilityChanged;
    public event Action<int> OnScoreChanged;
    public event Action<int> OnAddScoreChanged;
    public event Action<float> OnProgressChanged;
    public event Action<float> OnMultiplierChanged;
    public event Action<int> OnMusicLevelChanged;
    public event Action OnComboReset;
    public event Action OnMissHit;

    [Header("Score")]
    [SerializeField] private int defaultGoodScore = 100;
    [SerializeField] private int defaultPerfectScore = 200;

    [Header("Combo")]
    [SerializeField] private int maxMisses = 1;

    [Header("Music Level")]
    [Tooltip("Cantidad de aciertos consecutivos necesarios para subir 1 nivel.")]
    [SerializeField] private int hitsPerLevel = 3;

    [Tooltip("Nivel máximo de música/multiplicador.")]
    [SerializeField] private int maxMusicLevel = 5;

    [Header("Audio")]
    [Tooltip("Tiempo en el que se rampea el valor de MusicLevel.")]
    [SerializeField] private float fader = 1.5f;

    public int CurrentCombo { get; private set; } = 0;
    public int CurrentScore { get; private set; } = 0;
    public int CurrentMisses { get; private set; } = 0;
    public int LastAddScore { get; private set; } = 0;
    public float CurrentProgress { get; private set; } = 0f;
    public float CurrentMultiplier { get; private set; } = 1f;
    public int CurrentMusicLevel { get; private set; } = 0;

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
        ResetState();
    }

    public void RegisterHit(int baseScorePerHit)
    {
        CurrentMisses = 0;

        if (baseScorePerHit == defaultPerfectScore)
        {
            CurrentCombo++;
            UpdateLevelFromCombo();
        }

        LastAddScore = CalculateAddScore(baseScorePerHit);
        CurrentScore += LastAddScore;

        ApplyMusicLevel(CurrentMusicLevel);

        OnComboChanged?.Invoke(CurrentCombo);
        OnComboVisibilityChanged?.Invoke(true);
        OnMultiplierChanged?.Invoke(CurrentMultiplier);
        OnAddScoreChanged?.Invoke(LastAddScore);
        OnScoreChanged?.Invoke(CurrentScore);
        OnMusicLevelChanged?.Invoke(CurrentMusicLevel);
    }

    public void RegisterGoodHit()
    {
        RegisterHit(defaultGoodScore);
    }

    public void RegisterPerfectHit()
    {
        RegisterHit(defaultPerfectScore);
    }

    public void RegisterMiss()
    {
        CurrentMisses++;

        if (CurrentMisses >= maxMisses)
        {
            ReduceComboLevel();
        }

        OnMissHit?.Invoke();
    }

    public void UpdateProgress(float progress01)
    {
        CurrentProgress = Mathf.Clamp01(progress01);
        OnProgressChanged?.Invoke(CurrentProgress);
    }

    public void ResetState()
    {
        CurrentCombo = 0;
        CurrentScore = 0;
        CurrentMisses = 0;
        LastAddScore = 0;
        CurrentProgress = 0f;
        CurrentMultiplier = 1f;
        CurrentMusicLevel = 0;

        OnComboReset?.Invoke();
        OnComboChanged?.Invoke(CurrentCombo);
        OnComboVisibilityChanged?.Invoke(false);
        OnScoreChanged?.Invoke(CurrentScore);
        OnAddScoreChanged?.Invoke(LastAddScore);
        OnProgressChanged?.Invoke(CurrentProgress);
        OnMultiplierChanged?.Invoke(CurrentMultiplier);
        OnMusicLevelChanged?.Invoke(CurrentMusicLevel);

        ApplyMusicLevel(CurrentMusicLevel);
    }

    private void UpdateLevelFromCombo()
    {
        int levelFromCombo = CurrentCombo / hitsPerLevel;
        CurrentMusicLevel = Mathf.Clamp(levelFromCombo, 0, maxMusicLevel);
        CurrentMultiplier = 1f + CurrentMusicLevel;
    }

    private void ApplyMusicLevel(int level)
    {
        if (FMODMusicConductor.Instance != null)
        {
            FMODMusicConductor.Instance.RampParameter("ComboLevel", level, fader);
        }
    }

    private void ReduceComboLevel()
    {
        CurrentCombo = 0;

        if (CurrentMusicLevel > 0)
            CurrentMusicLevel--;

        CurrentMultiplier = Mathf.Max(1f, 1f + CurrentMusicLevel);

        ApplyMusicLevel(CurrentMusicLevel);

        OnComboChanged?.Invoke(CurrentCombo);
        OnComboVisibilityChanged?.Invoke(false);
        OnMultiplierChanged?.Invoke(CurrentMultiplier);
        OnMusicLevelChanged?.Invoke(CurrentMusicLevel);
    }

    private int CalculateAddScore(int baseScorePerHit)
    {
        return Mathf.RoundToInt(baseScorePerHit * CurrentMultiplier);
    }

    private float CalculateMultiplier(int baseScorePerHit)
    {
        return Mathf.RoundToInt(baseScorePerHit * CurrentMultiplier);
    }
}