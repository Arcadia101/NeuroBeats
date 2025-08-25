using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Singleton que maneja el final de nivel. Dispara un evento OnLevelEnd
/// en el que puedes suscribir tu UI de fin de nivel.
/// </summary>
public class LevelEndController : MonoBehaviour
{
    public static LevelEndController Instance { get; private set; }

    [Tooltip("Evento que se disparará cuando el nivel termine.")]
    public UnityEvent OnLevelEnd = new UnityEvent();

    private bool hasEnded = false;

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
    
    /// <summary>Resetea el controlador de fin de nivel.</summary>
    public void Reset()
    {
        hasEnded = false;
    }

    /// <summary>
    /// Llama a este método para indicar que el nivel ha terminado.
    /// </summary>
    public void TriggerEnd()
    {
        if (hasEnded) return;
        hasEnded = true;
        Debug.Log("LevelEndController: Nivel finalizado. Disparando evento OnLevelEnd.");
        OnLevelEnd.Invoke();
    }
}