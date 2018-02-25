#define TRACING_VERBOSE 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input; 


namespace WinMRSnippets
{
    public enum MotionControllerInteractionEvent
    {
        SelectPresed, 
        SelectReleased, 
        MenuPressed, 
        MenuReleased, 
        ThumbstickPressed, 
        ThumbstickReleased, 
        TouchpadPressed, 
        TouchpadReleased, 
        CustomEvent  
    }

    [RequireComponent(typeof(TrackedController))]
    public abstract class PointerRendererBase : MonoBehaviour {

        [SerializeField]
        protected MotionControllerInteractionEvent activationEvent = MotionControllerInteractionEvent.SelectPresed ;

        [SerializeField]
        protected MotionControllerInteractionEvent deActivationEvent = MotionControllerInteractionEvent.SelectPresed ;

        [SerializeField]
        protected InteractionSourceNode sourcePose; 


        protected  TrackedController trackedController;
        protected bool isActivated = false;
        protected bool isToggle = false;  
      
        // Use this for initialization
        protected void Start() {
            trackedController = GetComponent<TrackedController>();
            Debug.Assert(trackedController != null);
            StartListeners();

#if TRACING_VERBOSE
            Debug.Log(string.Format("Starting {0} with activation {1},deActivation: {2}, ",
                    this.GetType().ToString() , activationEvent, deActivationEvent)) ; 
#endif 
        }

        protected void StartListeners ()
        {
            isToggle = activationEvent == deActivationEvent; 
            if ( trackedController != null )
            {
                if (!isToggle)
                {
                    ToggleListener(activationEvent, OnActivateInteraction , true );
                    ToggleListener(deActivationEvent, OnDeactivateInteraction, true );
                } 
                else
                {
                    ToggleListener(activationEvent, OnToggleInteraction, true ); 
                }
            }
        }

        protected void RemoveListeners ()
        {
            if (!isToggle)
            {
                ToggleListener(activationEvent, OnActivateInteraction, false );
                ToggleListener(deActivationEvent, OnDeactivateInteraction , false );
            }
            else
            {
                ToggleListener(activationEvent, OnToggleInteraction, false );
            }
        }

        void ToggleListener (MotionControllerInteractionEvent interaction , MotionControllerStateChangedEventHandler  handler , bool subscribe )
        {
            switch ( interaction )
            {
                case MotionControllerInteractionEvent.MenuPressed:
                    if (subscribe)
                        trackedController.MenuPressed += handler;
                    else
                        trackedController.MenuPressed -= handler; 
                    break;
                case MotionControllerInteractionEvent.MenuReleased:
                    if ( subscribe )
                        trackedController.MenuReleased += handler;
                    else
                        trackedController.MenuReleased -= handler;
                    break;

                case MotionControllerInteractionEvent.SelectPresed:
                    if (subscribe)
                        trackedController.SelectPressed  += handler;
                    else
                        trackedController.SelectPressed -= handler;
                    break;

                case MotionControllerInteractionEvent.SelectReleased:
                    if (subscribe)
                        trackedController.SelectReleased += handler;
                    else
                        trackedController.SelectReleased -= handler;
                    break;

            }
        }

        void OnToggleInteraction(object sender, MotionControllerStateChangedEventArgs args)
        {
            isActivated = !isActivated ;
#if TRACING_VERBOSE
            Debug.Log("Renderer is activated: " + isActivated); 
#endif 
        }

        void OnActivateInteraction(object sender, MotionControllerStateChangedEventArgs args)
        {
            isActivated = true; 
        }

        void OnDeactivateInteraction(object sender, MotionControllerStateChangedEventArgs args)
        {
            isActivated = false ;
        }

        // Update is called once per frame
        void Update() {
            RenderPointer(); 
        }

        protected abstract void RenderPointer(); 
         
    }


   
} 
