using System;
using UnityEngine;
using UnityEngine.UI;

public class ResultUIController : MonoBehaviour
{
    [SerializeField] private CanvasGroup resultMenu;
    [SerializeField] private InputReader inputReader;
    [SerializeField] bool debugging = false;
    private void Awake()
    {
        LevelEndController.Instance.OnLevelEnd.AddListener(ShowResults);
        if (debugging)
        {
            inputReader.DebugButton  += ShowResults;    
        }
        
    }

    
    private void Start()
    {
        HideResults();
    }

    private void ShowResults()
    {
        resultMenu.alpha = 1;
        resultMenu.interactable = true;
        resultMenu.blocksRaycasts = true;
        
    }
    
    private void HideResults()
    {
        resultMenu.alpha = 0;
        resultMenu.interactable = false;
        resultMenu.blocksRaycasts = false;
    }

    public void RetryLevel(LevelConfig levelConfig)
    {
        
        GameManager.Instance.currentConfig = levelConfig;
        GameManager.Instance.LoadLevel(levelConfig);
    }

    public void MainMenu()
    {
        
    }
    
    
    
}
