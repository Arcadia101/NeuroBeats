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
    /// <summary>Se invoca con el nuevo valor de combo y score cuando aumenta.</summary>
    public UnityEvent<int,int> OnComboLevelChanged;
    /// <summary>Se invoca cuando el combo y score se reinicia a cero.</summary>
    public UnityEvent OnComboReset = new UnityEvent();
    public UnityEvent OnMissHit = new UnityEvent();

    /// <summary>Racha actual de aciertos.</summary>
    public int CurrentCombo { get; private set; } = 0;
    public int Currentscore { get; private set; } = 0;
    
    public int Currentmiss { get; private set; } = 0;

    [Header("Combo thresholds")]
    [Tooltip("Puntajes necesarios para cada combo extra (Combo2→Combo9).")]
    [SerializeField] private int[] comboThresholds = new int[9];

    [SerializeField] private int maxMisses = 3;
    
    
    [Header("Fader")]
    [Tooltip("Tiempo en el que se rampea el valor de ComboLevel.")]
    [SerializeField] private float fader = 1.5f;
    

    

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
        Currentscore++;
		Currentmiss = 0; // Reiniciamos el contador de fallos cuando hay un acierto
        UpdateComboLevel();
        SetComboLevel(Currentscore, CurrentCombo);
    }

    private void UpdateComboLevel()
    {
        int newCombo = 0; // Empezamos desde 0
        for (int i = 0; i < comboThresholds.Length; i++)
        {
            if (Currentscore >= comboThresholds[i])
            {
                newCombo = i + 1;
            }
            else
            {
                break;
            }
        }
        // Solo actualizamos si el nuevo combo es mayor que el actual

        if (newCombo > CurrentCombo)
        {
            CurrentCombo = Mathf.Clamp(newCombo, 0, comboThresholds.Length);
        }

    }

    /// <summary>
    /// Ajusta CurrentCombo según thresholds y notifica a FMODMusicConductor.
    /// </summary>
    public void SetComboLevel(int score ,int combo)
    {
        int level = Mathf.Clamp(combo, 0, comboThresholds.Length - 1);
        
        // Cambia el nivel de Combo en Fmod para que suenen nuevas pistas agregadas.
        FMODMusicConductor.Instance.RampParameter("ComboLevel", level, fader);

        OnComboLevelChanged?.Invoke(score ,level);
    }

    /// <summary>
    /// Llamar cuando una nota resulta en Miss.
    /// Reinicia el combo y lanza OnComboReset si no estaba ya en 0.
    /// </summary>
    public void RegisterMiss()
    {
        Currentmiss++;
        if (Currentmiss >= maxMisses)
        {
            ReduceComboLevel();
            Currentmiss = 0;
        }
        OnMissHit.Invoke();      
    }
    
	private void ReduceComboLevel()
    {
        // Si estamos en el nivel más bajo, no hacemos nada
        if (CurrentCombo <= 0) return;

        // Reducimos un nivel de combo
        CurrentCombo--;

		// Reiniciamos el score a 0
		Currentscore = 0;
		
		// Notificamos el cambio
        SetComboLevel(Currentscore, CurrentCombo);

	}

    /// <summary>Resetea el combo a cero.</summary>
    public void Reset()
    {
        CurrentCombo = 0;
        Currentscore = 0;
        Currentmiss = 0;
        // Opcional: notificar UI
        OnComboReset.Invoke();
    }
}