using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_5
using UnityEngine.VR;
using UnityEngine.VR.WSA.Input;
using XRNode = UnityEngine.VR.VRNode; 
#else
using UnityEngine.XR;
using UnityEngine.XR.WSA.Input;
#endif

public enum InputType { UnityInput, InteractionManagerPoll, InteractionManagerEvent };



public struct ControllerState
{
    public float SelectValue;     
    public float TouchPadXValue;     
    public float TouchPadYValue;
    public float ThumbstickXValue;
    public float ThumbstickYValue; 
    public bool SelectPressed;    
    public bool MenuPressed;     
    public bool GraspPressed;  
    public bool  TouchPadPressed;     
    public bool  TouchPadTouched; 
    public bool  ThumbstickPressed;

    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 PointerPosition;
    public Quaternion PointerRotation;
    ///
    public bool SupportsGrasp;
    public bool SupportsMenu;
    public bool SupportsPointing;
    public bool SupportsThumbstick;
    public bool SupportsTouchpad;
    public ushort VendorId;
    public ushort ProductId;
    public ushort ProductVersion;
    public uint Id;
    public bool IsLeftHand;
    public bool IsRightHand; 
}


public struct InputState
{
    public int DetectedControllers;
    public Dictionary<string, ControllerState> Controllers;
    public static InputState Current;
    public ControllerState Left;
    public ControllerState Right;


 
    public ControllerState GetController( InteractionSourceState interaction , bool preserveState = false )
    {
        if (interaction.source.handedness == InteractionSourceHandedness.Left)
        {
            if ( !preserveState )
            {
                Left = new ControllerState(); 
            }
             
            return Left;
        } 
        else
        {
            Debug.Assert(interaction.source.handedness == InteractionSourceHandedness.Right,  "Right hadnedness should be default/fallback");
            if (!preserveState)
            {
                Right = new ControllerState();
            }             
            return Right;
        }
    }

    internal ControllerState GetController(XRNode nodeType, bool preserveState = false )
    {
        
        if (nodeType == XRNode.LeftHand)
        {
            if (!preserveState )
            {
                Left = new ControllerState();
                Left.IsLeftHand = true; 
            }
            return Left;
        }
        else
        {
            if (!preserveState)
            {
                Right = new ControllerState();
                Right.IsRightHand = true;  
            }
            return Right ;
        }
    }
}


 


