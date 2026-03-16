using UnityEngine;

/// <summary>
/// Comportamiento simplificado para las notas del tutorial.
/// Se mueve linealmente hacia un objetivo y permite congelarse.
/// </summary>
public class TutorialNoteBehavior : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer noteSprite;
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite rightSprite;

    [Header("Movement")]
    [SerializeField] private float speed = 5f;

    private Transform targetButton;
    private TargetZoneManager zoneManager; // Referencia para liberar el target
    private bool isFrozen = false;
    public NoteInputType InputType { get; private set; }

    /// <summary>
    /// Inicializa la nota con su tipo y objetivo.
    /// </summary>
    public void Initialize(NoteInputType type, Transform target, TargetZoneManager manager = null)
    {
        InputType = type;
        targetButton = target;
        zoneManager = manager; // Guardar referencia (puede ser null si no se pasa)

        // Configurar sprite según el tipo (Lógica copiada de NoteBehavior)
        if (type == NoteInputType.LB || type == NoteInputType.LT)
        {
            noteSprite.sprite = leftSprite;
        }
        else if (type == NoteInputType.RB || type == NoteInputType.RT)
        {
            noteSprite.sprite = rightSprite;
        }

        // Configurar visualmente el pulsador objetivo
        if (targetButton != null)
        {
            var playerButton = targetButton.GetComponent<PlayerButton>();
            if (playerButton != null)
            {
                playerButton.ShowTutorialState(type);
            }
        }
    }

    private void Update()
    {
        if (isFrozen || targetButton == null) return;

        // Moverse hacia el objetivo
        transform.position = Vector3.MoveTowards(transform.position, targetButton.position, speed * Time.deltaTime);
    }

    /// <summary>
    /// Detiene el movimiento de la nota.
    /// </summary>
    public void Freeze()
    {
        isFrozen = true;
    }

    /// <summary>
    /// Calcula la distancia (con signo) al objetivo en el eje X.
    /// Positivo = Antes de llegar. Negativo = Se pasó.
    /// </summary>
    public float GetDistanceToTarget()
    {
        if (targetButton == null) return float.MaxValue;
        return Vector3.Distance(transform.position, targetButton.position);
    }
    
    /// <summary>
    /// Devuelve el componente PlayerButton del objetivo, si existe.
    /// </summary>
    public PlayerButton GetTargetButton()
    {
        if (targetButton == null) return null;
        return targetButton.GetComponent<PlayerButton>();
    }

    private void OnDestroy()
    {
        // Limpiar el pulsador al destruir la nota
        if (targetButton != null)
        {
            var playerButton = targetButton.GetComponent<PlayerButton>();
            if (playerButton != null)
            {
                playerButton.ClearNote();
            }

            // Liberar el target en el manager para que pueda ser reusado
            if (zoneManager != null)
            {
                zoneManager.ReleaseTarget(targetButton);
            }
        }
    }
}
