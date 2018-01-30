using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/* 
 * 
 * 
 IBeginDragHandler, 
 ICancelHandler, 
 IDeselectHandler, 
 IDragHandler, 
 IDropHandler, 
 IEndDragHandler, 
 IEventSystemHandler, 
 IInitializePotentialDragHandler, 
 IMoveHandler, 
 IPointerClickHandler, 
 IPointerDownHandler, 
 IPointerEnterHandler, 
 IPointerExitHandler, 
 IPointerUpHandler, 
 IScrollHandler, 
 ISelectHandler, 
 ISubmitHandler, 
 IUpdateSelectedHandler
*/

public class UIEventListener : MonoBehaviour,
        IPointerClickHandler,
        IPointerExitHandler,
        IPointerEnterHandler,
        ISelectHandler,
        IDeselectHandler,
        ISubmitHandler  
    //  TODO:  Implment move & Drag 
    //  , IMoveHandler 
    //  , IDragHandler, IBeginDragHandler, IEndDragHandler 
{

    public LogViewComponent logView; 
    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        LogEvent("PointerClick");         
    }

    public void OnPointerEnter( PointerEventData eventData)
    {
        LogEvent("PointerEnter"); 
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        LogEvent("PointerExit");
    } 

    public void OnDeselect( BaseEventData eventData)
    {
        LogEvent("Deselect"); 
    }

    public void OnSelect( BaseEventData eventData)
    {
        LogEvent("Select"); 
    }

    public void OnSubmit( BaseEventData eventData)
    {
        LogEvent("Submit"); 
    }


    void LogEvent ( string eventName )
    {
        Debug.Log(string.Format("{0}:{1} at {2}", name, eventName, Time.unscaledTime)); 
    }

    #region ButtonHandlers 
    public void Button_OnPointerClick(  )
    {
        Debug.Log("Button_OnPointerClick:" + name);

        //if ( logView != null )
        //{
        //    logView.Clear(); 
        //}
    }

    public void Button_OnSubmit(  )
    {
        Debug.Log("Button_OnSubmit:" + name);
    } 

    #endregion 
}
