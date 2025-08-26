using System;
using System.Collections;
using UI.MainMenu;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


public class NewGamePanelBehavior : MonoBehaviour, IUIPanel, IPointerClickHandler
{
    [SerializeField] private RectTransform panelRect;
    
    private void OnEnable()
    {
        StartCoroutine(InitBehavior());
        //Debug.Log("Ola wenas");
        
    }
    private void OnDisable()
    {
        EndBehavior();
        //Debug.Log("Ya meboi");
    }
    public IEnumerator InitBehavior()
    {
        while (MainMenu.Instance == null)
        {
            yield return null;
        }

        MainMenuController.Instance.leftClick += ClickEvent;
        MainMenuController.Instance.backButton += Back;
    }

    public void EndBehavior()
    {
        if (MainMenuController.Instance != null)
        {
            MainMenuController.Instance.leftClick -= ClickEvent;
            MainMenuController.Instance.backButton -= Back;
        }
    }

    public void ClickEvent() //Detect if left mouse click happened 
    {
        Vector2 localPoint = RectTransformUtility.WorldToScreenPoint(null, MainMenuController.Instance.pointerPosition);
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRect,MainMenuController.Instance.pointerPosition, null, out Vector2 localPoint);
        if (!RectTransformUtility.RectangleContainsScreenPoint(panelRect, localPoint, null))
        {
            Back(); // Disable the panel if clicked outside
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log("We are clicking");
        Vector2 clickPos = eventData.position;
        if (!RectTransformUtility.RectangleContainsScreenPoint(panelRect, clickPos, null))
        {
            Back();
        }
    }

    public void Back()
    {
        gameObject.SetActive(false);
    }
    
    
}
