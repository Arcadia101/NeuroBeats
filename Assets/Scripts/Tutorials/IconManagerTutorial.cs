using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class IconManagerTutorial : MonoBehaviour
{
    public static IconManagerTutorial Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        //UpdateIcons(TutorialController.Instance.currentScheme);
    }
    
    //----------------------------------------//
    //    Initialize Behavior and subscriptions // ------------------------------ //
    //----------------------------------------//
    private void OnEnable()
    {
        StartCoroutine(InitBehavior());
    }
    IEnumerator InitBehavior()
    {
        while (TutorialController.Instance == null)
        {
            yield return null;
        }
        TutorialController.Instance.controlsChanged += UpdateIcons;
        UpdateIcons(TutorialController.Instance.currentScheme);

    }

    private void OnDisable()
    {
        TutorialController.Instance.controlsChanged -= UpdateIcons;
    }
    
    //----------------------------------------//
    //    End Initialize Behavior and subscriptions // ------------------------------ //
    //----------------------------------------//
    
    public Sprite Right1;
    public Sprite Right2;
    public Sprite Left1;
    public Sprite Left2;
    public Sprite StartButton;
    public Sprite BackButton;
    public Sprite SubmitButton;
    public Sprite PreviousButton;
    public Sprite NextButton;
    public VideoClip[] videoClips;
    public string[] textInfo;
    
    [SerializeField] TutorialInfo tutorialIconsGamePad;
    [SerializeField] TutorialInfo tutorialIconsKeyboard;
    
    public TutorialInfo currentTutorialIcons;
    public event UnityAction updateIcons = delegate { };


    public void UpdateIcons(TutorialController.InputScheme input)
    {
        switch (input)
        {
            case TutorialController.InputScheme.Gamepad:
                currentTutorialIcons = tutorialIconsGamePad;
                break;
            case TutorialController.InputScheme.Keyboard:
                currentTutorialIcons = tutorialIconsKeyboard;
                break;
            default:
                currentTutorialIcons = tutorialIconsKeyboard;
                break;
        }
        Right1 = currentTutorialIcons.Right1;
        Right2 = currentTutorialIcons.Right2;
        Left1 = currentTutorialIcons.Left1;
        Left2 = currentTutorialIcons.Left2;
        StartButton = currentTutorialIcons.StartButton;
        BackButton = currentTutorialIcons.BackButton;
        SubmitButton = currentTutorialIcons.SubmitButton;
        PreviousButton = currentTutorialIcons.PreviousButton;
        NextButton = currentTutorialIcons.NextButton;
        textInfo = currentTutorialIcons.textInfo;
        videoClips = currentTutorialIcons.videoClips;
        updateIcons?.Invoke();
    }
}
