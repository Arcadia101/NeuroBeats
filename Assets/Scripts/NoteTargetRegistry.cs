using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controla una cola global para que sólo una nota se evalúe a la vez.
/// </summary>
public class NoteTargetRegistry : MonoBehaviour
{
    public static NoteTargetRegistry Instance { get; private set; }

    private Queue<NoteBehavior> queue = new Queue<NoteBehavior>();
    private NoteBehavior current = null;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Encola una nota para evaluación. Si no hay ninguna activa, la lanza enseguida.
    /// </summary>
    public void EnqueueForEvaluation(NoteBehavior note)
    {
        queue.Enqueue(note);
        TryNext();
    }

    /// <summary>
    /// Llamar cuando la nota actual termina (hit o miss).
    /// </summary>
    public void FinishCurrentEvaluation()
    {
        current = null;
        TryNext();
    }
    
    public NoteBehavior CurrentlyEvaluating(NoteInputType type)
    {
        // Si usas la cola única, compara type con current.InputType
        return current != null && current.InputType == type
            ? current
            : null;
    }


    private void TryNext()
    {
        if (current != null) return;
        if (queue.Count == 0) return;
        current = queue.Dequeue();
        current.BeginEvaluation();
    }
}