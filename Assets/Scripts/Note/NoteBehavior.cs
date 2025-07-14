using UnityEngine;

/// <summary>
/// Contenedor de una nota: agrega ZigZagMover y NoteEvaluator.
/// MarkerDrivenSpawner inyecta datos y lanza Initialize.
/// </summary>
[RequireComponent(typeof(ZigZagMover))]
[RequireComponent(typeof(NoteEvaluator))]
public class NoteBehavior : MonoBehaviour
{
    private ZigZagMover mover;
    private NoteEvaluator evaluator;
    
    [Header("Evaluation Zones")]
    [Tooltip("Radio de zona Perfect.")]
    [SerializeField] private float perfectZoneRadius = 0.5f;
    [Tooltip("Radio de zona Good.")]
    [SerializeField] private float goodZoneRadius = 1.0f;
    public float SpawnTime { get; private set; }
    public float TargetTime { get; private set; }
    
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer noteSprite;
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite rightSprite;
    
    [Header("Evaluation timings")]
    [SerializeField] private float fixedEvalTime   = 0.5f; // segundos de evaluación (Good/Perfect)
    [SerializeField] private float fixedExitTime   = 0.3f; // segundos de salida tras evaluación
    

    private void Awake()
    {
        mover = GetComponent<ZigZagMover>();
        evaluator = GetComponent<NoteEvaluator>();
        mover.OnMovementComplete += evaluator.OnMovementComplete;
    }

    /// <summary>
    /// Inicializa ambos componentes con los parámetros necesarios.
    /// </summary>
    public void Initialize(PlayerButton button, TargetZoneManager zoneManager,
        float markerTime, float spawnTime, NoteInputType type)
    {
        this.SpawnTime = spawnTime;
        this.TargetTime = markerTime;
        
        
        // 1) Iniciar movimiento
        mover.Initialize(transform.position, button.transform.position, markerTime - spawnTime, fixedEvalTime, fixedExitTime);
        
        // Selección del sprite visual según lado
        if (type == NoteInputType.LB || type == NoteInputType.LT)
        {
            noteSprite.sprite = leftSprite;
        }
        else if (type == NoteInputType.RB || type == NoteInputType.RT)
        {
            noteSprite.sprite = rightSprite;
        }

        // 2) Iniciar evaluación
        evaluator.Initialize(button, zoneManager, markerTime, spawnTime, type);
    }
    //--- FACHADA hacia NoteEvaluator ---//
    public NoteInputType InputType 
    { 
        get { return evaluator.TargetType; } 
    }

    /// <summary>
    /// Invocado por HitController o PlayerButton
    /// </summary>
    public void ReceiveInput(NoteInputType input)
    {
        evaluator.ReceiveInput(input);
    }

    /// <summary>
    /// Invocado por NoteTargetRegistry
    /// </summary>
    public void BeginEvaluation()
    {
        evaluator.BeginEvaluation();
    }
    
    /// <summary>
    /// Dibuja en el editor las zonas de evaluación Good y Perfect alrededor del pulsador.
    /// </summary>
    private void OnDrawGizmos()
    {
        // Nos aseguramos de que evaluator y su transform del pulsador existen
        if (evaluator == null || evaluator.assignedButton.transform == null) 
            return;

        Vector3 pos = evaluator.assignedButton.transform.position;

        // Zona Perfect (verde)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pos, perfectZoneRadius);

        // Zona Good (amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(pos, goodZoneRadius);
    }

}



/// <summary>
/// Datos de spawn de una nota, usados si necesitas representar o guardar información.
/// </summary>
public class NoteSpawnData
{
    public Vector3 targetPosition;
    public float arrivalTime;
    public string markerName;

    public NoteSpawnData(Vector3 targetPosition, float arrivalTime, string markerName)
    {
        this.targetPosition = targetPosition;
        this.arrivalTime = arrivalTime;
        this.markerName = markerName;
    }
}
