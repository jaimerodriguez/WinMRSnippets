using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

using WinMRSnippets;

namespace WinMRSnippets
{
    public class MotionControllerStateChangedEventArgs
    {
        public MotionControllerStateChangedEventArgs ( MotionControllerState state )
        {
            NewState = state; 
        }

        public MotionControllerState NewState { get; private set;  }

    }

    public delegate void MotionControllerStateChangedEventHandler (object sender, MotionControllerStateChangedEventArgs args); 

    [System.Flags ]
    public enum TrackedControllerAttributesEnum
    {
        None = 0 , 
        FireEvents  = ( 1 << 0) , 
        Pose        = ( 1 << 1 ), 
        ButtonStates = ( 1 << 2 ), 
        All = ~0 
    }

    public class TrackedController : MonoBehaviour
    {
        [Tooltip("Controller hand to track")]
        [SerializeField]
        private InteractionSourceHandedness handedness = InteractionSourceHandedness.Unknown ;
        public InteractionSourceHandedness Handedness { get { return handedness; } }

        [SerializeField]     
        TrackedControllerAttributesEnum attributesToTrack = TrackedControllerAttributesEnum.All ;

        [SerializeField]
        bool animateSystemControllerModel  = true;
         

        public TrackedControllerAttributesEnum AttributesToTrack
        {
            get { return attributesToTrack; }
            set { attributesToTrack = value; }
        }

        [SerializeField]
        private bool useSystemControllerModel = false ;

        [SerializeField]
        private Material controllerMaterial; 

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
            if (this.useSystemControllerModel )
            {
                ControllerModelProvider.Instance.StartListening();
                Debug.Assert(controllerMaterial != null , "Controller material must not be null when using System Controller" ); 
                
            }
            else
            {
                Debug.Assert(!animateSystemControllerModel, "if not using controller model, no need to enable animations");
                animateSystemControllerModel = false; 
            }
        }


        private void Start()
        {
            StartListeners(); 
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
                // Note: when using a system controller model we delay initialization until control is fully loaded. 
                if (useSystemControllerModel)
                {
                    StartCoroutine(LoadControllerModel(source));
                }
                else // Note: when using a controller model we delay initialization until control is fully loaded. 
                {
                    InitializeSelf(args.state.source );
                } 

#if TRACING_VERBOSE
                Debug.Log("TrackedController SourceDetected: " + (useSystemControllerModel? args.state.source.id : SourceId )); 
#endif

            }
        }

        IEnumerator LoadControllerModel (InteractionSource source )
        {
#if DEBUG 
            if ( activeControllerModels.ContainsKey( source.id))
            {
                string error = "We should not be loading a model on source we already have. This implies incorrect Lost/Detected sequence. "; 
                Debug.Assert(false, error );
                Debug.LogError(error);                  
            }
#endif 
            if (!controllerModelsLoading.Contains (source.id))
            {
                GameObject go = new GameObject();
                go.transform.parent = this.transform;
                go.name = "Controller " + source.id;
                controllerModelsLoading.Add(source.id);  
                yield return ControllerHelpers.AttachModel(go, this.transform, source, controllerMaterial, controllerMaterial);
                activeControllerModels.Add(source.id, go);
                InitializeSelf(source); 
                if (animateSystemControllerModel)
                {
                    var info = new MotionControllerInfo() { ControllerParent = this.gameObject };
                    {
                        info.LoadInfo(this.transform, go.GetComponentsInChildren<Transform>(), controllerMaterial);
                        activeMotionControllerInfo.Add(source.id, info); 
                    }
                }
                controllerModelsLoading.Remove(source.id); 
            }
        }

        Dictionary<uint, GameObject> activeControllerModels = new Dictionary<uint, GameObject>();
        Dictionary<uint, MotionControllerInfo> activeMotionControllerInfo = new Dictionary<uint, MotionControllerInfo>();
        List<uint> controllerModelsLoading = new List<uint>(); 

        void UnloadControllerModel ( uint id )
        {
            GameObject controllerModel; 
            if ( activeControllerModels.TryGetValue(id, out controllerModel))
            {                
                activeControllerModels.Remove(id);
                Destroy(controllerModel);                
            }

            MotionControllerInfo info;
            if (activeMotionControllerInfo.TryGetValue(id, out info))
            {
                activeMotionControllerInfo.Remove(id);  
            } 
        }

        private void InteractionManager_InteractionSourceLost(InteractionSourceLostEventArgs args)
        {
             
            if ( args.state.source.kind == InteractionSourceKind.Controller && args.state.source.handedness == this.handedness )
            {
#if TRACING_VERBOSE
                Debug.Log("TrackedController SourceLost : " + SourceId); 
#endif                 
                UninitializeSelf(args.state.source );        
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
             
            if (!IsActive && args.state.source.handedness == this.handedness && args.state.source.kind == InteractionSourceKind.Controller )
            {
                InitializeSelf(args.state.source);
            }

            if ( IsTarget(args.state) )
            {
                 _currentState.ResetDynamicState(); 

                UpdateState(args.state);
                UpdatePose(args.state.sourcePose);

                if ( animateSystemControllerModel  )
                {
                    AnimateController( args.state.source.id ); 
                } 
            }
        }

        #endregion

        void AnimateController(uint id)
        {
            MotionControllerInfo controllerInfo;
            if (activeMotionControllerInfo.TryGetValue(id, out controllerInfo))
            {
                controllerInfo.AnimateSelect(_currentState.SelectValue);

                if (_currentState.SupportsGrasp)
                {
                    controllerInfo.AnimateGrasp(_currentState.GraspPressed);
                }

                if (_currentState.SupportsMenu)
                {
                    controllerInfo.AnimateMenu(_currentState.MenuPressed);
                }

                if (_currentState.SupportsThumbstick)
                {
                    controllerInfo.AnimateThumbstick(_currentState.ThumbstickPressed, _currentState.ThumbStickPosition);
                }

                if (_currentState.SupportsTouchpad)
                {
                    controllerInfo.AnimateTouchpad(_currentState.TouchPadPressed, _currentState.TouchPadTouched, _currentState.TouchpadPosition);
                }
            }       
        }


        void InitializeSelf ( InteractionSource source )
        {
#if TRACING_VERBOSE
            Debug.Log("Initializing tracked controller for " + source.id);
#endif
            //This can happen if there is an event while the controller model is loading. 
            if (SourceId == source.id)
            {
                IsActive = true;
                //This just confirms that StaticProperties have been initialzed 
                Debug.Assert(_currentState.Id == SourceId); 
            }
            else
            {
#if DEBUG
                Debug.Assert(SourceId == defaultValue, "We expect to never initialize this instance without a proper SourceLost");
#endif
                SourceId = source.id;
                IsActive = true;
                UpdateControllerProperties(source);
            } 
        }

        void UninitializeSelf(InteractionSource source )
        {
            if ( source.id == SourceId)
            {
                SourceId = defaultValue;
                IsActive = false;
                UnloadControllerModel( source.id);
                _currentState.Reset( defaultValue );
            } 
            else 
            {
                string errorMessage = string.Format("Unexpected Release for controller id:{0}, when this Tracker feels it is {1}",
                    source.id, SourceId); 

                Debug.Assert(false, errorMessage); 
                Debug.LogError(errorMessage); 
                UnloadControllerModel( source.id); 
            }
        }

        void UpdateControllerProperties ( InteractionSource source )
        {
            _currentState.SupportsGrasp = source.supportsGrasp;
            _currentState.SupportsMenu = source.supportsMenu;
            _currentState.SupportsPointing = source.supportsPointing;
            _currentState.SupportsTouchpad = source.supportsTouchpad;
            _currentState.SupportsThumbstick = source.supportsThumbstick; 
            _currentState.VendorId = source.vendorId;
            _currentState.ProductVersion = source.productVersion; 
            _currentState.Id = source.id;
            _currentState.IsLeftHand = source.handedness == InteractionSourceHandedness.Left;
            _currentState.IsRightHand = source.handedness == InteractionSourceHandedness.Right; 
        }

        void ResetDynamicState()
        {
            //Analog properties 
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

            Vector3 angularVelocity, velocity, gripPosition, pointerPosition , pointerForward, gripForward ;
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


            if (pose.TryGetForward(out gripForward, InteractionSourceNode.Grip))
            {
                _currentState.GripForward = gripForward;
            }



            if (pose.TryGetAngularVelocity(out angularVelocity ))
            {
                _currentState.AngularVelocity = angularVelocity;
            }

            if (pose.TryGetVelocity(out velocity))
            {
                _currentState.Velocity = velocity;
            }
             
        }


        private bool IsTarget ( InteractionSourceState state )
        {
            if (state.source.kind == InteractionSourceKind.Controller && state.source.id == SourceId)
                return true;
            return false; 

        }


        void UpdatePressed(InteractionSourcePressType pressType, InteractionSourceState state , bool isRelease , bool updateState = true )
        {
            MotionControllerStateChangedEventHandler handler = null; 
            if ( updateState )
            {
                UpdateState(state);
                UpdatePose(state.sourcePose); 
            }

            switch (pressType )
            {                 
                case InteractionSourcePressType.Select:
                    _currentState.SelectPressed = state.selectPressed;
                    handler = (isRelease ? SelectReleased : SelectPressed);                     
                    break;                 
                case InteractionSourcePressType.Grasp:
                    _currentState.GraspPressed = state.grasped ;
                    handler = (isRelease ? GraspReleased : GraspPressed);
                    break;
                case InteractionSourcePressType.Menu:
                    _currentState.MenuPressed = state.menuPressed;
                    handler = (isRelease ? MenuReleased : MenuPressed);
                    break;
                case InteractionSourcePressType.Touchpad:
                    _currentState.TouchPadPressed = state.touchpadPressed;
                    handler = (isRelease ? TouchpadReleased : TouchpadPressed);
                    break;
                case InteractionSourcePressType.Thumbstick:
                    _currentState.ThumbstickPressed = state.thumbstickPressed;
                    handler = (isRelease ? ThumbstickReleased : ThumbstickPressed);
                    break;
            }

            if ( FireEvents && handler != null )
            {
                var eh = handler;
                handler(this, new MotionControllerStateChangedEventArgs(_currentState)); 
            }
        }  

        bool FireEvents {
            get
            {
                return (attributesToTrack & TrackedControllerAttributesEnum.FireEvents) != 0; 

            }
        }

        private void InteractionManager_InteractionSourcePressed(InteractionSourcePressedEventArgs args)
        {             
            if (IsTarget(args.state))
            {
                UpdatePressed (args.pressType, args.state , false );
            }
        }

        private void InteractionManager_InteractionSourceReleased(InteractionSourceReleasedEventArgs args)
        {             
            if ( IsTarget(args.state)) 
            {
                UpdatePressed (args.pressType, args.state , true );
            }
        }

        public MotionControllerState GetState()
        {
            return _currentState; 
        }



        #region Controller Events 

         
        public event MotionControllerStateChangedEventHandler SelectPressed; 
        public event MotionControllerStateChangedEventHandler SelectReleased;
        public event MotionControllerStateChangedEventHandler GraspPressed;
        public event MotionControllerStateChangedEventHandler GraspReleased;
        public event MotionControllerStateChangedEventHandler MenuPressed;
        public event MotionControllerStateChangedEventHandler MenuReleased;
        public event MotionControllerStateChangedEventHandler ThumbstickPressed;
        public event MotionControllerStateChangedEventHandler ThumbstickReleased;
        public event MotionControllerStateChangedEventHandler TouchpadPressed;
        public event MotionControllerStateChangedEventHandler TouchpadReleased;

        #endregion
    }

} 