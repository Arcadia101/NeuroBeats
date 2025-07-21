using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
public class TutorialLevel : MonoBehaviour
{
    
    //----------------------------------------//
    //    Profile pattern design        // ------------------------------ //
    //----------------------------------------//
    [SerializeField] private Image currentTutorialIcon1;
    [SerializeField] private Image currentTutorialIcon2;
    [SerializeField] private VideoClip currentVideo;
    [SerializeField] private TextMeshProUGUI currentTutorialText;
    [SerializeField] private int profiles;//Digamos que tenemos, profile 1 2 y 3
    public int currentProfile = 0;

    public Sprite defaultImage;
    //----------------------------------------//
    //   End of Profile pattern design        // ------------------------------ //
    //----------------------------------------//
    
    
    //----------------------------------------//
    //    IconSwitcher declaration        // ------------------------------ //
    //----------------------------------------//
    
    [SerializeField] IconSwitcher iconSwitcher;
    private void Start()
    {
        
        //iconSwitcher = GetComponent<IconSwitcher>();
    }
    //----------------------------------------//
    //    End of IconSwitcher declaration        // ------------------------------ //
    //----------------------------------------//

    private void OnEnable()
    {
        iconSwitcher.updateIcons += UpdateIcons;
        StartCoroutine(InitBehavior());

    }

    private void OnDisable()
    {
        iconSwitcher.updateIcons -= UpdateIcons;
        TutorialController.Instance.submitButton -= StartGame;
        TutorialController.Instance.startButton -= StartGame;
        TutorialController.Instance.backButton -= BackToMainMenu;
        TutorialController.Instance.previousButton -= PreviousTutorial;
        TutorialController.Instance.nextButton -= NextTutorial;
        TutorialController.Instance.submitButton -= NextTutorial;
        
    }

    IEnumerator InitBehavior()
    {
        while (TutorialController.Instance == null)
        {
            yield return null;
        }
        
        TutorialController.Instance.submitButton += NextTutorial;
        TutorialController.Instance.startButton += StartGame;
        TutorialController.Instance.backButton += BackToMainMenu;
        TutorialController.Instance.previousButton += PreviousTutorial;
        TutorialController.Instance.nextButton += NextTutorial;
        TutorialController.Instance.submitButton += NextTutorial;


        while (iconSwitcher.videos.Length == 0)
        {
            yield return null;
        }
        profiles = iconSwitcher.videos.Length;
        UpdateIcons();
    }


    

    public void NextTutorial()
    {
        if (currentProfile < profiles - 1)
        {
            currentProfile += 1;
            SetProfile(currentProfile); 
        }

    }

    public void PreviousTutorial()
    {
        if(currentProfile > 0)
        {
            currentProfile -= 1;
            SetProfile(currentProfile); 
        }
    }

    public void StartGame()
    {
        if (GameScenesManager.Instance != null)
        {
            GameScenesManager.Instance.ChangeScene(SceneName.LevelMenu);
        }
    }

    public void BackToMainMenu()
    {
        if (GameScenesManager.Instance != null)
        {
            GameScenesManager.Instance.ChangeScene(SceneName.MainMenu);
        }
    }
    
    [SerializeField] private VideoPlayer videoPlayer;
    //[SerializeField] private Texture2D rawImageTexture; //not sure why we call it yet. but we might need it

    void UpdateIcons()
    {
        // Debug.Log("we are updating the icons");
        SetProfile(currentProfile);
    }
    public void DisplayVideo()
    {
        videoPlayer.clip = currentVideo;
        videoPlayer.Play();
    }

    public void SetProfile(int profile)
    {
        Debug.Log("We will try to set a profile on TutorialLevel");
        switch (profile)
        {
            case 0:
                currentTutorialIcon1.sprite = iconSwitcher.Left1.sprite;
                currentTutorialIcon1.SetNativeSize();
                currentTutorialIcon2.sprite = iconSwitcher.Left2.sprite;
                currentTutorialIcon2.SetNativeSize();
                currentTutorialText.text = iconSwitcher.description[0];
                currentVideo = iconSwitcher.videos[0];
                break;
            case 1:
                currentTutorialIcon1.sprite = iconSwitcher.R1.sprite;
                currentTutorialIcon1.SetNativeSize();
                currentTutorialIcon2.sprite = iconSwitcher.R2.sprite;
                currentTutorialIcon2.SetNativeSize();
                currentVideo = iconSwitcher.videos[1];
                currentTutorialText.text = iconSwitcher.description[1];
                break;
            case 2:
                currentTutorialIcon1.sprite = iconSwitcher.Left1.sprite;
                currentTutorialIcon1.SetNativeSize();
                currentTutorialIcon2.gameObject.SetActive(false);
                currentVideo = iconSwitcher.videos[2];
                currentTutorialText.text = iconSwitcher.description[2];
                break;
            default:
                break;
        }
        Debug.Log("We were able to Set a profile from TutorialLevel");
        DisplayVideo();
    }
    
    
    
}
