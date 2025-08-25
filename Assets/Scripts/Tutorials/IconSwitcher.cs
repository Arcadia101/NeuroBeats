using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

public class IconSwitcher : MonoBehaviour
{
    [SerializeField] public Image R1;
    [SerializeField] public Image R2;
    [SerializeField] public Image Left1;
    [SerializeField] public Image Left2;
    [SerializeField] public Image StartButton;
    [SerializeField] public Image BackButton;
    [SerializeField] public Image PreviousButton;
    [SerializeField] public Image NextButton;
    [SerializeField] public string[] description;
    
    [SerializeField] public VideoClip[] videos;
    
    private void Start()
    {
        //UpdateIcons();
    }

    private void OnEnable()
    {
        StartCoroutine(InitBehavior());
    }

    private void OnDisable()
    {
        IconManagerTutorial.Instance.updateIcons -= UpdateIcons;
    }
    
    IEnumerator InitBehavior()
    {
        while (IconManagerTutorial.Instance == null)
        {
            yield return null;
        }
        IconManagerTutorial.Instance.updateIcons += UpdateIcons;
        
        
    }

    public event UnityAction updateIcons;
    void UpdateIcons()
    {
        R1.sprite = IconManagerTutorial.Instance.Right1;
        R1.SetNativeSize();

        R2.sprite = IconManagerTutorial.Instance.Right2;
        R2.SetNativeSize();

        Left1.sprite = IconManagerTutorial.Instance.Left1;
        Left1.SetNativeSize();

        Left2.sprite = IconManagerTutorial.Instance.Left2;
        Left2.SetNativeSize();
        
        PreviousButton.sprite = IconManagerTutorial.Instance.PreviousButton;
        PreviousButton.SetNativeSize();
        
        NextButton.sprite = IconManagerTutorial.Instance.NextButton;
        NextButton.SetNativeSize();
        
        StartButton.sprite = IconManagerTutorial.Instance.StartButton;
        StartButton.SetNativeSize();
        BackButton.sprite = IconManagerTutorial.Instance.BackButton;
        BackButton.SetNativeSize();
        
        videos = IconManagerTutorial.Instance.videoClips;
        description = IconManagerTutorial.Instance.textInfo;
        updateIcons?.Invoke();
    }


}
