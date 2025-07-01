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

    private void Awake()
    {
        // Singleton simple
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        OnComboIncreased.AddListener(value => {
            FMODMusicConductor.Instance.SetComboLevel(value);
        });
        OnComboReset.AddListener(() => {
            FMODMusicConductor.Instance.SetComboLevel(0);
        });
    }

    /// <summary>
    /// Llamar cuando una nota ha sido evaluada con resultado Good o Perfect.
    /// Incrementa el combo y lanza OnComboIncreased.
    /// </summary>
    public void RegisterHit()
    {
        CurrentCombo++;
        OnComboIncreased.Invoke(CurrentCombo);
    }

    /// <summary>
    /// Llamar cuando una nota resulta en Miss.
    /// Reinicia el combo y lanza OnComboReset si no estaba ya en 0.
    /// </summary>
    public void RegisterMiss()
    {
        if (CurrentCombo > 0)
        {
            CurrentCombo = 0;
            OnComboReset.Invoke();
        }
    }
}