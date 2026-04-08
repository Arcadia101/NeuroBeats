using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoUIController : MonoBehaviour
{
    [Tooltip("Referencia al panel raíz de la UI.")]
    [SerializeField] private Transform infoUI;

    [Tooltip("Referencia al Text que muestra el porcentaje de la canción transcurrido.")]
    [SerializeField] private TextMeshProUGUI progressPercent;

    [Tooltip("Referencia al Text que muestra el score.")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Tooltip("Referencia al Text que muestra el score que se suma al total.")]
    [SerializeField] private TextMeshProUGUI addScoreText;

    //[Tooltip("Referencia a la Imagen que muestra el combo.")]
    //[SerializeField] private Image comboImage;

    [Tooltip("Referencia al Text que muestra el número del combo.")]
    [SerializeField] private TextMeshProUGUI comboNumber;

    [Tooltip("Referencia al transform vacío que almacena el Bar y el Fill del combo.")]
    [SerializeField] private Transform comboBar;

    [SerializeField] private float fadeTime = 0.5f;
    [SerializeField] private float comboFadeTime = 1f;

    private Image comboBarFill;

    private void Awake()
    {
        comboBarFill = comboBar.GetChild(0).GetComponent<Image>();

        // Suscripciones a eventos de InfoController (ya las tenías)
        if (InfoController.Instance != null)
        {
            InfoController.Instance.OnComboChanged += ShowCombo;
            InfoController.Instance.OnComboVisibilityChanged += SetComboVisibility;
            InfoController.Instance.OnProgressChanged += UpdateProgress;
            InfoController.Instance.OnScoreChanged += UpdateScore;
            InfoController.Instance.OnAddScoreChanged += ShowAddScore;
            InfoController.Instance.OnMultiplierChanged += UpdateComboBar;
        }

        // Nueva suscripción: Copiado del formato de ResultUIController
        if (LevelEndController.Instance != null)
        {
            LevelEndController.Instance.OnLevelEnd.AddListener(HideInfoUI);
        }
        else
        {
            // Opcional: Log si no encuentra, como en ResultUIController
            // Debug.LogWarning("InfoUIController: LevelEndController not found.");
        }
    }

    private void Start()
    {
        infoUI.gameObject.SetActive(true);
        //comboImage.gameObject.SetActive(false);
        addScoreText.gameObject.SetActive(false);

        comboNumber.text = "x0";
        progressPercent.text = "0%";
        scoreText.text = "0";
        comboBarFill.fillAmount = 0f;
    }

    private void OnDestroy()
    {
        if (InfoController.Instance != null)
        {
            InfoController.Instance.OnComboChanged -= ShowCombo;
            InfoController.Instance.OnComboVisibilityChanged -= SetComboVisibility;
            InfoController.Instance.OnProgressChanged -= UpdateProgress;
            InfoController.Instance.OnScoreChanged -= UpdateScore;
            InfoController.Instance.OnAddScoreChanged -= ShowAddScore;
            InfoController.Instance.OnMultiplierChanged -= UpdateComboBar;
        }

        // Nueva desuscripción: Copiado del formato
        if (LevelEndController.Instance != null)
        {
            LevelEndController.Instance.OnLevelEnd.RemoveListener(HideInfoUI);
        }
    }

    public void HideInfoUI()
    {
        if (infoUI != null)
        {
            infoUI.gameObject.SetActive(false);
        }
    }

    private void ShowCombo(int combo)
    {
        comboNumber.gameObject.SetActive(true);
        comboNumber.text = "x" + combo;
        
        StartCoroutine(HideComboAfterDelay(comboFadeTime));
    }

    private void SetComboVisibility(bool visible)
    {
        //comboImage.gameObject.SetActive(visible);
        
    }

    private void UpdateComboBar(float multiplier)
    {
        
        float fillAmount = Mathf.InverseLerp(1f, 5f, multiplier);
        comboBarFill.fillAmount = fillAmount;
    }

    private void UpdateProgress(float progress)
    {
        progressPercent.text = Mathf.RoundToInt(progress * 100f) + "%";
    }

    private void UpdateScore(int score)
    {
        scoreText.text = score.ToString();
    }

    private void ShowAddScore(int addScore)
    {
        if (addScore > 0)
        {
            addScoreText.gameObject.SetActive(true);
            addScoreText.text = "+" + addScore;
            // Iniciar corrutina para ocultar después de tiempo designado en fadeTime
            StartCoroutine(HideAddScoreAfterDelay(fadeTime));
        }
        else
        {
            addScoreText.gameObject.SetActive(false);
        }
    }

    private System.Collections.IEnumerator HideAddScoreAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        addScoreText.gameObject.SetActive(false);
    }
    
    private System.Collections.IEnumerator HideComboAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        comboNumber.gameObject.SetActive(false);
    }
}
