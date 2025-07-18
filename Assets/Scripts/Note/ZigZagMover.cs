using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mueve un GameObject en zig-zag hasta el target y luego un tramo extra
/// en la misma dirección de llegada para la zona de “Good”.  
/// Dispara OnMovementComplete solo al finalizar todo el recorrido.
/// </summary>
public class ZigZagMover : MonoBehaviour
{
    [Header("ZigZag Settings")]
    [SerializeField] private float minDeviation = 1f;
    [SerializeField] private float maxDeviation = 2f;
    [SerializeField] private int minSegments = 2;
    [SerializeField] private int maxSegments = 5;

    [Header("Exit Zone")]
    [Tooltip("Distancia extra más allá del target, en la dirección de llegada.")]
    [SerializeField] private float exitDistance = 1f;

    private List<Vector3> path;
    private float speed;
    private int segmentIndex;

    /// <summary>Invocado tras completar todo el recorrido (incluida la salida).</summary>
    public event Action OnMovementComplete = delegate { };

    /// <summary>
    /// Inicializa la ruta y la velocidad.  
    /// travelTime es el tiempo para llegar hasta el target (sin contar la salida).
    /// </summary>
    public void Initialize(Vector3 start, Vector3 end, float travelTime)
    {
        // 1) Generar ruta zig-zag hasta el target
        path = new List<Vector3> { start };
        Vector3 dirToTarget = (end - start).normalized;
        int segments = UnityEngine.Random.Range(minSegments, maxSegments + 1);

        for (int i = 1; i < segments; i++)
        {
            float t = (float)i / segments;
            Vector3 pt = Vector3.Lerp(start, end, t);
            Vector3 perp = Vector3.Cross(dirToTarget, Vector3.forward).normalized;
            float dev = UnityEngine.Random.Range(minDeviation, maxDeviation) * (UnityEngine.Random.value > 0.5f ? 1 : -1);
            pt += perp * dev;
            path.Add(pt);
        }
        path.Add(end);

        // 2) Calcular velocidad en base a la distancia hasta el target
        float distToTarget = 0f;
        for (int i = 0; i < path.Count - 1; i++)
            distToTarget += Vector3.Distance(path[i], path[i + 1]);

        speed = distToTarget / Mathf.Max(travelTime, 0.01f);

        // 3) Obtener la dirección de llegada (último segmento)
        Vector3 fromPrevToTarget = (end - path[path.Count - 2]).normalized;

        // 4) Añadir punto de salida en esa misma dirección
        Vector3 exitPoint = end + fromPrevToTarget * exitDistance;
        path.Add(exitPoint);

        // 5) Preparar movimiento
        segmentIndex = 0;
        transform.position = path[0];
    }

    private void Update()
    {
        if (path == null || segmentIndex >= path.Count - 1)
            return;

        float step = speed * Time.deltaTime;
        Vector3 next = path[segmentIndex + 1];
        transform.position = Vector3.MoveTowards(transform.position, next, step);

        if (Vector3.Distance(transform.position, next) < 0.01f)
        {
            segmentIndex++;
            if (segmentIndex >= path.Count - 1)
            {
                OnMovementComplete.Invoke();
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        if (path == null || path.Count < 2) return;

        // Dibuja líneas conectando cada punto de la ruta
        Gizmos.color = Color.cyan;
        for (int i = 0; i < path.Count - 1; i++)
        {
            Gizmos.DrawLine(path[i], path[i + 1]);
        }

        // Dibuja un pequeño gizmo en cada punto
        for (int i = 0; i < path.Count; i++)
        {
            Gizmos.DrawSphere(path[i], 0.05f);
        }

        // Marca con un color distinto el punto de salida
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(path[path.Count - 1], 0.18f);
    }
}

