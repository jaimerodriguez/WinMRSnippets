using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

using WinMRSnippets;

namespace WinMRSnippets
{
    public class TrackedController : MonoBehaviour
    {
        [Tooltip("Controllers hand")]
        [SerializeField]
        private InteractionSourceHandedness handedness;
        public InteractionSourceHandedness Handedness { get { return handedness; } }


        private const uint defaultValue = uint.MinValue;
        public uint SourceId { get; private set; }  

        private MotionControllerState _currentState; 
        public MotionControllerState State { get { return _currentState; } } 

        private bool IsActive { get; set;  }
        
        #region Controller Event Listeners 
        private void StartListeners ()
        {
            InteractionManager.InteractionSourceDetected += InteractionManager_InteractionSourceDetected;
            InteractionManager.InteractionSourceLost += InteractionManager_InteractionSourceLost;
            InteractionManager.InteractionSourceUpdated += InteractionManager_InteractionSourceUpdated;
            InteractionManager.InteractionSourcePressed += InteractionManager_InteractionSourcePressed;
            InteractionManager.InteractionSourceReleased += InteractionManager_InteractionSourceReleased;
        }

        private void Awake()
        {
            SourceId = defaultValue;  
        }

        private void StopListeners ()
        {
            InteractionManager.InteractionSourceDetected -= InteractionManager_InteractionSourceDetected;
            InteractionManager.InteractionSourceLost -= InteractionManager_InteractionSourceLost;
            InteractionManager.InteractionSourceUpdated -= InteractionManager_InteractionSourceUpdated;
            InteractionManager.InteractionSourcePressed -= InteractionManager_InteractionSourcePressed;
            InteractionManager.InteractionSourceReleased -= InteractionManager_InteractionSourceReleased;
        }

        private void InteractionManager_InteractionSourceDetected(InteractionSourceDetectedEventArgs args)
        {
            InteractionSourceState state = args.state;
            InteractionSource source = state.source;

            if (source.kind == InteractionSourceKind.Controller && source.handedness == handedness)
            {

                InitializeSelf(args.state); 
#if TRACING_VERBOSE
                Debug.Log("TrackedController SourceDetected: " + SourceId); 
#endif

#if DEBUG
                Debug.Assert(SourceId != defaultValue); 
#endif 
            }
        }

      

        private void InteractionManager_InteractionSourceLost(InteractionSourceLostEventArgs args)
        {
             
            if ( args.state.source.kind == InteractionSourceKind.Controller && args.state.source.id == SourceId )
            {
#if TRACING_VERBOSE
                Debug.Log("TrackedController SourceLost : " + SourceId); 
#endif
                UninitializeSelf(args.state);        
            }
#if DEBUG
            else
            {  
                Debug.Assert(args.state.source.handedness != handedness, "Having two controllers for same hand is not expected");
            }
#endif 
        }

        private void InteractionManager_InteractionSourceUpdated(InteractionSourceUpdatedEventArgs args)
        {             
            if ( args.state.source.kind == InteractionSourceKind.Controller && args.state.source.id == SourceId )
            {
                if (!IsActive)
                {
                    InitializeSelf(args.state); 
                }

                UpdateState(args.state); 
                  

                //TODO: REMOVE 
                //UpdateAxis(state);
                //UpdatePose(state);
            }
        }

        #endregion 


        void InitializeSelf ( InteractionSourceState state   )
        {
#if DEBUG
            Debug.Assert(SourceId == defaultValue, "We expect to never initialize this instance without a proper SourceLost");              
#endif 
            SourceId =  state.source.id;
            IsActive = true;
        }

        void UninitializeSelf(InteractionSourceState state)
        {
#if DEBUG
            Debug.Assert(SourceId == defaultValue, "We expect to never initialize this instance without a proper SourceLost");
#endif
            SourceId = defaultValue ;
            IsActive = false ;
        }

        void UpdateControllerProperties ( InteractionSource source )
        {
            _currentState.SupportsGrasp = source.supportsGrasp;
            _currentState.SupportsMenu = source.supportsMenu;
            _currentState.SupportsPointing = source.supportsPointing;
            _currentState.SupportsTouchpad = source.supportsTouchpad;
            _currentState.SupportsMenu = source.supportsMenu;
            _currentState.VendorId = source.vendorId;
            _currentState.ProductVersion = source.productVersion; 
            _currentState.Id = source.id;
            _currentState.IsLeftHand = source.handedness == InteractionSourceHandedness.Left;
            _currentState.IsRightHand = source.handedness == InteractionSourceHandedness.Right; 

        }

        void ResetState()
        {
            //ANalog properties 
            _currentState.TouchpadPosition = Vector2.zero;
            _currentState.ThumbStickPosition = Vector2.zero;
            _currentState.SelectValue = 0f; 

            
            //POSE 
            _currentState.GripPosition = Vector3.zero;
            _currentState.GripRotation = Quaternion.identity; 
            _currentState.PointerPosition = Vector3.zero;
            _currentState.PointerRotation = Quaternion.identity;
            _currentState.PointerForward = Vector3.zero;
            _currentState.GripForward = Vector3.zero;
            _currentState.AngularVelocity = Vector3.zero ;  


            //Button Pressses          
            _currentState.SelectPressed= false; 
            _currentState.MenuPressed= false; 
            _currentState.GraspPressed= false; 
            _currentState.TouchPadPressed= false; 
            _currentState.TouchPadTouched= false; 
            _currentState.ThumbstickPressed= false; 

        }


         
        void UpdateState ( InteractionSourceState state)
        {
            // Analog properties 
            _currentState.SelectValue = state.selectPressedAmount;
            _currentState.TouchpadPosition = state.touchpadPosition;
            _currentState.ThumbStickPosition = state.thumbstickPosition;

            //Button presses 
            _currentState.SelectPressed = state.selectPressed;
            _currentState.MenuPressed = state.menuPressed ;
            _currentState.GraspPressed = state.grasped;
            _currentState.TouchPadPressed = state.touchpadPressed ;
            _currentState.TouchPadTouched = state.touchpadTouched ;
            _currentState.ThumbstickPressed = state.thumbstickPressed ;                         
             
        }
 
        void UpdatePose ( InteractionSourcePose pose )             
        {
            Vector3 angularVelocity, gripPosition, pointerPosition , pointerForward, gripForward ;
            Quaternion gripRotation, pointerRotation; 
            
            if ( pose.TryGetPosition( out gripPosition , InteractionSourceNode.Grip ))
            {
                _currentState.GripPosition = gripPosition;  
            }
            
            if ( pose.TryGetPosition ( out pointerPosition , InteractionSourceNode.Pointer))
            {
                _currentState.PointerPosition = pointerPosition; 
            }

            if (pose.TryGetRotation(out pointerRotation , InteractionSourceNode.Pointer))
            {
                _currentState.PointerRotation = pointerRotation ;
            }


            if (pose.TryGetRotation(out gripRotation , InteractionSourceNode.Grip))
            {
                _currentState.GripRotation= gripRotation;
            }

            if ( pose.TryGetForward( out pointerForward, InteractionSourceNode.Pointer ))
            {
                _currentState.PointerForward = pointerForward; 
            }

            if (pose.TryGetAngularVelocity(out angularVelocity))
            {
                _currentState.AngularVelocity = angularVelocity;
            }

        }


        private bool IsTarget ( InteractionSourceState state )
        {
            if (state.source.kind == InteractionSourceKind.Controller && state.source.id == SourceId)
                return true;
            return false; 

        }

        void UpdatePressed(InteractionSourcePressType pressType, InteractionSourceState state)
        {
            switch (pressType )
            {                 
                case InteractionSourcePressType.Select:
                    _currentState.SelectPressed = state.selectPressed;                     
                    break;                 
                case InteractionSourcePressType.Grasp:
                    _currentState.GraspPressed = state.grasped ;                     
                    break;
                case InteractionSourcePressType.Menu:
                    _currentState.MenuPressed = state.menuPressed;                      
                    break;
                case InteractionSourcePressType.Touchpad:
                    _currentState.TouchPadPressed = state.touchpadPressed;                      
                    break;
                case InteractionSourcePressType.Thumbstick:
                    _currentState.ThumbstickPressed = state.thumbstickPressed;                      
                    break;
            }
        }  
        private void InteractionManager_InteractionSourcePressed(InteractionSourcePressedEventArgs args)
        {
             
            if (IsTarget(args.state))
            {
                UpdatePressed (args.pressType, args.state );
            }
        }

        private void InteractionManager_InteractionSourceReleased(InteractionSourceReleasedEventArgs args)
        {             
            if ( IsTarget(args.state)) 
            {
                UpdatePressed (args.pressType, args.state );
            }
        }


    }

} 