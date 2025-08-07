using System;
using UnityEngine;
using UnityEngine.UI;

public class ResultUIController : MonoBehaviour
{
    [SerializeField] private CanvasGroup resultMenu;

    private void Awake()
    {
        LevelEndController.Instance.OnLevelEnd.AddListener(ShowResults);
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
}
