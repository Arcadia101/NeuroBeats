using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla la secuencia del tutorial: Fase Guiada (congelada) y Fase Práctica (tiempo real).
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject tutorialNotePrefab;
    [SerializeField] private TargetZoneManager targetZoneManager;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private TextMeshProUGUI feedbackText; 
    
    [Tooltip("Arrastra aquí el objeto con el CanvasGroup del panel de resultados.")]
    [SerializeField] private CanvasGroup resultsPanel;
    
    [Tooltip("Arrastra aquí el objeto con el CanvasGroup del panel de pausa.")]
    [SerializeField] private CanvasGroup pausePanel;

    [Header("UI Instructions (Text Objects)")]
    [SerializeField] private GameObject textWelcome;
    [SerializeField] private GameObject textRightInstruction;
    [SerializeField] private GameObject textLeftInstruction;
    [SerializeField] private GameObject textPracticeIntro;

    [Header("UI Instructions (Images)")]
    [SerializeField] private GameObject gamepadImage; // El objeto padre o la imagen base
    [SerializeField] private Image highlightRB;
    [SerializeField] private Image highlightRT;
    [SerializeField] private Image highlightLB;
    [SerializeField] private Image highlightLT;

    [Header("Settings")]
    [SerializeField] private float perfectDistance = 0.5f;
    [SerializeField] private float goodDistance = 1.5f;
    [SerializeField] private float missDistance = 3.0f;
    [SerializeField] private float spawnDelay = 1.0f;
    
    [Header("Practice Phase")]
    [SerializeField] private int practiceNoteCount = 5;
    [SerializeField] private float practiceSpawnInterval = 2.0f;

    private TutorialNoteBehavior currentNote;
    private bool waitingForInput = false;
    private bool isPracticePhase = false; 
    private bool isPaused = false;
    
    // Referencia privada para la imagen base del mando
    private Image gamepadBaseGraphic;

    private void Start()
    {
        // Inicializar UI
        if (resultsPanel != null)
        {
            resultsPanel.alpha = 0;
            resultsPanel.interactable = false;
            resultsPanel.blocksRaycasts = false;
            resultsPanel.gameObject.SetActive(true); 
        }
        
        if (pausePanel != null)
        {
            pausePanel.alpha = 0;
            pausePanel.interactable = false;
            pausePanel.blocksRaycasts = false;
            pausePanel.gameObject.SetActive(true);
        }
        
        // Obtener la imagen base del mando
        if (gamepadImage != null)
        {
            gamepadBaseGraphic = gamepadImage.GetComponent<Image>();
            if (gamepadBaseGraphic == null)
            {
                Debug.LogWarning("TutorialManager: gamepadImage no tiene componente Image. El fade del mando podría no funcionar correctamente.");
            }
        }

        // Inicializar textos e imágenes: ACTIVOS pero TRANSPARENTES
        InitAlpha(textWelcome);
        InitAlpha(textRightInstruction);
        InitAlpha(textLeftInstruction);
        InitAlpha(textPracticeIntro);
        
        InitAlpha(highlightRB);
        InitAlpha(highlightRT);
        InitAlpha(highlightLB);
        InitAlpha(highlightLT);

        // Inicializar solo la base del mando
        if (gamepadBaseGraphic != null)
        {
            gamepadImage.SetActive(true);
            SetAlpha(gamepadBaseGraphic, 0f);
        }

        Debug.Log("[TutorialManager] Iniciando secuencia...");
        StartCoroutine(TutorialSequence());
    }

    private void InitAlpha(GameObject obj)
    {
        if (obj == null) return;
        obj.SetActive(true);
        var graphics = obj.GetComponentsInChildren<Graphic>();
        foreach (var g in graphics)
        {
            SetAlpha(g, 0f);
        }
    }
    
    private void InitAlpha(Image img)
    {
        if (img == null) return;
        img.gameObject.SetActive(true);
        var graphics = img.GetComponentsInChildren<Graphic>();
        foreach (var g in graphics)
        {
            SetAlpha(g, 0f);
        }
    }

    private void SetAlpha(Graphic g, float alpha)
    {
        if (g == null) return;
        Color c = g.color;
        c.a = alpha;
        g.color = c;
    }

    private void OnEnable()
    {
        inputReader.LeftButton += () => OnInputReceived(NoteInputType.LB);
        inputReader.LeftTrigger += () => OnInputReceived(NoteInputType.LT);
        inputReader.RightButton += () => OnInputReceived(NoteInputType.RB);
        inputReader.RightTrigger += () => OnInputReceived(NoteInputType.RT);
        inputReader.MenuButton += TogglePause;
    }

    private void OnDisable()
    {
        inputReader.LeftButton -= () => OnInputReceived(NoteInputType.LB);
        inputReader.LeftTrigger -= () => OnInputReceived(NoteInputType.LT);
        inputReader.RightButton -= () => OnInputReceived(NoteInputType.RB);
        inputReader.RightTrigger -= () => OnInputReceived(NoteInputType.RT);
        inputReader.MenuButton -= TogglePause;
    }

    private void TogglePause()
    {
        if (resultsPanel.alpha > 0) return; 

        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            ShowPauseMenu();
        }
        else
        {
            Time.timeScale = 1f;
            HidePauseMenu();
        }
    }

    private void ShowPauseMenu()
    {
        if (pausePanel != null)
        {
            pausePanel.alpha = 1;
            pausePanel.interactable = true;
            pausePanel.blocksRaycasts = true;
            
            var firstButton = pausePanel.GetComponentInChildren<Button>();
            if (firstButton != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
            }
            inputReader.ChangeActionMap(InputReader.ActionMapType.UI);
        }
    }

    private void HidePauseMenu()
    {
        if (pausePanel != null)
        {
            pausePanel.alpha = 0;
            pausePanel.interactable = false;
            pausePanel.blocksRaycasts = false;
            inputReader.ChangeActionMap(InputReader.ActionMapType.Player);
        }
    }

    public void ResumeGame()
    {
        if (isPaused) TogglePause();
    }

    private IEnumerator TutorialSequence()
    {
        // --- INTRODUCCIÓN ---
        yield return StartCoroutine(ShowTextSequence(textWelcome));
        
        // --- FASE 1: GUIADA (Lado Derecho) ---
        Debug.Log("[TutorialManager] Fase 1: Guiada (Derecha)");
        isPracticePhase = false;
        
        // Mostrar SOLO la base del mando
        StartCoroutine(FadeGraphic(gamepadBaseGraphic, 0f, 1f, 0.5f));
        
        // Mostrar highlights DERECHA (RB + RT)
        StartCoroutine(FadeHighlight(NoteInputType.RB, 0f, 1f, 0.5f));
        StartCoroutine(FadeHighlight(NoteInputType.RT, 0f, 1f, 0.5f));
        
        yield return StartCoroutine(ShowTextSequence(textRightInstruction));
        
        // Ocultar highlights generales
        StartCoroutine(FadeHighlight(NoteInputType.RB, 1f, 0f, 0.5f));
        StartCoroutine(FadeHighlight(NoteInputType.RT, 1f, 0f, 0.5f));

        // Nota RB
        yield return StartCoroutine(HandleGuidedNote(NoteInputType.RB, "Presiona RB"));
        
        // Nota RT
        yield return StartCoroutine(HandleGuidedNote(NoteInputType.RT, "Presiona RT"));

        // Ocultar mando temporalmente
        yield return StartCoroutine(FadeGraphic(gamepadBaseGraphic, 1f, 0f, 0.5f));
        yield return new WaitForSeconds(0.5f);

        // --- TEXTO INTERMEDIO (Izquierda) ---
        StartCoroutine(FadeGraphic(gamepadBaseGraphic, 0f, 1f, 0.5f)); // Reaparece mando base
        
        // Mostrar highlights IZQUIERDA (LB + LT)
        StartCoroutine(FadeHighlight(NoteInputType.LB, 0f, 1f, 0.5f));
        StartCoroutine(FadeHighlight(NoteInputType.LT, 0f, 1f, 0.5f));
        
        yield return StartCoroutine(ShowTextSequence(textLeftInstruction));
        
        // Ocultar highlights generales
        StartCoroutine(FadeHighlight(NoteInputType.LB, 1f, 0f, 0.5f));
        StartCoroutine(FadeHighlight(NoteInputType.LT, 1f, 0f, 0.5f));

        // --- FASE 1: GUIADA (Lado Izquierdo) ---
        Debug.Log("[TutorialManager] Fase 1: Guiada (Izquierda)");

        // Nota LB
        yield return StartCoroutine(HandleGuidedNote(NoteInputType.LB, "Presiona LB"));
        
        // Nota LT
        yield return StartCoroutine(HandleGuidedNote(NoteInputType.LT, "Presiona LT"));

        // Ocultar mando al terminar fase guiada
        yield return StartCoroutine(FadeGraphic(gamepadBaseGraphic, 1f, 0f, 0.5f));
        Debug.Log("[TutorialManager] Fase 1 completada.");

        // --- FASE 2: PRÁCTICA (Tiempo Real) ---
        // Mostrar mando una última vez
        StartCoroutine(FadeGraphic(gamepadBaseGraphic, 0f, 1f, 0.5f));
        
        // Mostrar TODOS los highlights (RB, RT, LB, LT)
        StartCoroutine(FadeHighlight(NoteInputType.RB, 0f, 1f, 0.5f));
        StartCoroutine(FadeHighlight(NoteInputType.RT, 0f, 1f, 0.5f));
        StartCoroutine(FadeHighlight(NoteInputType.LB, 0f, 1f, 0.5f));
        StartCoroutine(FadeHighlight(NoteInputType.LT, 0f, 1f, 0.5f));
        
        yield return StartCoroutine(ShowTextSequence(textPracticeIntro));
        
        // Ocultar TODOS los highlights
        StartCoroutine(FadeHighlight(NoteInputType.RB, 1f, 0f, 0.5f));
        StartCoroutine(FadeHighlight(NoteInputType.RT, 1f, 0f, 0.5f));
        StartCoroutine(FadeHighlight(NoteInputType.LB, 1f, 0f, 0.5f));
        StartCoroutine(FadeHighlight(NoteInputType.LT, 1f, 0f, 0.5f));
        
        StartCoroutine(FadeGraphic(gamepadBaseGraphic, 1f, 0f, 0.5f)); // Adiós mando

        Debug.Log("[TutorialManager] Fase 2: Práctica");
        isPracticePhase = true;
        for (int i = 0; i < practiceNoteCount; i++)
        {
            Debug.Log($"[TutorialManager] Nota práctica {i + 1}/{practiceNoteCount}");
            yield return StartCoroutine(HandlePracticeNote(i));
        }

        Debug.Log("[TutorialManager] Fase 2 completada. Mostrando resultados...");
        // Fin
        yield return new WaitForSeconds(1.0f); 
        ShowResults();
    }

    // --- Lógica Fase Guiada ---
    private IEnumerator HandleGuidedNote(NoteInputType type, string instruction)
    {
        if (feedbackText != null) feedbackText.text = instruction;
        SpawnNote(type);

        bool highlightShown = false;
        waitingForInput = true;
        
        while (waitingForInput)
        {
            if (currentNote == null) SpawnNote(type); 
            else
            {
                float dist = currentNote.GetDistanceToTarget();
                
                // Mostrar highlight ESPECÍFICO cuando se acerca
                if (!highlightShown && dist < 3.0f)
                {
                    StartCoroutine(FadeHighlight(type, 0f, 1f, 0.5f));
                    highlightShown = true;
                }

                if (dist < 0.1f)
                {
                    currentNote.Freeze();
                }
            }
            yield return null;
        }
        
        // Ocultar highlight al terminar
        StartCoroutine(FadeHighlight(type, 1f, 0f, 0.5f));
        
        if (feedbackText != null) feedbackText.text = ""; 
        yield return new WaitForSeconds(spawnDelay);
    }

    // --- Lógica Fase Práctica ---
    private IEnumerator HandlePracticeNote(int index)
    {
        NoteInputType type = GetRandomNoteType();
        if (feedbackText != null) feedbackText.text = $"Nota {index + 1} / {practiceNoteCount}";
        
        SpawnNote(type);
        waitingForInput = true;

        float timeout = 5.0f; 
        float timer = 0f;

        while (waitingForInput)
        {
            timer += Time.deltaTime;
            if (timer > timeout)
            {
                if (currentNote != null) Destroy(currentNote.gameObject);
                waitingForInput = false;
                break;
            }

            if (currentNote == null)
            {
                waitingForInput = false; 
                break;
            }

            // Simular Miss Late
            if (currentNote.GetDistanceToTarget() < 0.05f)
            {
                yield return new WaitForSeconds(0.2f); 
                if (waitingForInput && currentNote != null)
                {
                    if (FeedbackManager.Instance != null) FeedbackManager.Instance.ShowFeedback(currentNote.transform, FeedbackType.Miss);
                    Destroy(currentNote.gameObject);
                    waitingForInput = false;
                }
            }

            yield return null;
        }
        
        yield return new WaitForSeconds(practiceSpawnInterval);
    }

    private void SpawnNote(NoteInputType type)
    {
        Transform target = targetZoneManager.RequestRandomTarget(type);
        if (target == null) return;

        Vector3 spawnPos = GetSpawnPosition(type);
        GameObject go = Instantiate(tutorialNotePrefab, spawnPos, Quaternion.identity);
        currentNote = go.GetComponent<TutorialNoteBehavior>();
        currentNote.Initialize(type, target, targetZoneManager);
    }

    private void OnInputReceived(NoteInputType input)
    {
        if (isPaused) return;
        if (!waitingForInput || currentNote == null) return;

        if (currentNote.InputType != input)
        {
            if (isPracticePhase)
            {
                if (FeedbackManager.Instance != null) FeedbackManager.Instance.ShowFeedback(currentNote.transform, FeedbackType.Miss);
                Destroy(currentNote.gameObject);
                waitingForInput = false;
            }
            return; 
        }

        float distance = currentNote.GetDistanceToTarget();
        string result = EvaluateHit(distance);

        if (result == "Miss" && !isPracticePhase) return;

        currentNote.Freeze(); 
        
        var btn = currentNote.GetTargetButton();
        if (btn != null)
        {
            if (result == "Perfect") btn.ShowPerfectState();
            else if (result == "Good") btn.ShowGoodState();
            else btn.ShowMissState();
        }

        if (FeedbackManager.Instance != null)
        {
            FeedbackType fbType = FeedbackType.Miss;
            if (result == "Perfect") fbType = FeedbackType.Perfect;
            else if (result == "Good") fbType = FeedbackType.Good;
            Transform popupTarget = btn != null ? btn.transform : currentNote.transform;
            FeedbackManager.Instance.ShowFeedback(popupTarget, fbType);
        }

        waitingForInput = false;
        Destroy(currentNote.gameObject, 0.2f);
    }

    private string EvaluateHit(float distance)
    {
        if (distance <= perfectDistance) return "Perfect";
        if (distance <= goodDistance) return "Good";
        return "Miss";
    }

    private Vector3 GetSpawnPosition(NoteInputType type)
    {
        float xOffset = 10f;
        float x = (type == NoteInputType.LB || type == NoteInputType.LT) ? -xOffset : xOffset;
        return new Vector3(x, 0, 0);
    }

    private NoteInputType GetRandomNoteType()
    {
        var values = System.Enum.GetValues(typeof(NoteInputType));
        return (NoteInputType)values.GetValue(Random.Range(0, values.Length));
    }

    // --- FADE HELPERS ---

    private IEnumerator ShowTextSequence(GameObject textObj)
    {
        if (textObj == null) yield break;
        
        // Fade In
        yield return StartCoroutine(FadeGameObject(textObj, 0f, 1f, 0.5f));
        
        // Wait Visible
        yield return new WaitForSeconds(2.0f);
        
        // Fade Out
        yield return StartCoroutine(FadeGameObject(textObj, 1f, 0f, 0.5f));
    }

    private IEnumerator FadeGameObject(GameObject obj, float startAlpha, float endAlpha, float duration)
    {
        if (obj == null) yield break;
        var graphics = obj.GetComponentsInChildren<Graphic>();
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            foreach (var g in graphics)
            {
                SetAlpha(g, alpha);
            }
            yield return null;
        }
        
        foreach (var g in graphics)
        {
            SetAlpha(g, endAlpha);
        }
    }

    // Fade para un solo gráfico (usado para la base del mando)
    private IEnumerator FadeGraphic(Graphic g, float startAlpha, float endAlpha, float duration)
    {
        if (g == null) yield break;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            SetAlpha(g, alpha);
            yield return null;
        }
        SetAlpha(g, endAlpha);
    }

    private IEnumerator FadeHighlight(NoteInputType type, float startAlpha, float endAlpha, float duration)
    {
        Image targetImg = null;
        switch (type)
        {
            case NoteInputType.RB: targetImg = highlightRB; break;
            case NoteInputType.RT: targetImg = highlightRT; break;
            case NoteInputType.LB: targetImg = highlightLB; break;
            case NoteInputType.LT: targetImg = highlightLT; break;
        }

        if (targetImg != null)
        {
            // Usamos FadeGameObject para que también afecte a hijos si los tiene
            yield return StartCoroutine(FadeGameObject(targetImg.gameObject, startAlpha, endAlpha, duration));
        }
    }

    private void HideAllHighlights()
    {
        // Ya se inicializan en alpha 0 en Start
    }

    private void HideAllTexts()
    {
        // Se inicializan en alpha 0 en Start
    }

    private void ShowResults()
    {
        if (resultsPanel != null)
        {
            resultsPanel.gameObject.SetActive(true); 
            resultsPanel.alpha = 1;
            resultsPanel.interactable = true;
            resultsPanel.blocksRaycasts = true;
            
            var firstButton = resultsPanel.GetComponentInChildren<Button>();
            if (firstButton != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
            }
            inputReader.ChangeActionMap(InputReader.ActionMapType.UI);
        }
        
        if (feedbackText != null) feedbackText.text = "¡Tutorial Completado!";
    }

    public void RestartTutorial()
    {
        Time.timeScale = 1f; 
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
