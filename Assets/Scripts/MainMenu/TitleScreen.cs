using System.Collections;
using UI.MainMenu;
using Unity.VisualScripting;
using UnityEngine;

public class TitleScreen : MonoBehaviour, IUIPanel
{
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
    
    public void Back()
    {
        
    }
    
    public void StartGame()
    {
        gameObject.SetActive(false);
    }
    
    
}
