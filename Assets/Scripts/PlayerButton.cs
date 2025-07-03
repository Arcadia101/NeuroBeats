using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Pulsador que muestra un círculo rotatorio y, encima, una letra estática.
/// Cada letra y círculo tiene 5 variantes de sprite: Left, Right, Good, Perfect y Miss.
/// </summary>
public class PlayerButton : MonoBehaviour
{
    [Header("Tipos de input aceptados")]
    [Tooltip("Tipos de input (NoteInputType) que este botón puede aceptar.")]
    [SerializeField] private List<NoteInputType> acceptedInputTypes = new();

    [Header("Círculo rotatorio")]
    [SerializeField] private Transform circleTransform;
    [SerializeField] private SpriteRenderer circleRenderer;
    [Tooltip("Sprite Left (LB/LT).")]
    [SerializeField] private Sprite circleLeftSprite;
    [Tooltip("Sprite Right (RB/RT).")]
    [SerializeField] private Sprite circleRightSprite;
    [Tooltip("Sprite Good.")]
    [SerializeField] private Sprite circleGoodSprite;
    [Tooltip("Sprite Perfect.")]
    [SerializeField] private Sprite circlePerfectSprite;
    [Tooltip("Sprite Miss.")]
    [SerializeField] private Sprite circleMissSprite;

    [Header("Letra estática")]
    [SerializeField] private SpriteRenderer letterRenderer;
    [Tooltip("Sprite 'B' Left.")]
    [SerializeField] private Sprite spriteBLeft;
    [Tooltip("Sprite 'T' Left.")]
    [SerializeField] private Sprite spriteTLeft;
    [Tooltip("Sprite 'B' Right.")]
    [SerializeField] private Sprite spriteBRight;
    [Tooltip("Sprite 'T' Right.")]
    [SerializeField] private Sprite spriteTRight;
    [Tooltip("Sprite 'B' Good.")]
    [SerializeField] private Sprite spriteBGood;
    [Tooltip("Sprite 'T' Good.")]
    [SerializeField] private Sprite spriteTGood;
    [Tooltip("Sprite 'B' Perfect.")]
    [SerializeField] private Sprite spriteBPerfect;
    [Tooltip("Sprite 'T' Perfect.")]
    [SerializeField] private Sprite spriteTPerfect;
    [Tooltip("Sprite 'B' Miss.")]
    [SerializeField] private Sprite spriteBMiss;
    [Tooltip("Sprite 'T' Miss.")]
    [SerializeField] private Sprite spriteTMiss;

    [Header("Rotación")]
    [Tooltip("Velocidad de rotación del círculo (grados por segundo).")]
    [SerializeField] private float rotationSpeed = 90f;

    private NoteBehavior currentNote;
    private NoteInputType currentType;

    /// <summary>
    /// Comprueba si este botón acepta el tipo de input dado.
    /// </summary>
    public bool CanAcceptInput(NoteInputType type)
        => acceptedInputTypes.Contains(type);
    
    
    [Header("Rotación Dinámica")]
        [Tooltip("Velocidad mínima de rotación (grados/s).")]
        [SerializeField] private float minRotationSpeed = 30f;
        [Tooltip("Velocidad máxima de rotación (grados/s).")]
        [SerializeField] private float maxRotationSpeed = 180f;
        [Tooltip("Tiempo (s) antes de TargetTime para empezar a acelerar.")]
        [SerializeField] private float accelDuration = 1.0f;
        [Tooltip("Tiempo (s) tras TargetTime para desacelerar.")]
        [SerializeField] private float decelDuration = 1.0f;
        
        
        private void Update()
        {
            if (currentNote == null) return;

            // 1) Obtén tiempos
            float now    = MusicTimeTracker.Instance.CurrentSongTime;
            float spawn  = currentNote.SpawnTime;
            float target = currentNote.TargetTime;

            // 2) Cálculo de aceleración lineal desde spawn→target
            float travelDur = Mathf.Max(target - spawn, 0.01f);
            float elapsed   = Mathf.Clamp(now - spawn, 0f, travelDur);
            float accelT    = elapsed / travelDur;
            float speedAccel= Mathf.Lerp(minRotationSpeed, maxRotationSpeed, accelT);

            float speed = speedAccel;

            // 3) Si ya pasamos el perfect, aplicamos desaceleración
            if (now > target)
            {
                float post    = now - target;
                float decelT  = Mathf.Clamp01(post / decelDuration);
                speed         = Mathf.Lerp(maxRotationSpeed, minRotationSpeed, decelT);
            }

            // 4) Rota el círculo
            circleTransform.Rotate(0f, 0f, speed * Time.deltaTime);

            // 5) Fija la letra sin rotación
            letterRenderer.transform.rotation = Quaternion.identity;
        }



    /// <summary>
    /// Asignado por la nota al instanciarse: muestra Left/Right variante normal.
    /// </summary>
    public void AssignNote(NoteBehavior note, NoteInputType type, float spawnTime)
    {
        currentNote = note;
        currentType = type;

        // Círculo Left/Right
        circleRenderer.sprite = (type == NoteInputType.LB || type == NoteInputType.LT)
            ? circleLeftSprite
            : circleRightSprite;
        circleTransform.gameObject.SetActive(true);

        // Letra Left/Right
        switch (type)
        {
            case NoteInputType.LB: letterRenderer.sprite = spriteBLeft; break;
            case NoteInputType.LT: letterRenderer.sprite = spriteTLeft; break;
            case NoteInputType.RB: letterRenderer.sprite = spriteBRight; break;
            case NoteInputType.RT: letterRenderer.sprite = spriteTRight; break;
        }
        letterRenderer.enabled = true;
    }

    /// <summary>
    /// Llamado al acertar “Good”: cambia círculo y letra a variante Good.
    /// </summary>
    public void ShowGoodState()
    {
        circleRenderer.sprite = circleGoodSprite;

        // Letra Good (B/T)
        bool isButton = (currentType == NoteInputType.LB || currentType == NoteInputType.RB);
        letterRenderer.sprite = isButton ? spriteBGood : spriteTGood;
        letterRenderer.enabled = true;
    }

    /// <summary>
    /// Llamado al acertar “Perfect”: cambia círculo y letra a variante Perfect.
    /// </summary>
    public void ShowPerfectState()
    {
        circleRenderer.sprite = circlePerfectSprite;

        // Letra Perfect (B/T)
        bool isButton = (currentType == NoteInputType.LB || currentType == NoteInputType.RB);
        letterRenderer.sprite = isButton ? spriteBPerfect : spriteTPerfect;
        letterRenderer.enabled = true;
    }

    /// <summary>
    /// Llamado al fallar “Miss”: cambia círculo y letra a variante Miss.
    /// </summary>
    public void ShowMissState()
    {
        circleRenderer.sprite = circleMissSprite;

        // Letra Miss (B/T)
        bool isButton = (currentType == NoteInputType.LB || currentType == NoteInputType.RB);
        letterRenderer.sprite = isButton ? spriteBMiss : spriteTMiss;
        letterRenderer.enabled = true;
    }

    /// <summary>
    /// Limpia la asignación: oculta círculo y letra.
    /// </summary>
    public void ClearNote()
    {
        currentNote = null;
        circleTransform.gameObject.SetActive(false);
        letterRenderer.enabled = false;
    }

    /// <summary>
    /// Recibe el input del jugador desde HitController.
    /// </summary>
    public void OnPlayerInput(NoteInputType input)
    {
        if (currentNote == null) return;
        currentNote.ReceiveInput(input);
    }

    #region Gizmos
    [Header("Visualización Debug (Gizmos)")]
    [Tooltip("Radio (en unidades de mundo) para la zona de Good (exitDistance).")]
    [SerializeField] private float goodRadius = 1f;
    [Tooltip("Radio (en unidades de mundo) para la zona de Perfect.")]
    [SerializeField] private float perfectRadius = 0.5f;
    
    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        // Zona Good (amarillo)
        Handles.color = new Color(1f, 1f, 0f, 0.5f);
        Handles.DrawWireDisc(transform.position, Vector3.forward, goodRadius);

        // Zona Perfect (cian)
        Handles.color = new Color(0f, 1f, 1f, 0.5f);
        Handles.DrawWireDisc(transform.position, Vector3.forward, perfectRadius);
#endif
    }
    #endregion
}
