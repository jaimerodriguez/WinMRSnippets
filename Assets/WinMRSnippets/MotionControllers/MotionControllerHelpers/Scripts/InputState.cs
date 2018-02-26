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
        public Vector3 Velocity; 


        public void ResetDynamicState()
        {
            //ANalog properties 
            TouchpadPosition = Vector2.zero;
            ThumbStickPosition = Vector2.zero;
            SelectValue = 0f;


            //POSE 
            GripPosition = Vector3.zero;
            GripRotation = Quaternion.identity;
            PointerPosition = Vector3.zero;
            PointerRotation = Quaternion.identity;
            PointerForward = Vector3.zero;
            GripForward = Vector3.zero;
            AngularVelocity = Vector3.zero;
            Velocity = Vector3.zero; 

            //Button Pressses          
            SelectPressed = false;
            MenuPressed = false;
            GraspPressed = false;
            TouchPadPressed = false;
            TouchPadTouched = false;
            ThumbstickPressed = false;

        }

        public void Reset ( uint defaultId, ushort defaultVendorId = 0, ushort defaultProductVersion = 0)
        {
            ResetDynamicState();
            ResetStaticProperties( defaultId , defaultVendorId, defaultProductVersion ); 
        }

        private void ResetStaticProperties ( uint defaultId , ushort defaultVendorId  , ushort defaultProductVersion  )
        {
            SupportsGrasp = false ;
            SupportsMenu =  false ;
            SupportsPointing = false ;
            SupportsTouchpad = false ;
            SupportsThumbstick = false ;
            VendorId = defaultVendorId  ;
            ProductVersion = defaultProductVersion ;
            Id = defaultId ;
            IsLeftHand = false ;
            IsRightHand = false ;
        }
        public void InitStaticProperties (InteractionSource source)
        {
            SupportsGrasp = source.supportsGrasp;
            SupportsMenu = source.supportsMenu;
            SupportsPointing = source.supportsPointing;
            SupportsTouchpad = source.supportsTouchpad;
            SupportsThumbstick = source.supportsThumbstick;
            VendorId = source.vendorId;
            ProductVersion = source.productVersion;
            Id = source.id;
            IsLeftHand = source.handedness == InteractionSourceHandedness.Left;
            IsRightHand = source.handedness == InteractionSourceHandedness.Right;
        }



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
 


