using FMODUnity;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Gestiona la racha de aciertos (combo) y emite eventos cuando cambia o se reinicia.
/// </summary>
public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance { get; private set; }

    [Header("Eventos de Combo")]
    /// <summary>Se invoca con el nuevo valor de combo cuando aumenta.</summary>
    public UnityEvent<int> OnComboIncreased = new UnityEvent<int>();
    /// <summary>Se invoca cuando el combo se reinicia a cero.</summary>
    public UnityEvent OnComboReset = new UnityEvent();

    /// <summary>Racha actual de aciertos.</summary>
    public int CurrentCombo { get; private set; } = 0;

    [Header("Combo thresholds")]
    [Tooltip("Puntajes necesarios para cada combo extra (Combo2→Combo9).")]
    [SerializeField] private int[] comboThresholds = new int[9];
    
    [Header("Fader")]
    [Tooltip("Tiempo en el que se rampea el valor de ComboLevel.")]
    [SerializeField] private float fader = 1.5f;
    

    public UnityEvent<int> OnComboLevelChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Llamar cuando una nota ha sido evaluada con resultado Good o Perfect.
    /// Incrementa el combo y lanza OnComboIncreased.
    /// </summary>
    public void RegisterHit()
    {
        CurrentCombo++;
        SetComboLevel(CurrentCombo);
    }
    
    /// <summary>
    /// Ajusta CurrentCombo según thresholds y notifica a FMODMusicConductor.
    /// </summary>
    public void SetComboLevel(int combo)
    {
        int level = Mathf.Clamp(combo, 0, comboThresholds.Length - 1);
        
        // Cambia el nivel de Combo en Fmod para que suenen nuevas pistas agregadas.
        FMODMusicConductor.Instance.RampParameter("ComboLevel", level, fader);

        OnComboLevelChanged?.Invoke(level);
    }

    /// <summary>
    /// Llamar cuando una nota resulta en Miss.
    /// Reinicia el combo y lanza OnComboReset si no estaba ya en 0.
    /// </summary>
    public void RegisterMiss()
    {
        if (CurrentCombo > 0)
        {
            Reset();
        }
    }
    
    /// <summary>Resetea el combo a cero.</summary>
    public void Reset()
    {
        CurrentCombo =- CurrentCombo ;
        // Opcional: notificar UI
        OnComboReset.Invoke();
    }
}