using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

/// <summary>
/// Controla la visualización del combo en pantalla:
/// muestra el texto con animación de aparición/desaparición.
/// </summary>
public class ComboUIController : MonoBehaviour
{
    [Tooltip("Referencia al Text que muestra el combo.")]
    [SerializeField] private TextMeshProUGUI comboText;

    [Tooltip("Tiempo que el combo permanece visible tras cada aumento.")]
    [SerializeField] private float visibleDuration = 1.0f;

    [Tooltip("Tiempo de fade-out al ocultar.")]
    [SerializeField] private float fadeDuration = 0.3f;

    private Coroutine hideRoutine;

    private void Awake()
    {
        // Suscribirse a eventos de ComboManager
        ComboManager.Instance.OnComboIncreased.AddListener(HandleComboIncrease);
        ComboManager.Instance.OnComboReset.AddListener(HandleComboReset);

        comboText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Cuando el combo aumenta, actualiza el texto y (re)activa el indicador.
    /// </summary>
    private void HandleComboIncrease(int newCombo)
    {
        // Si era una rutina de ocultar en curso, la detenemos
        if (hideRoutine != null) StopCoroutine(hideRoutine);

        comboText.text = $"COMBO x{newCombo}";
        comboText.color = Color.white;
        comboText.gameObject.SetActive(true);
    }

    /// <summary>
    /// Cuando el combo se reinicia, hace un fade rápidamente y desactiva el texto.
    /// </summary>
    private void HandleComboReset()
    {
        if (hideRoutine != null) StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(FadeAndHide());
    }

    /// <summary>
    /// Fade-out y ocultación tras un retardo.
    /// </summary>
    private IEnumerator FadeAndHide()
    {
        // Espera visible
        yield return new WaitForSeconds(visibleDuration);

        // Fade out
        float elapsed = 0f;
        Color original = comboText.color;
        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            comboText.color = Color.Lerp(original, new Color(original.r, original.g, original.b, 0), t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        comboText.gameObject.SetActive(false);
        comboText.color = original; // reset alpha
    }
}
