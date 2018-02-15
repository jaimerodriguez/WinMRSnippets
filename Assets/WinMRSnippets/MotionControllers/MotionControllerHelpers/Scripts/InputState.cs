using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.WSA.Input;


namespace WinMRSnippets
{
    public enum MotionControllerInputAPI { UnityInput, InteractionManagerPoll, InteractionManagerEvent };

    public struct MotionControllerState
    {
        
        public float SelectValue;

        public Vector2 TouchpadPosition;        
        public Vector2 ThumbStickPosition;

        public bool SelectPressed;
        public bool MenuPressed;
        public bool GraspPressed;
        public bool TouchPadPressed;
        public bool TouchPadTouched;
        public bool ThumbstickPressed;


        public Vector3 GripPosition;
        public Quaternion GripRotation;
        public Vector3 PointerPosition;
        public Quaternion PointerRotation;
        public Vector3 PointerForward;
        public Vector3 GripForward; 

        //
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

        public Vector3 AngularVelocity; 

    }


    public struct InputState
    {
        public int DetectedControllers;
        public Dictionary<string, MotionControllerState> Controllers;
        public static InputState Current;
        public MotionControllerState LeftController;
        public MotionControllerState RightController;

        public MotionControllerState GetController(InteractionSourceState interaction, bool preserveState = false)
        {
            if (interaction.source.handedness == InteractionSourceHandedness.Left)
            {
                if (!preserveState)
                {
                    LeftController = new MotionControllerState();
                }

                return LeftController;
            }
            else
            {
                Debug.Assert(interaction.source.handedness == InteractionSourceHandedness.Right, "Right hadnedness should be default/fallback");
                if (!preserveState)
                {
                    RightController = new MotionControllerState();
                }
                return RightController;
            }
        }

        internal MotionControllerState GetController(XRNode nodeType, bool preserveState = false)
        {

            if (nodeType == XRNode.LeftHand)
            {
                if (!preserveState)
                {
                    LeftController = new MotionControllerState();
                    LeftController.IsLeftHand = true;
                }
                return LeftController;
            }
            else
            {
                if (!preserveState)
                {
                    RightController = new MotionControllerState();
                    RightController.IsRightHand = true;
                }
                return RightController;
            }
        }
    }
} 
 


