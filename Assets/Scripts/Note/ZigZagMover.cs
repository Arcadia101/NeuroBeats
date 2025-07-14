using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mueve un objeto en zig-zag desde un origen hasta un destino,
/// con un tramo final de evaluación y un tramo de salida (extensión).
/// </summary>
public class ZigZagMover : MonoBehaviour
{
    [Header("ZigZag Settings")]
    [SerializeField] private float minDeviation = 1f;
    [SerializeField] private float maxDeviation = 2f;
    [SerializeField] private int minSegments = 2;
    [SerializeField] private int maxSegments = 5;

    private List<Vector3> path;
    private int segmentIndex;

    private float speedPhase1;
    private float speedEval;   // velocidad del penúltimo tramo (evaluación)
    private float speedExit;   // velocidad del tramo de salida

    /// <summary>
    /// Inicializa la ruta y velocidades segmentadas.
    /// </summary>
    public void Initialize(
        Vector3 start,
        Vector3 markerPos,
        float totalTime,
        float fixedEvalTime,
        float fixedExitTime)
    {
        BuildPath(start, markerPos);

        // Calcular distancias
        float distTotal = ComputeDistance(path);
        int last = path.Count - 1;
        float distEval = Vector3.Distance(path[last - 1], path[last]);
        // Agregar punto de salida extendido
        Vector3 dir = (path[last] - path[last - 1]).normalized;
        Vector3 exitPoint = path[last] + dir * distEval; // misma longitud que tramo de evaluación
        path.Add(exitPoint);
        float distExit = Vector3.Distance(path[last], path[last + 1]);

        float distPhase1 = distTotal - distEval;

        // Tiempos
        float timePhase1 = Mathf.Max(0.01f, totalTime - fixedEvalTime - fixedExitTime);

        // Velocidades
        speedPhase1 = distPhase1 / timePhase1;
        speedEval   = distEval   / Mathf.Max(0.01f, fixedEvalTime);
        speedExit   = distExit   / Mathf.Max(0.01f, fixedExitTime);

        segmentIndex = 0;
        transform.position = path[0];
    }

    private void Update()
    {
        if (path == null || segmentIndex >= path.Count - 1)
            return;

        // Detectar inicio de la ventana de evaluación (cuando la nota llega al marcador)
        if (segmentIndex == path.Count - 2)
        {
            Debug.Log($"[Note] Evaluation window START at time {Time.time:F2}s");
        }
        // Detectar fin de la ventana de evaluación (cuando la nota sale del marcador)
        if (segmentIndex == path.Count - 1)
        {
            Debug.Log($"[Note] Evaluation window END at time {Time.time:F2}s");
        }

        float currentSpeed;
        if (segmentIndex < path.Count - 2)
        {
            // Fase inicial y zig-zag
            currentSpeed = speedPhase1;
        }
        else if (segmentIndex == path.Count - 2)
        {
            // Tramo de evaluación final (llegada al marcador)
            currentSpeed = speedEval;
        }
        else
        {
            // Tramo de salida tras evaluación
            currentSpeed = speedExit;
        }

        // Mover hacia el siguiente punto de la ruta
        Vector3 next = path[segmentIndex + 1];
        transform.position = Vector3.MoveTowards(
            transform.position,
            next,
            currentSpeed * Time.deltaTime);

        // Si llegó, avanzar segmento
        if (Vector3.Distance(transform.position, next) < 0.01f)
        {
            segmentIndex++;
            if (segmentIndex >= path.Count - 1)
            {
                OnMovementComplete?.Invoke();
            }
        }
    }

    public event System.Action OnMovementComplete;

    private void BuildPath(Vector3 start, Vector3 markerPos)
    {
        path = new List<Vector3> { start };
        Vector3 dir = (markerPos - start).normalized;
        int segments = Random.Range(minSegments, maxSegments + 1);
        for (int i = 1; i < segments; i++)
        {
            float t = (float)i / segments;
            Vector3 point = Vector3.Lerp(start, markerPos, t);
            Vector3 perp = Vector3.Cross(dir, Vector3.forward).normalized;
            float dev = Random.Range(minDeviation, maxDeviation) * (Random.value > 0.5f ? 1 : -1);
            point += perp * dev;
            path.Add(point);
        }
        path.Add(markerPos);
    }

    private float ComputeDistance(List<Vector3> pts)
    {
        float d = 0f;
        for (int i = 0; i < pts.Count - 1; i++)
            d += Vector3.Distance(pts[i], pts[i + 1]);
        return d;
    }

    private void OnDrawGizmos()
    {
        if (path == null || path.Count < 2) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < path.Count - 1; i++)
            Gizmos.DrawLine(path[i], path[i + 1]);
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(path[path.Count - 1], 0.08f);
    }
}

