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

    // Referencias externas
    private PlayerButton assignedButton;
    private TargetZoneManager zoneManager;

    /// <summary>
    /// Inicializa el evaluator con todos los datos necesarios.
    /// </summary>
    public void Initialize(
        PlayerButton button,
        TargetZoneManager zoneManager,
        float markerTime,
        float spawnTime,
        NoteInputType type)
    {
        this.assignedButton = button;
        this.zoneManager = zoneManager;
        this.TargetTime = markerTime;
        this.TargetType = type;
        // Asignar al botón (pasa NoteBehavior, no NoteEvaluator)
        var behavior = GetComponent<NoteBehavior>();
        assignedButton.AssignNote(behavior, type , spawnTime);

        // Encolar la NoteBehavior para evaluación (no 'this')
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
        if (absDelta <= 0.1f) result = "Perfect";
        else if (absDelta <= 0.25f) result = "Good";
        else result = "Miss";

        bool correctInput = input == TargetType;
        if (!correctInput && result != "Miss") result = "Good";

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
        if (result == "Perfect" || result == "Good")
            ComboManager.Instance.RegisterHit();
        else
            ComboManager.Instance.RegisterMiss();

        // Registrar historial
        NoteHistoryRecorder.Instance.Record(new NoteResult
        {
            markerName = gameObject.name,
            targetTime = TargetTime,
            hitTime = now,
            inputReceived = input.ToString(),
            result = result,
            evaluationDuration = now - evaluationStartTime
        });

        FinishEvaluation();
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

