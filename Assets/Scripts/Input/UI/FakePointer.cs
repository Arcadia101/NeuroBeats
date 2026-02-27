using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
public class FakePointer : MonoBehaviour
{
    [SerializeField] private RectTransform pointerRectTransform;
    [SerializeField] private Canvas uiCanvas;

    private GraphicRaycaster raycaster;

    private void Awake()
    {
        pointerRectTransform = GetComponent<RectTransform>();
    }


    public void AdjustPointerPosition(Vector2 position)
    {
        pointerRectTransform.position =  position;
        
    }
}
