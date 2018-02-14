﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.WSA.Input;

using WinMRSnippets;
using WinMRSnippets.DebugHelpers; 


public class UnityInputListener : MonoBehaviour {

    private Dictionary<string, XRNode> trackedControllers = new Dictionary<string, XRNode>();
    private Dictionary<XRNode, Transform> uDevices = new Dictionary<XRNode, Transform>();
    public Transform ControllersRoot;
    public Material GLTFMaterial;
    public bool ShowDebugAxis = false;

    private DebugAxisRenderer axisRendererLeft, axisRendererRight;

    // Use this for initialization
    void Start()
    {
        ControllerModelProvider.Instance.StartListening();
        Setup(); 
    }
    private void Setup()
    {
        Debug.Log("UnityInput_ControllerVisualizer.Setup");
        AddSourceChangedListeners(); 
 
        UpdateTrackedControllers();
    }

    void AddSourceChangedListeners ()
    {
        InputTracking.nodeAdded += InputTracking_nodeAdded;
        InputTracking.nodeRemoved += InputTracking_nodeRemoved;
    }


    private void InputTracking_nodeRemoved(XRNodeState obj)
    {
        Debug.Log("node removed: " + obj.uniqueID +   "-" + InputTracking.GetNodeName(obj.uniqueID));
        RemoveDevice(InputTracking.GetNodeName(obj.uniqueID));
    }

    private void InputTracking_nodeAdded(XRNodeState obj)
    { 
        AddDevice(InputTracking.GetNodeName(obj.uniqueID)); 
    }


    void TraceJoystickNames(string[] names)
    {
        StringBuilder b = new StringBuilder();
        b.Append("joystics:"); 
        foreach (var name in names)
            b.Append(" " + name + ",");

        TraceHelper.LogDiff(b.ToString(), TraceCacheGrouping.Joysticks); 
    }


    private void UpdateTrackedControllers()
    {
        var joysticks = Input.GetJoystickNames();
        TraceJoystickNames(joysticks); 
        var detectedControllers = new List<string>(joysticks);
        
        // compare detected with tracked
        foreach (var trackedContrller in this.trackedControllers.Keys)
        {
            if (detectedControllers.Contains(trackedContrller))
            {
                detectedControllers.Remove(trackedContrller);
            }
            else
            {
                RemoveDevice(trackedContrller);
            }
        }

        // add any new controllers
        foreach (var name in detectedControllers)
        {
            TraceHelper.LogModulus  ("detected:" + name , TraceVariables.UnityLoop );             
            {
                AddDevice(name);
            }            
        }
    }

    private void UpdateUnityInput()
    {
        
        UpdateTrackedControllers();

        var left = InputState.Current.GetController(XRNode.LeftHand);
        var right = InputState.Current.GetController(XRNode.RightHand); 
        // Motion
        foreach (XRNode nodeType in uDevices.Keys)
        {
            MotionControllerState state; 
            if (nodeType == XRNode.LeftHand)
                state = left;
            else if (nodeType == XRNode.RightHand)
                state = right;
            else
            {
                TraceHelper.Log("Ignoring nodetype" + nodeType); 
                continue;

            }
            var position = InputTracking.GetLocalPosition(nodeType);            
            var rotation = InputTracking.GetLocalRotation(nodeType);
            
            SetTransform(uDevices[nodeType], position, rotation);

            if (ShowDebugAxis)
            {
                DebugAxisRenderer axis = null;
                var angles = rotation.eulerAngles; 

                axis = (nodeType == XRNode.RightHand) ? axisRendererRight : axisRendererLeft;
                if (axis != null)
                {                   
                    axis.SetWorldValues(position,  (rotation * Vector3.forward), rotation, DebugAxisRenderer.ControllerElement.Grip);
                } 

                if (axis != null)
                {
                    position.y += .05f; 
                    axis.SetWorldValues(position, angles, rotation, DebugAxisRenderer.ControllerElement.Pointer );
                }
            } 

            state.GripPosition = position;
            state.GripRotation = rotation;              
        }
       
        // Buttons
        if (Input.GetButtonDown( UnityInputAxis.MotionController_Select_Left))
        {          
            left.SelectPressed = true; 
        }
        if (Input.GetButtonDown(UnityInputAxis.MotionController_Select_Right))
        {          
            right.SelectPressed = true;
        }
        if (Input.GetButtonDown(UnityInputAxis.MotionController_Menu_Left))
        {          
            left.MenuPressed = true;
        }
        if (Input.GetButtonDown(UnityInputAxis.MotionController_Menu_Right))
        {           
            right.MenuPressed = true;
        }
        if (Input.GetButtonDown(UnityInputAxis.MotionController_Grasp_Left))
        {          
            left.GraspPressed = true;
        }
        if (Input.GetButtonDown(UnityInputAxis.MotionController_Grasp_Right))
        {          
            right.GraspPressed = true;
        }
        if (Input.GetButtonDown(UnityInputAxis.MotionController_TouchpadTouched_Left))
        {           
            left.TouchPadTouched= true;
        }
        if (Input.GetButtonDown(UnityInputAxis.MotionController_TouchpadTouched_Right))
        {          
            right.TouchPadTouched = true;
        }
        if (Input.GetButtonDown(UnityInputAxis.MotionController_TouchpadPressed_Left))
        {            
            left.TouchPadPressed = true;
        }
        if (Input.GetButtonDown(UnityInputAxis.MotionController_TouchpadPressed_Right))
        {             
            right.TouchPadPressed = true;
        }
        if (Input.GetButtonDown(UnityInputAxis.MotionController_ThumbstickPressed_Left))
        {          
            left.ThumbstickPressed = true;
        }
        if (Input.GetButtonDown(UnityInputAxis.MotionController_ThumbstickPressed_Right))
        {          
            right.ThumbstickPressed = true;
        }

        // Axes
        
        left.SelectValue = Input.GetAxis( UnityInputAxis.MotionController_SelectPressedValue_Left);
        right.SelectValue = Input.GetAxis(UnityInputAxis.MotionController_SelectPressedValue_Right);
        left.TouchpadPosition = new Vector2(Input.GetAxis(UnityInputAxis.MotionController_TouchpadX_Left), Input.GetAxis(UnityInputAxis.MotionController_TouchpadY_Left));
        right.TouchpadPosition = new Vector2(Input.GetAxis(UnityInputAxis.MotionController_TouchpadX_Right), Input.GetAxis(UnityInputAxis.MotionController_TouchpadY_Right));

        left.ThumbStickPosition = new Vector2( Input.GetAxis(UnityInputAxis.MotionController_ThumbstickX_Left), Input.GetAxis(UnityInputAxis.MotionController_ThumbstickY_Left));

        right.ThumbStickPosition = new Vector2(Input.GetAxis(UnityInputAxis.MotionController_ThumbstickX_Right), Input.GetAxis(UnityInputAxis.MotionController_ThumbstickY_Right));         

#if TRACING_VERBOSE
        right.TraceState(PoseSource.Any);
        left.TraceState(PoseSource.Any); 
#endif 

    }

    private void TearDownUnityInput()
    {
        foreach (var controllerName in Input.GetJoystickNames())
        {
            if (trackedControllers.ContainsKey(controllerName))
            {
                RemoveDevice(controllerName);
            }
        }
    }

    private void AddDevice(string name )
    {
        if ( !name.Contains("Spatial Controller")) // Motion Controllers
        {             
            TraceHelper.Log ("Ignoring non-Spatial controller:" + name );
            return;  
        }
#if VERBOSE_STATE
        else
        {
            TraceHelper.Log("Adding " + name); 
        }
#endif

        XRNode? nodeType = null;

        if (name.Contains("Left"))
        {
            nodeType = XRNode.LeftHand;
        }
        else if (name.Contains("Right"))
        {
            nodeType = XRNode.RightHand;
        }

        if (nodeType.HasValue)
        {
            if (uDevices.ContainsKey(nodeType.Value))
            {
                return; 
            }
            else // (!uDevices.ContainsKey(nodeType.Value))
            {
                CreateControllerVisual(nodeType.Value);
            }
        }

             
    }

    void CreateControllerVisual(XRNode nodeType )
    {
#if UNITY_5
        var position = InputTracking.GetLocalPosition(nodeType);
        if (position == Vector3.zero)
        {
            TraceHelper.LogDiff ("Not creating controller for " + nodeType.ToString(), TraceCacheGrouping.IgnoreController ); 
            return; 
        }
#endif

        GameObject go = new GameObject();
        go.transform.parent = ControllersRoot;
        go.name = "Controller" + nodeType.ToString();
        uDevices[nodeType] = go.transform;         
        var coroutine = StartCoroutine(Attach(go, ControllersRoot, nodeType ));
    }



    private IEnumerator Attach(GameObject target, Transform parent, XRNode nodeType )
    {

        yield return ControllerHelpers.AttachModel(target, parent, nodeType, GLTFMaterial, GLTFMaterial);
        if (ShowDebugAxis)
        {
            if (nodeType == XRNode.LeftHand)
                axisRendererLeft = target.AddComponent<DebugAxisRenderer>();
            else
                axisRendererRight = target.AddComponent<DebugAxisRenderer>();
        }
    }


    private void RemoveDevice(string name)
    {
        if (this.trackedControllers.ContainsKey(name))
        {
            XRNode nodeType = this.trackedControllers[name];
            if (uDevices.ContainsKey(nodeType))
            {
                Destroy(uDevices[nodeType].gameObject);
                uDevices.Remove(nodeType);
                Debug.Log("Removed device " + name);
            }
            else
            {
                Debug.Log("Tracked controller: " + name + " not on devices list!");
            }

            trackedControllers.Remove(name);
        }

        InputState.Current.DetectedControllers = this.uDevices.Keys.Count;
    }

    private void SetTransform(Transform t, Vector3 position, Quaternion rotation)
    {
        t.localPosition = position;
        t.localRotation = rotation;
    }


    // Update is called once per frame
    void Update () {
        UpdateUnityInput(); 
	}



   
}
