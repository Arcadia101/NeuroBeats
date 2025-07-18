using System;
using System.Collections;
using UI.MainMenu;
using UnityEngine;
using System.Collections.Generic;

public class OptionsMenu : MonoBehaviour, IUIPanel
{

    //----------------------------------------//
    //    List of interfaces                     // ------------------------------------------------------------------ //
    //----------------------------------------// 
    /// <summary>
    /// Here we define the panels that will be visible within the Options menu
    /// These going to be stored in a System.Collections.Generic new List<GameObject> { } 
    /// </summary>

    //------------------   Declaration     ----------------------// 
    [SerializeField] private GameObject videoInterface;
    [SerializeField] private GameObject audioInterface;
    [SerializeField] private GameObject controlsInterface;
    
    List<GameObject> optionsInterfaces = new List<GameObject>(); //List that is going to contain the interfaces

    private void Awake()
    {
        optionsInterfaces = new List<GameObject> { 
            videoInterface, 
            audioInterface, 
            controlsInterface //If you want more interfaces, add them to the List of interfaces section then come back and add them here
        };
    }

    private void OnEnable()
    {
        StartCoroutine(InitBehavior());
        Debug.Log("Ola wenas");
        
    }
    private void OnDisable()
    {
        EndBehavior();
        Debug.Log("Ya meboi");
    }
    public IEnumerator InitBehavior()
    {
        while (MainMenu.Instance == null)
        {
            yield return null;
        }
        MainMenuController.Instance.backButton += Back;
    }
    private void EndBehavior()
    {
        if (MainMenuController.Instance != null)
        {
            MainMenuController.Instance.backButton -= Back;
        }
    }
    
    public void Video()
    {
        Debug.Log("Video settings");
        SwitchInterface(videoInterface);
        
    }

    public void Audio()
    {
        Debug.Log("Audio settings");
        SwitchInterface(audioInterface);
    }

    public void Controls()
    {
        Debug.Log("Controls settings");
        SwitchInterface(controlsInterface);
    }

    public void Back()
    {
        Debug.Log("Back to main menu");
        SwitchInterface(gameObject);
        gameObject.SetActive(false);
        MainMenu.Instance.MainMenu_Interface();
    }

    void SwitchInterface(GameObject obj)
    {
        videoInterface.SetActive(false);
        audioInterface.SetActive(false);
        controlsInterface.SetActive(false);
        obj.SetActive(true);
    }
    
}
