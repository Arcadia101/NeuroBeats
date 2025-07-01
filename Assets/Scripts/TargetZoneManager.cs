using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona las zonas de pulsación (PlayerButtons). Asigna zonas disponibles por tipo
/// y permite liberar zonas usadas para futuras notas.
/// </summary>
public class TargetZoneManager : MonoBehaviour
{
    [System.Serializable]
    public struct TypedTarget
    {
        public NoteInputType type;
        public Transform targetTransform;
    }

    [Header("Configuración de zonas (PlayerButtons)")]
    [Tooltip("Zonas de destino clasificadas por tipo (ej. LB, RB, etc.)")]
    [SerializeField] private List<TypedTarget> typedTargets;

    // Control de disponibilidad individual
    private Dictionary<Transform, bool> targetAvailability = new();

    // Mapa de tipos a lista de targets disponibles
    private Dictionary<NoteInputType, List<Transform>> targetTypeMap = new();

    private void Awake()
    {
        // Inicializa el sistema de zonas disponibles
        foreach (var entry in typedTargets)
        {
            var transform = entry.targetTransform;
            var type = entry.type;

            if (!targetAvailability.ContainsKey(transform))
            {
                targetAvailability.Add(transform, true); // Disponible al inicio
            }

            if (!targetTypeMap.ContainsKey(type))
            {
                targetTypeMap[type] = new List<Transform>();
            }

            targetTypeMap[type].Add(transform);
        }
    }

    /// <summary>
    /// Solicita una zona de un tipo específico (ej. LB, RB, etc.). Devuelve null si ninguna está libre.
    /// </summary>
    public Transform RequestTarget(NoteInputType type)
    {
        if (!targetTypeMap.ContainsKey(type)) return null;

        foreach (var target in targetTypeMap[type])
        {
            if (targetAvailability.ContainsKey(target) && targetAvailability[target])
            {
                targetAvailability[target] = false;
                return target;
            }
        }

        return null; // Todas ocupadas
    }

    /// <summary>
    /// Libera un target para que pueda ser reutilizado.
    /// </summary>
    public void ReleaseTarget(Transform target)
    {
        if (target != null && targetAvailability.ContainsKey(target))
        {
            targetAvailability[target] = true;
        }
        else
        {
            Debug.LogWarning("Tried to release a target that wasn't registered or is null.");
        }
    }

    /// <summary>
    /// Devuelve cuántos targets están disponibles en total (útil para debug).
    /// </summary>
    public int CountAvailableTargets()
    {
        int count = 0;
        foreach (var available in targetAvailability.Values)
        {
            if (available) count++;
        }
        return count;
    }
}
