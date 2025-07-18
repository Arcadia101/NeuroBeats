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
    private PointerEventData pointerData;
    private EventSystem eventSystem;

    private void Awake()
    {
        pointerRectTransform = GetComponent<RectTransform>();
        StartCoroutine(InitializeCanvas());
        if (raycaster == null)
        {
            Debug.Log("We couldn't initialize raycaster");
        }
        eventSystem = EventSystem.current;
    }

    IEnumerator InitializeCanvas()
    {
        while (uiCanvas == null)
        {
            yield return null;
        }
        raycaster = uiCanvas.GetComponent<GraphicRaycaster>();
        
        if (raycaster != null)
        {
            Debug.Log("We were able to initialize the raycaster");
        }

        if (pointerRectTransform != null)
        {
            Debug.Log("We were able to initialize the pointer rect transform");
        }
        
        yield return null;
    }
    
    public void SimulateUIClick()
    {
        // Step 1: Convert fake cursor position to screen space
        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(null, pointerRectTransform.position);

        // Step 2: Create PointerEventData for the raycast
        pointerData = new PointerEventData(eventSystem)
        {
            position = screenPosition
        };

        // Step 3: Raycast using GraphicRaycaster
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerData, results);//.Raycast(pointerData, results);

        foreach (var hit in results)
        {
            //Debug.Log("Hit: " + hit.gameObject.name);
        }
        if (results.Count > 0)
        {
            //Debug.Log("we found "+ results.Count + " raycast results"); //Debug log 1
            // Step 4: Send click events to the first valid UI element
            GameObject validTarget = null;

            foreach (var result in results)
            {
                GameObject candidate = ExecuteEvents.GetEventHandler<IPointerClickHandler>(result.gameObject);
                if (candidate != null)
                {
                    validTarget = candidate;
                    break;
                }
            }
            
            if (validTarget != null)
            {

                //Debug.Log("Valid click target: " + validTarget.name);

                ExecuteEvents.Execute(validTarget, pointerData, ExecuteEvents.pointerEnterHandler);
                ExecuteEvents.Execute(validTarget, pointerData, ExecuteEvents.pointerDownHandler);
                ExecuteEvents.Execute(validTarget, pointerData, ExecuteEvents.pointerClickHandler);
                ExecuteEvents.Execute(validTarget, pointerData, ExecuteEvents.pointerUpHandler);
                //Debug.Log("We are hitting click event at " + pointerData.position + "on "+ validTarget.name); //Debug log 2
            }
        }

        
    }
    /* Just a debugging tool ignore this crap.
        [SerializeField] GameObject debuggingTool;
        GameObject debugger = Instantiate(debuggingTool, validTarget.GetComponent<RectTransform>().position, Quaternion.identity);
        debugger.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z-0.5f);
    */
}
