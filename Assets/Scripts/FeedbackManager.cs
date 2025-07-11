using UnityEngine;
using System.Collections;
using FMODUnity;
using FMOD.Studio;

/// <summary>
/// Singleton que muestra feedback visual (sprites) y reproduce SFX.
/// </summary>
public class FeedbackManager : MonoBehaviour
{
    public static FeedbackManager Instance { get; private set; }

    [Header("Prefabs & Sprites")]
    [Tooltip("Prefab que contiene un SpriteRenderer + CanvasGroup.")]
    [SerializeField] private GameObject feedbackPopupPrefab;
    [Tooltip("Sprite para Perfect.")]
    [SerializeField] private Sprite perfectSprite;
    [Tooltip("Sprite para Good.")]
    [SerializeField] private Sprite goodSprite;
    [Tooltip("Sprite para Miss.")]
    [SerializeField] private Sprite missSprite;

    [Header("FMOD Events")]
    [SerializeField] private EventReference perfectSfx;
    [SerializeField] private EventReference goodSfx;
    [SerializeField] private EventReference missSfx;

    [Header("Timings")]
    [SerializeField] private float displayDuration = 0.5f;
    [SerializeField] private float fadeDuration = 0.2f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Muestra el feedback (sprite + SFX) sobre la posición world del targetTransform.
    /// </summary>
    public void ShowFeedback(Transform targetTransform, FeedbackType type)
    {
        EventReference sfxToPlay = type == FeedbackType.Perfect
            ? perfectSfx
            : type == FeedbackType.Good
                ? goodSfx
                : missSfx;

        if (string.IsNullOrEmpty(sfxToPlay.Path))
        {
            Debug.LogWarning($"FeedbackManager: SFX not assigned for {type}");
        }
        else
        {
            RuntimeManager.PlayOneShot(sfxToPlay, targetTransform.position);
        }
        // 1) Instantiate popup
        var go = Instantiate(feedbackPopupPrefab, targetTransform.position, Quaternion.identity);
        var sr = go.GetComponent<SpriteRenderer>();
        var cg = go.GetComponent<CanvasGroup>();

        // 2) Assign sprite
        switch (type)
        {
            case FeedbackType.Perfect:
                sr.sprite = perfectSprite;
                break;
            case FeedbackType.Good:
                sr.sprite = goodSprite;
                break;
            case FeedbackType.Miss:
                sr.sprite = missSprite;
                break;
        }

        // 3) Start fade coroutine
        StartCoroutine(FadeAndDestroy(cg));
    }

    private IEnumerator FadeAndDestroy(CanvasGroup cg)
    {
        // Full visible
        cg.alpha = 1f;
        yield return new WaitForSeconds(displayDuration);

        // Fade-out
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            cg.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(cg.gameObject);
    }
}

/// <summary>
/// Tipos de feedback a mostrar.
/// </summary>
public enum FeedbackType
{
    Perfect,
    Good,
    Miss
}
