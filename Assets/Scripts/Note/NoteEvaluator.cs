using UnityEngine;

/// <summary>
/// Componente que controla la evaluación de una nota en cuanto a timing e input.
/// Se suscribe a NoteTargetRegistry para manejar la cola de evaluación.
/// </summary>
public class NoteEvaluator : MonoBehaviour
{
    // Parámetros
    public float TargetTime { get; private set; }
    public NoteInputType TargetType { get; private set; }

    // Estados
    private bool isEvaluable = false;
    private bool hasEvaluated = false;
    private float evaluationStartTime;
    private Vector3 spawnPosition; // Para calcular congruencia

    // Referencias externas
    public PlayerButton assignedButton;
    private TargetZoneManager zoneManager;
    public string markerCombo;

    [Header("Evaluation Settings")]
    [SerializeField] private float perfectResultDelta = 0.18f;
    [SerializeField] private float goodResultDelta = 0.21f;

    /// <summary>
    /// Inicializa el evaluator con todos los datos necesarios.
    /// </summary>
    public void Initialize(
        PlayerButton button,
        TargetZoneManager zoneManager,
        float markerTime,
        float spawnTime,
        NoteInputType type, string markerName,
        Vector3 spawnPos)
    {
        this.assignedButton = button;
        this.zoneManager = zoneManager;
        this.TargetTime = markerTime;
        this.TargetType = type;
        this.spawnPosition = spawnPos;
        
        // Asignar al botón
        var behavior = GetComponent<NoteBehavior>();
        markerCombo = markerName;
        assignedButton.AssignNote(behavior, type , spawnTime);

        // Encolar la NoteBehavior para evaluación
        NoteTargetRegistry.Instance.EnqueueForEvaluation(behavior);

        isEvaluable = true;
    }

    /// <summary>
    /// Llamado por el registry cuando esta nota debe comenzar a evaluar input.
    /// </summary>
    public void BeginEvaluation()
    {
        evaluationStartTime = MusicTimeTracker.Instance.CurrentSongTime;
    }

    /// <summary>
    /// Recibe un intento de input del jugador y decide resultado.
    /// </summary>
    public void ReceiveInput(NoteInputType input)
    {
        if (!isEvaluable || hasEvaluated) return;

        float now = MusicTimeTracker.Instance.CurrentSongTime;
        float delta = now - TargetTime;
        float absDelta = Mathf.Abs(delta);
        
        string result;

        if (TargetType != input)
        {
            result = "Miss";
        }
        else
        {
            if (absDelta <= perfectResultDelta) 
                result = "Perfect";
            else if (absDelta <= goodResultDelta)
                result = "Good";
            else 
                result = "Miss";
        }

        // Feedback visual/sonoro
        if (result == "Perfect") assignedButton.ShowPerfectState();
        else if (result == "Good") assignedButton.ShowGoodState();
        else assignedButton.ShowMissState();
        
        // Llama al FeedbackManager para popup + SFX
        var feedbackType = result == "Perfect"
            ? FeedbackType.Perfect
            : result == "Good"
                ? FeedbackType.Good
                : FeedbackType.Miss;
        FeedbackManager.Instance.ShowFeedback(assignedButton.transform, feedbackType);

        // Combo
        if (result == "Perfect")
        {
            InfoController.Instance.RegisterPerfectHit();
        }
        else if (result == "Good")
        {
            InfoController.Instance.RegisterGoodHit();
        }
        else
        {
            InfoController.Instance.RegisterMiss();
        }

        // Calcular congruencia
        string congruencyVal = CalculateCongruency();

        // Registrar historial
        NoteHistoryRecorder.Instance.Record(new NoteResult
        {
            markerName = markerCombo,
            targetTime = TargetTime,
            hitTime = now,
            inputReceived = input.ToString(),
            result = result,
            evaluationDuration = now - evaluationStartTime,
            inputSpected = TargetType.ToString(),
            congruency = congruencyVal
        });

        FinishEvaluation();
    }

    private string CalculateCongruency()
    {
        if (assignedButton == null) return "Unknown";

        float targetX = assignedButton.transform.position.x;
        float spawnX = spawnPosition.x;

        // Definir un umbral pequeño para considerar "centro"
        float centerThreshold = 0.5f;

        // 1. Neutro: si termina en el centro
        if (Mathf.Abs(targetX) < centerThreshold)
        {
            return "Neutral";
        }

        // 2. Congruente: mismo signo (Ambos positivos o ambos negativos)
        bool sameSide = (spawnX > 0 && targetX > 0) || (spawnX < 0 && targetX < 0);
        
        return sameSide ? "Congruent" : "Incongruent";
    }

    /// <summary>
    /// Si llega al final del movimiento sin input, es Miss.
    /// </summary>
    public void OnMovementComplete()
    {
        if (!isEvaluable || hasEvaluated) return;
        // Reusar la lógica de fallo sin input
        ReceiveInput(TargetType); // pasamos mismo tipo para get "Miss"
    }

    private void FinishEvaluation()
    {
        hasEvaluated = true;
        isEvaluable = false;
        assignedButton.ClearNote();
        zoneManager.ReleaseTarget(assignedButton.transform);
        NoteTargetRegistry.Instance.FinishCurrentEvaluation();
        Destroy(gameObject);
    }
}
