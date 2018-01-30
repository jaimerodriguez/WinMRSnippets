#define VERBOSE_STATE 

using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using UnityEngine.XR;
using UnityEngine.XR.WSA.Input;

using WinMRSnippets;
using WinMRSnippets.DebugHelpers;


namespace WinMRSnippets.Samples
{
    


    public class MotionControllerListener : MonoBehaviour, IControllerVisualizer
    {
        public GameObject cursor;
        public GameObject DevicePrefab;
        public Transform ControllersRoot;

        public PoseSource FilterInput = PoseSource.Grip;
        public MotionControllerInputAPI inputType = MotionControllerInputAPI.InteractionManagerPoll;
        public Material GLTFMaterial;
        public bool ShowDebugAxis = false;
        public bool AnimateControllerModel = true;
        private AxisRenderer axisRendererLeft, axisRendererRight;



        // Interaction Manager Input
        private Dictionary<uint, Transform> imDevices = new Dictionary<uint, Transform>();

        private Dictionary<uint, MotionControllerInfo> controllerInfoForAnimation = new Dictionary<uint, MotionControllerInfo>();


        // Use this for initialization
        void Start()
        {

            ControllerModelProvider.Instance.StartListening();
            if (inputType == MotionControllerInputAPI.InteractionManagerPoll)
            {
                SetupInteractionManagerPollingInput();
            }
            else
            {
                SetupInteractionManagerEventInput();
            }

        }


        void DestroyGameObject(MonoBehaviour bh)
        {
            if (bh != null && bh.gameObject != null)
            {
                Destroy(bh.gameObject);
                bh = null;
            }
        }

        private void OnDestroy()
        {
            ControllerModelProvider.Instance.StopListening();
        }

        // Update is called once per frame
        void Update()
        {
            if (inputType == MotionControllerInputAPI.InteractionManagerPoll)
                UpdateInteractionManagerPollingInput();
        }


        public void SetupInteractionManagerEventInput()
        {
            TraceHelper.Log("MotionControllerInput:SetupInteractionManagerEventInput()");
#if UNITY_5
        InteractionManager.SourceDetected += InteractionManager_SourceDetected;
        InteractionManager.SourceLost += InteractionManager_SourceLost;
        InteractionManager.SourceUpdated += InteractionManager_SourceUpdated;
        InteractionManager.SourcePressed += InteractionManager_SourcePressed;
#else
            InteractionManager.InteractionSourceDetected += InteractionManager_SourceDetected;
            InteractionManager.InteractionSourceLost += InteractionManager_SourceLost;
            InteractionManager.InteractionSourceUpdated += InteractionManager_SourceUpdated;
            InteractionManager.InteractionSourcePressed += InteractionManager_SourcePressed;
#endif

            // Add any already detected devices to the list
            foreach (var sourceState in InteractionManager.GetCurrentReading())
            {
                if (sourceState.source.supportsPointing)
                {
                    this.AddDevice(sourceState.source);
                }
            }
        }

        static int lastCount = 0;
        static int lastFrame = 0;
#if UNITY_5
    private void InteractionManager_SourceUpdated(InteractionManager.SourceEventArgs args)
#else
        private void InteractionManager_SourceUpdated(InteractionSourceUpdatedEventArgs args)
#endif
        {
            if (lastFrame == Time.frameCount)
            {
                lastCount++;
                TraceHelper.Log("Source updated " + lastCount + " for frame " + +Time.frameCount);
            }
            else
            {
                lastFrame = Time.frameCount;
                lastCount = 1;
            }

            this.UpdateInteractionSourceState(args.state);
        }

#if UNITY_5
    private void InteractionManager_SourcePressed(InteractionManager.SourceEventArgs args)
#else
        private void InteractionManager_SourcePressed(InteractionSourcePressedEventArgs args)
#endif
        {
            var state = args.state;
#if UNITY_5
        var pressed = state.pressed;
#else
            var pressed = state.GetPressedProperty();
#endif
            TraceHelper.Log(state.source.id + " controller " + pressed + " pressed");
        }



#if UNITY_5
    private void InteractionManager_SourceLost(InteractionManager.SourceEventArgs args)
#else
        private void InteractionManager_SourceLost(InteractionSourceLostEventArgs args)
#endif
        {
            var state = args.state;
            TraceHelper.Log("Source Lost:" + state.source.id);
            uint id = state.source.id;
            this.RemoveDevice(id);
        }


#if UNITY_5
    private void InteractionManager_SourceDetected(InteractionManager.SourceEventArgs args)

#else
        private void InteractionManager_SourceDetected(InteractionSourceDetectedEventArgs args)
#endif
        {
            var state = args.state;
            TraceHelper.Log("Source Detected:" + state.source.id);
            if (state.source.supportsPointing)
            {
                this.AddDevice(state.source);
            }
        }

        private void SetupInteractionManagerPollingInput()
        {
            TraceHelper.Log("MotionControllerInput:SetupInteractionManagerPollingInput()");
            var reading = InteractionManager.GetCurrentReading();

#if VERBOSE_STATE
            if (reading.Length == 0)
            {
                TraceHelper.LogModulus("no reading", TraceVariables.InteractionManagerLoop);
            }
#endif

            foreach (var sourceState in reading)
            {
                uint id = sourceState.source.id;
                if (id != 0 && sourceState.source.supportsPointing)
                {
                    this.AddDevice(sourceState.source);
                }
            }
        }

        private void UpdateInteractionManagerPollingInput()
        {
            var reading = InteractionManager.GetCurrentReading();
            if (reading.Length == 0)
            {
                TraceHelper.LogModulus("no reading", TraceVariables.InteractionManagerLoop);
            }

            foreach (var sourceState in reading)
            {
                uint id = sourceState.source.id;
                if (!this.imDevices.ContainsKey(id))
                {
                    if (id != 0 && sourceState.source.supportsPointing)
                    {

                        this.AddDevice(sourceState.source);
                    }
                }
                this.UpdateInteractionSourceState(sourceState);
            }
        }

        private void TearDownInteractionManagerPollingInput()
        {
            // Remove all devices from the list
            foreach (var sourceState in InteractionManager.GetCurrentReading())
            {
                uint id = sourceState.source.id;
                this.RemoveDevice(id);
            }
        }

    

        private void UpdateInteractionSourceState(InteractionSourceState sourceState)
        {
            uint id = sourceState.source.id;
            MotionControllerState state = InputState.Current.GetController(sourceState);
            if (imDevices.ContainsKey(id))
            {
                var sourcePose = sourceState.sourcePose;
                Vector3 position = Vector3.zero;
                Quaternion rotation = Quaternion.identity;
                Vector3 pointerPosition = Vector3.zero;
                Quaternion pointerRotation = Quaternion.identity;

#if UNITY_5
            bool hasRotation = sourcePose.TryGetRotation(out rotation);
            bool hasPosition = sourcePose.TryGetPosition(out position);
#else
                bool hasRotation = sourcePose.TryGetRotation(out rotation, InteractionSourceNode.Grip);
                bool hasPosition = sourcePose.TryGetPosition(out position, InteractionSourceNode.Grip);
#endif

#if UNITY_5
            bool hasPointerPosition = sourcePose.TryGetPointerPosition(out pointerPosition);
            bool hasPointerRotation = sourcePose.TryGetPointerRotation(out pointerRotation);
#else
                bool hasPointerPosition = sourcePose.TryGetPosition(out pointerPosition, InteractionSourceNode.Pointer);
                bool hasPointerRotation = sourcePose.TryGetRotation(out pointerRotation, InteractionSourceNode.Pointer);
#endif

                if (hasPosition || hasRotation || hasPointerPosition || hasPointerRotation)
                {

#if VALIDATE_INPUT
                    if (pointerPosition.IsDebugValid() && position.IsDebugValid() &&
                             pointerRotation.IsDebugValid() && rotation.IsDebugValid()
                            )
#endif
                        {
                            state.GripPosition = position;
                            state.GripRotation = rotation;

                            if ((hasPosition && hasRotation) && ((FilterInput & PoseSource.Grip) != PoseSource.None))
                            {
                                SetTransform(imDevices[id], position, rotation);
                            }

                            else if ((hasPointerPosition && hasPointerRotation) && ((FilterInput & PoseSource.Pointer) != PoseSource.None))
                            {
                                SetTransform(imDevices[id], pointerPosition, pointerRotation);
                            }


                            if (hasPointerPosition)
                            {
                                Vector3 forward;
    #if UNITY_5
                            Ray ray; 
                            if ( sourcePose.TryGetPointerRay(out ray))
                            {
                                forward = ray.direction;
                                pointerPosition = ray.origin; 
    #else
                                if (sourcePose.TryGetForward(out forward, InteractionSourceNode.Pointer))
                                {
    #endif
    #if VERBOSE_POSITION
                                TraceHelper.LogDiff(string.Format("Pos:{0}, Fwd:{1}",
                                     pointerPosition, forward), Cache.ForwardPosition);
    #endif
                                    if (cursor != null)
                                        cursor.transform.position = pointerPosition + forward;

                                }
                                else
                                {
                                    TraceHelper.Log("Failed get forward");
                                }
                            }
                        }
#if VALIDATE_INPUT
                        else 
                        {                     
                            TraceHelper.LogModulus(string.Format("Invalid: Pointer: {0}, Grip: {1} ", pointerPosition, position ), TraceVariables.InteractionManagerLoop );  
                        }
#endif
                }
                else
                {
                    TraceHelper.LogModulus(string.Format("No readings:{0}-- Grip: Pos-{1}, Rot-{2}, PointerPos-{3}, Rot-{4}",
#if UNITY_5
                    sourceState.source.sourceKind.ToString(),
#else
                    sourceState.source.kind.ToString(),
#endif
                        hasPosition, hasRotation,
                            hasPointerPosition, hasPointerRotation
                            ), TraceVariables.InteractionManagerLoop);
                }
                var rootproperties = sourceState;
#if UNITY_5
            state.SelectValue = (float)rootproperties.selectPressedValue;
            state.TouchPadXValue = (float)rootproperties.controllerProperties.touchpadX;
            state.TouchPadYValue = (float)rootproperties.controllerProperties.touchpadY;
            state.ThumbstickXValue = (float)rootproperties.controllerProperties.thumbstickX;
            state.ThumbstickYValue = (float)rootproperties.controllerProperties.thumbstickY;
            state.TouchPadPressed = rootproperties.controllerProperties.touchpadPressed;
            state.TouchPadTouched = rootproperties.controllerProperties.touchpadTouched;
            state.ThumbstickPressed = rootproperties.controllerProperties.thumbstickPressed;
#else
                state.SelectValue = rootproperties.selectPressedAmount;
                state.TouchPadXValue = rootproperties.touchpadPosition.x;
                state.TouchPadYValue = rootproperties.touchpadPosition.y;
                state.ThumbstickXValue = rootproperties.thumbstickPosition.x;
                state.ThumbstickYValue = rootproperties.thumbstickPosition.y;
                state.TouchPadPressed = rootproperties.touchpadPressed;
                state.TouchPadTouched = rootproperties.touchpadTouched;
                state.ThumbstickPressed = rootproperties.thumbstickPressed;
#endif

                state.SelectPressed = rootproperties.selectPressed;
                state.MenuPressed = rootproperties.menuPressed;
                state.GraspPressed = rootproperties.grasped;
                state.GripPosition = position;
                state.PointerPosition = pointerPosition;
                state.GripRotation = rotation;
                state.PointerRotation = pointerRotation;
                state.IsLeftHand = sourceState.source.handedness == InteractionSourceHandedness.Left;
                state.IsRightHand = sourceState.source.handedness == InteractionSourceHandedness.Right;

#if VERBOSE_STATE
                bool hasChanges, hasEvent;
                string traceState = state.GetTraceState(FilterInput, false, out hasChanges, out hasEvent);
                if (hasChanges || hasEvent)
                {
                    Debug.Log(traceState);
                }
#endif

                Vector3 pointerForward = Vector3.zero, gripForward = Vector3.zero;
                bool hasPointerForward = false, hasGripForward = false;
#if !UNITY_5
                if ((hasPointerForward = sourcePose.TryGetForward(out pointerForward, InteractionSourceNode.Pointer)) &&
                     (hasGripForward = sourcePose.TryGetForward(out gripForward, InteractionSourceNode.Grip)))
                {
#else
            Ray rayAxis; 
            if ( ( hasPointerForward = hasPointerPosition =  sourcePose.TryGetPointerRay ( out rayAxis)))
            {
                pointerForward = rayAxis.direction;
                pointerPosition = rayAxis.origin;
                hasGripForward = true; //it is a zero vector. 
#endif
                    if (ShowDebugAxis)
                    {
                        AxisRenderer axis = (state.IsLeftHand ? axisRendererLeft : axisRendererRight);
                        if (axis != null)
                        {
                            if (hasPointerForward && hasPointerPosition)
                                axis.SetWorldValues(state.PointerPosition, pointerForward, state.PointerRotation, AxisRenderer.ControllerElement.Pointer);
                            if (hasGripForward && hasPosition)
                                axis.SetWorldValues(state.GripPosition, gripForward, state.GripRotation, AxisRenderer.ControllerElement.Grip);
                        }
                    }
                }
#if VERBOSE_STATE
                else
                {
                    TraceHelper.LogModulus(string.Format("No Forward vectors. Grip: {0}, pointer:{1}",
                                hasPointerForward, hasGripForward), TraceVariables.InteractionManagerLoop);
                }
#endif


                if (AnimateControllerModel)
                {
                    MotionControllerInfo currentController;
                    if (controllerInfoForAnimation.TryGetValue(sourceState.source.id, out currentController))
                    {

#if !UNITY_5
                        currentController.AnimateSelect(sourceState.selectPressedAmount);
                        if (sourceState.source.supportsGrasp)
                        {
                            currentController.AnimateGrasp(sourceState.grasped);
                        }

                        if (sourceState.source.supportsMenu)
                        {
                            currentController.AnimateMenu(sourceState.menuPressed);
                        }
                        if (sourceState.source.supportsThumbstick)
                        {
                            currentController.AnimateThumbstick(sourceState.thumbstickPressed, sourceState.thumbstickPosition);
                        }

                        if (sourceState.source.supportsTouchpad)
                        {
                            currentController.AnimateTouchpad(sourceState.touchpadPressed, sourceState.touchpadTouched, sourceState.touchpadPosition);
                        }
#else

                    currentController.AnimateSelect( (float) sourceState.selectPressedValue);
                    if (sourceState.source.supportsGrasp)
                    {
                        currentController.AnimateGrasp(sourceState.grasped);
                    }

                    if (sourceState.source.supportsMenu)
                    {
                        currentController.AnimateMenu(sourceState.menuPressed);
                    }

                    InteractionController controller;
                    if (sourceState.source.TryGetController(out controller))
                    {
                        if (controller.hasThumbstick)
                        {
                            currentController.AnimateThumbstick(sourceState.controllerProperties.thumbstickPressed,
                                new Vector2((float)sourceState.controllerProperties.thumbstickX,
                                            (float) sourceState.controllerProperties.thumbstickY));

                        }

                        if (controller.hasTouchpad)
                        {
                            currentController.AnimateTouchpad(sourceState.controllerProperties.touchpadPressed, 
                                    sourceState.controllerProperties.touchpadTouched, 
                                    new Vector2( (float)sourceState.controllerProperties.touchpadX,
                                                 (float)sourceState.controllerProperties.touchpadX) );
                        }
                    } 

#endif
                    }
                    else
                        TraceHelper.Log("Could not animate " + sourceState.source.id);
                }

                if (state.ThumbstickPressed &&
                                    (Time.unscaledTime > (debounceThumbstickTime + 3f))
                   )
                {
                    ShowDebugAxis = !ShowDebugAxis;
                    UpdateDebugAxis(ShowDebugAxis);
                    debounceThumbstickTime = Time.unscaledTime;
                }
            }
        }

        float debounceThumbstickTime = 0f;


        private void AddDevice(InteractionSource source)
        {
            if (!imDevices.ContainsKey(source.id))
            {
                TraceHelper.Log("MotionControllerInput:AddDevice:" + source.id);
                CreateControllerVisual(source);
            }

        }

        void CreateControllerVisual(InteractionSource source)
        {
            GameObject go = new GameObject();
            go.transform.parent = ControllersRoot;
            go.name = "Controller" + source.id;
            imDevices[source.id] = go.transform;
            InputState.Current.DetectedControllers = this.imDevices.Keys.Count;
            var coroutine = StartCoroutine(Attach(go, ControllersRoot, source));
        }

        private IEnumerator Attach(GameObject target, Transform parent, InteractionSource source)
        {
            yield return ControllerHelpers.AttachModel(target, parent, source, GLTFMaterial, GLTFMaterial);

            if (AnimateControllerModel)
            {
                var newControllerInfo = new MotionControllerInfo() { };
                newControllerInfo.LoadInfo(target.GetComponentsInChildren<Transform>(), this);
                controllerInfoForAnimation.Add(source.id, newControllerInfo);
                TraceHelper.Log("Controller added for animation");
            }

            if (ShowDebugAxis)
            {
                if (source.handedness == InteractionSourceHandedness.Left)
                    axisRendererLeft = target.AddComponent<AxisRenderer>();
                else
                    axisRendererRight = target.AddComponent<AxisRenderer>();
            }

        }

        void UpdateDebugAxis(bool show)
        {

            int devices = imDevices.Count;
            int axisDevices = ((axisRendererRight == null) ? 0 : 1) + ((axisRendererLeft == null) ? 0 : 1);
            if (show && (devices == axisDevices))
            {
                return;
            }
            if (!show && axisDevices == 0)
                return;

            if (!show)
            {
                if (axisRendererLeft != null)
                {
                    Destroy(axisRendererLeft);
                    axisRendererLeft = null;
                }

                if (axisRendererRight != null)
                {
                    Destroy(axisRendererRight);
                    axisRendererRight = null;
                }
            }
            else
            {
                var states = InteractionManager.GetCurrentReading();
                List<uint> ids = new List<uint>();
                foreach (var state in states)
                {
                    uint id = state.source.id;
                    if (!ids.Contains(id))
                    {
                        Transform transform;
                        if (imDevices.TryGetValue(id, out transform))
                        {
                            GameObject target = transform.gameObject;
                            if (state.source.handedness == InteractionSourceHandedness.Left)
                            {
                                axisRendererLeft = target.AddComponent<AxisRenderer>();
                            }
                            else
                                axisRendererRight = target.AddComponent<AxisRenderer>();

                            ids.Add(id);
                        }
                    }
                }
            }
        }

        private void RemoveDevice(uint id)
        {
            if (imDevices.ContainsKey(id))
            {
                TraceHelper.Log("MotionControllerInput:RemoveDevice:" + id);
                Destroy(imDevices[id].gameObject);
                imDevices.Remove(id);
                controllerInfoForAnimation.Remove(id);
            }
        }


        private void SetTransform(Transform t, Vector3 position, Quaternion rotation)
        {
            t.localPosition = position;
            t.localRotation = rotation;
        }



        public GameObject SpawnTouchpadVisualizer(Transform parentTransform)
        {
            GameObject touchVisualizer;

            touchVisualizer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            touchVisualizer.transform.localScale = new Vector3(0.0025f, 0.0025f, 0.0025f);
            touchVisualizer.GetComponent<Renderer>().sharedMaterial = GLTFMaterial;
            Destroy(touchVisualizer.GetComponent<Collider>());
            touchVisualizer.transform.parent = parentTransform;
            touchVisualizer.transform.localPosition = Vector3.zero;
            touchVisualizer.transform.localRotation = Quaternion.identity;
            touchVisualizer.SetActive(false);
            return touchVisualizer;
        }
    }
} 