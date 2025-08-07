using System.Collections.Generic;
using UnityEngine;

public class TargetZoneManager : MonoBehaviour
{
    [Tooltip("Todos los posibles pulsadores (PlayerButtons) en escena.")]
    [SerializeField] private List<Transform> targetPoints;

    // Mapa de disponibilidad
    private Dictionary<Transform, bool> targetAvailability = new Dictionary<Transform, bool>();

    void Awake()
    {
        targetAvailability.Clear();
        foreach (var point in targetPoints)
        {
            if (point != null && !targetAvailability.ContainsKey(point))
                targetAvailability[point] = true;
        }
    }

    /// <summary>
    /// Devuelve un pulsador aleatorio que esté libre y acepte el tipo dado.
    /// </summary>
    public Transform RequestRandomTarget(NoteInputType type)
    {
        var candidates = new List<Transform>();

        // 1) Recolecta todos los libres y compatibles
        foreach (var kvp in targetAvailability)
        {
            if (!kvp.Value) continue; // ya ocupado
            var t = kvp.Key;
            var pb = t.GetComponent<PlayerButton>();
            if (pb != null && pb.CanAcceptInput(type))
                candidates.Add(t);
        }

        // 2) Si no hay ninguno, warning y null
        if (candidates.Count == 0)
        {
            Debug.LogWarning($"RequestRandomTarget: no hay pulsadores libres que acepten {type}");
            return null;
        }

        // 3) Escoge uno al azar
        int idx = Random.Range(0, candidates.Count);
        var chosen = candidates[idx];

        // 4) Márcalo como ocupado y devuélvelo
        targetAvailability[chosen] = false;
        return chosen;
    }

    /// <summary>
    /// Libera el pulsador para que vuelva a estar disponible.
    /// </summary>
    public void ReleaseTarget(Transform target)
    {
        if (targetAvailability.ContainsKey(target))
            targetAvailability[target] = true;
        else
            Debug.LogWarning("ReleaseTarget: este Transform no estaba registrado.");
    }
}
