using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

#if UNITY_5
using UnityEngine.VR;
using UnityEngine.VR.WSA.Input;
#else
using UnityEngine.XR;
using UnityEngine.XR.WSA.Input;
#endif


public class Constants
{
    public static string ThumbStick = "ThumbStick";
    public static string Controller = "Controller";
    public static string Menu = "Menu";
    public static string Touchpad = "Touchpad";
    public static string LeftHandness = "left";
    public static string RightHandeness = "right";
    public static string GLTFFileExtension = ".gltf";
    public static string ControllerFilePrefix = "controller" ; 
}


public class UnityInputAxis
{
    public const string MotionController_SelectPressedValue_Left = "MotionController-SelectPressedValue-Left";
    public const string MotionController_SelectPressedValue_Right = "MotionController-SelectPressedValue-Right";

    public const string MotionController_TouchpadY_Left = "MotionController-TouchpadY-Left";
    public const string MotionController_TouchpadY_Right = "MotionController-TouchpadY-Right";
    public const string MotionController_TouchpadX_Left = "MotionController-TouchpadX-Left";
    public const string MotionController_TouchpadX_Right = "MotionController-TouchpadX-Right";


    public const string MotionController_ThumbstickY_Left = "MotionController-ThumbstickY-Left";
    public const string MotionController_ThumbstickY_Right = "MotionController-ThumbstickY-Right";
    public const string MotionController_ThumbstickX_Left = "MotionController-ThumbstickX-Left";
    public const string MotionController_ThumbstickX_Right = "MotionController-ThumbstickX-Right";

    public const string MotionController_Select_Left = "MotionController-Select-Left";
    public const string MotionController_Select_Right = "MotionController-Select-Right";

    public const string MotionController_Menu_Left = "MotionController-Menu-Left";
    public const string MotionController_Menu_Right = "MotionController-Menu-Right";

    public const string MotionController_ThumbstickPressed_Left = "MotionController-ThumbstickPressed-Left";
    public const string MotionController_ThumbstickPressed_Right = "MotionController-ThumbstickPressed-Right";

    public const string MotionController_Grasp_Left = "MotionController-Grasp-Left";
    public const string MotionController_Grasp_Right = "MotionController-Grasp-Right";


    public const string MotionController_TouchpadPressed_Left = "MotionController-TouchpadPressed-Left";
    public const string MotionController_TouchpadPressed_Right = "MotionController-TouchpadPressed-Right";


    public const string MotionController_TouchpadTouched_Left = "MotionController-TouchpadTouched-Left";
    public const string MotionController_TouchpadTouched_Right = "MotionController-TouchpadTouched-Right";


    public const string XboxController_AButton = "Xbox-A";
    public const string XboxController_BButton = "Xbox-B";
    public const string XboxController_XButton = "Xbox-X";
    public const string XboxController_YButton = "Xbox-Y";

    public const string XboxController_BackButton = "Xbox-View";
    public const string XboxController_StartButton = "Xbox-Menu";
    // public const string XboxController_XboxButton = "Unused ";

    public const string XboxController_LeftBumper = "Xbox-LB";
    public const string XboxController_RightBumper = "Xbox-RB";

    public const string XboxController_LeftStickButton = "Xbox-LS";
    public const string XboxController_RightStickButton = "Xbox-RS";


    public const string XboxController_LeftStickX = "Xbox-LeftStickX";
    public const string XboxController_LeftStickY = "Xbox-LeftStickY";

    public const string XboxController_RightStickX = "Xbox-RightStickX";
    public const string XboxController_RightStickY = "Xbox-RightStickY";


    public const string XboxController_Trigger = "Xbox-TriggerAxis";

    public const string XboxController_DPadX = "Xbox-DPADX"; 
    public const string XboxController_DPadY = "Xbox-DPADY"; 
    
}