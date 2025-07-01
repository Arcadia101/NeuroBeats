using System.Collections.Generic;
using UnityEngine;

public class NoteBehavior : MonoBehaviour
{
    public float TargetTime { get; private set; }
    public NoteInputType InputType { get; private set; }
    private bool isEvaluable = false;
    private bool hasEvaluated = false;
    private string noteSide;

    private List<Vector3> path;
    private float speed;
    private int segmentIndex;

    private PlayerButton assignedButton;
    private TargetZoneManager zoneManager;
    private float evaluationStartTime;

    public void Initialize(
        PlayerButton button,
        TargetZoneManager zoneManager,
        float markerTime,
        float spawnTime,
        NoteInputType type,
        string side)
    {
        assignedButton = button;
        this.zoneManager = zoneManager;
        TargetTime = markerTime;
        InputType = type;
        noteSide = side;

        // Asigna al botón y encola para evaluación
        assignedButton.AssignNote(this, type);
        NoteTargetRegistry.Instance.EnqueueForEvaluation(this);

        BuildPath(transform.position, assignedButton.transform.position);
        float dist = ComputeDistance(path);
        speed = dist / Mathf.Max(0.01f, markerTime - spawnTime);
        segmentIndex = 0;
        transform.position = path[0];
        isEvaluable = true;
    }

    private void Update()
    {
        if (!isEvaluable || segmentIndex >= path.Count - 1) return;
        float step = speed * Time.deltaTime;
        Vector3 next = path[segmentIndex + 1];
        transform.position = Vector3.MoveTowards(transform.position, next, step);

        if (Vector3.Distance(transform.position, next) < 0.01f)
        {
            segmentIndex++;
            if (segmentIndex >= path.Count - 1 && !hasEvaluated)
                HandleMiss();
        }
    }

    public void BeginEvaluation()
    {
        evaluationStartTime = MusicTimeTracker.Instance.CurrentSongTime;
    }

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

        bool correctInput = (input == InputType);
        if (!correctInput && result != "Miss") result = "Good";

        // Guarda resultado
        NoteHistoryRecorder.Instance.Record(new NoteResult {
            markerName = gameObject.name,
            targetTime = TargetTime,
            hitTime = now,
            inputReceived = input.ToString(),
            result = result,
            evaluationDuration = now - evaluationStartTime,
            side = noteSide
        });
        
        // Mostrar resultado en feedback visual sobre la pulsacion del jugador
        FeedbackManager.Instance.ShowFeedback(assignedButton.transform,
            result == "Perfect" ? FeedbackType.Perfect :
            result == "Good"    ? FeedbackType.Good :
            FeedbackType.Miss);

        switch (result)
        {
            case "Perfect": assignedButton.ShowPerfectState(); break;
            case "Good": assignedButton.ShowGoodState(); break;
            case "Miss": assignedButton.ShowMissState(); break;
            default: break;
        }
        
        if (result == "Perfect" || result == "Good")
            ComboManager.Instance.RegisterHit();
        else
            ComboManager.Instance.RegisterMiss();
        
        FinishEvaluation();
    }

    private void HandleMiss()
    {
        if (!isEvaluable || hasEvaluated) return;
        float now = MusicTimeTracker.Instance.CurrentSongTime;
        NoteHistoryRecorder.Instance.Record(new NoteResult {
            markerName = gameObject.name,
            targetTime = TargetTime,
            hitTime = now,
            inputReceived = "None",
            result = "Miss",
            evaluationDuration = now - evaluationStartTime
        });
        
        FeedbackManager.Instance.ShowFeedback(assignedButton.transform, FeedbackType.Miss);
        assignedButton.ShowMissState();
        ComboManager.Instance.RegisterMiss();

        FinishEvaluation();
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

    private void BuildPath(Vector3 start, Vector3 end)
    {
        path = new List<Vector3> { start };
        Vector3 dir = (end - start).normalized;
        int seg = Random.Range(2, 6);
        for (int i = 1; i < seg; i++)
        {
            float t = (float)i / seg;
            Vector3 pt = Vector3.Lerp(start, end, t);
            Vector3 perp = Vector3.Cross(dir, Vector3.forward).normalized;
            pt += perp * Random.Range(1f, 2f) * (Random.value > 0.5f ? 1 : -1);
            path.Add(pt);
        }
        path.Add(end);
    }

    private float ComputeDistance(List<Vector3> pts)
    {
        float d = 0;
        for (int i = 0; i < pts.Count - 1; i++)
            d += Vector3.Distance(pts[i], pts[i + 1]);
        return d;
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
