
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

using UnityEngine.XR;
using UnityEngine.XR.WSA.Input;

namespace WinMRSnippets
{
    /// <summary>
    /// Used to filter sources of MotionControllerInput 
    /// </summary>     
    public enum PoseSource
    {
        None = 0,
        Grip = 1,
        Pointer = 2,
        Any =  Grip | Pointer 
    };


    public static class InputExtensions
    {
        public static string GetPressedProperty(this InteractionSourceState state)
        {
            StringBuilder sb = new StringBuilder();

            if (state.thumbstickPressed)
            {
                sb.AppendFormat("{0} ({1})", Constants.ThumbStick, state.thumbstickPosition.ToString());
            }

            if (state.selectPressed)
            {
                sb.AppendFormat("{0} ({1})", Constants.Controller, state.selectPressedAmount);
            }
            if (state.menuPressed)
            {
                sb.Append(Constants.Menu);
            }
            if (state.touchpadPressed)

            {
                sb.Append(Constants.Touchpad);
            }
            return sb.ToString();
        }


        public static string GetHandName(this InteractionSourceState state)
        {
            return state.source.handedness.ToString();
        }

        static MotionControllerState pastLeft;
        static MotionControllerState pastRight;
        static MotionControllerState diffState;

        static bool IsDifferent(Vector2 v1, Vector2 v2, float threshold)
        {
            Vector3 diff = v1 - v2;
            bool retVal = diff.magnitude > threshold;
            return retVal;
        }

        static bool IsDifferent(float f1, float f2, float threshold)
        {
            var diff = Mathf.Abs(f1 - f2);
            bool retVal = (diff > threshold);
            return retVal;
        }

        static bool IsDifferent(Vector3 v1, Vector3 v2, float threshold)
        {
            Vector3 diff = v1 - v2;
            bool retVal = diff.magnitude > threshold;
            return retVal;
        }

        static bool IsDifferent(bool b1, bool b2)
        {
            return b1 != b2;
        }

        static bool HasContent(string s)
        {
            return s != leftPrefix && s != rightPrefix;
        }

        static string leftPrefix = "Left:";
        static string rightPrefix = "Right:";


        public static string GetTraceState(this MotionControllerState state, PoseSource filterInput, bool allChanges,  out bool hasNonDefaultValue , out bool hasEvent )
        {
            StringBuilder sb = new StringBuilder();
            hasNonDefaultValue = false;
            hasEvent = false;
             
            float vector3ThresholdForDifference = 0.2f;
            float floatThresholdForDifference = 0.02f;
                         
            if (state.IsLeftHand)
            {
                sb.Append(leftPrefix);
                diffState = pastLeft;
            }
            else if (state.IsRightHand)
            {
                sb.Append(rightPrefix);
                diffState = pastRight;
            }

            if (state.SelectPressed)
            {
                hasEvent = true;
                sb.AppendFormat("Select Pressed ({0});", state.SelectValue);
            }

            if (state.TouchPadPressed || state.TouchPadTouched)
            {
                hasEvent = true;
                sb.AppendFormat("TouchPad {0} {1} ({2},{3});",
                    state.TouchPadPressed ? "Pressed" : "",
                    state.TouchPadTouched ? "Touched" : "",
                    state.TouchPadXValue, state.TouchPadYValue);
            }
            if (state.ThumbstickPressed)
            {
                hasEvent = true;
                sb.AppendFormat("Thumbstick pressed({0},{1});", state.ThumbstickXValue, state.ThumbstickYValue);
            }
            if (state.MenuPressed)
            {
                hasEvent = true;
                sb.Append("Menu Pressed;");
            }
            if (state.GraspPressed)
            {
                hasEvent = true;
                sb.Append("Grasp Pressed;");
            }

            // Grip position  
            if ((filterInput & PoseSource.Grip) != PoseSource.None)
            {
                if (allChanges || (IsDifferent(state.GripPosition, diffState.GripPosition, vector3ThresholdForDifference)))
                {
                    hasNonDefaultValue = true;
                    sb.AppendFormat("Pos:{0},Rot:{1}", state.GripPosition, state.GripRotation);
                }
            }
            // Pointer position 
            if ((filterInput & PoseSource.Pointer) != PoseSource.None)
            {
                if (allChanges || (IsDifferent(state.PointerPosition, diffState.PointerPosition, vector3ThresholdForDifference)))
                {
                    hasNonDefaultValue = true;
                    sb.AppendFormat("PointPos:{0},PointRot:{1}", state.PointerPosition, state.PointerRotation);
                }
            }

            if (allChanges || (IsDifferent(state.ThumbstickXValue, diffState.ThumbstickXValue, floatThresholdForDifference) || IsDifferent(state.ThumbstickYValue, diffState.ThumbstickYValue, floatThresholdForDifference)))
            {
                hasNonDefaultValue = true;
                sb.AppendFormat("Thumb:{0},{1}", state.ThumbstickXValue, state.ThumbstickYValue);
            }

            string message = sb.ToString();

#if TRACING_VERBOSE
            if (hasNonDefaultValue || HasContent(message))
            {
                Debug.Log(message);
            }
#endif 
            
            if (state.IsLeftHand)
            {
                pastLeft = state;
            }
            else if (state.IsRightHand)
            {
                pastRight = state;
            }
             
            return message;
        }



        public static string GetTraceState(this GamepadState state, out bool hasNonDefaultValue)
        {
            StringBuilder sb = new StringBuilder();
            hasNonDefaultValue = false;
            sb.Append("Gamepad:");

            if (state.AButtonPressed)
            {
                sb.Append("A-Pressed");
                hasNonDefaultValue = true;
            }

            if (state.BButtonPressed)
            {
                sb.Append("B-Pressed");
                hasNonDefaultValue = true;
            }

            if (state.XButtonPressed)
            {
                sb.Append("X-Pressed");
                hasNonDefaultValue = true;
            }

            if (state.YButtonPressed)
            {
                sb.Append("Y-Pressed");
                hasNonDefaultValue = true;
            }

            if (state.BackButtonPressed)
            {
                sb.Append("Back-Pressed");
                hasNonDefaultValue = true;
            }

            if (state.StartButtonPressed)
            {
                sb.Append("Start-Pressed");
                hasNonDefaultValue = true;
            }

            if (state.LeftBumperPressed)
            {
                sb.Append("LB:Pressed");
                hasNonDefaultValue = true;
            }

            if (state.RightBumperPressed)
            {
                sb.Append("RB:Pressed");
                hasNonDefaultValue = true;
            }

            if (state.LeftStickClicked)
            {
                sb.Append("LS:Pressed");
                hasNonDefaultValue = true;
            }

            if (state.RightStickClicked)
            {
                sb.Append("RS:Pressed");
                hasNonDefaultValue = true;
            }

            if (state.LeftStickX != 0f || state.LeftStickY != 0f)
            {
                sb.AppendFormat("LS ({0},{1})", state.LeftStickX, state.LeftStickY);
                hasNonDefaultValue = true;
            }

            if (state.RightStickX != 0f || state.RightStickY != 0f)
            {
                sb.AppendFormat("RS ({0},{1})", state.RightStickX, state.RightStickY);
                hasNonDefaultValue = true;
            }

            if (state.DPadX != 0f || state.DPadY != 0f)
            {
                sb.AppendFormat("DPad ({0},{1})", state.DPadX, state.DPadY);
                hasNonDefaultValue = true;
            }

            if (state.LeftTrigger != 0f || state.RightTrigger != 0f)
            {
                sb.AppendFormat("Trigger ({0},{1})", state.LeftTrigger, state.RightTrigger);
                hasNonDefaultValue = true;
            }

            if (hasNonDefaultValue)
                return sb.ToString();
            else
                return string.Empty;
        }



  
    }
} 

public static class UnityTypesExtensions
{
    public static bool IsDebugValid(this Vector3 v)
    {

        bool hasNan = float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z);

        if (!hasNan)
        {
            return Mathf.Abs(v.x) < 20f && Mathf.Abs(v.y) < 20f && Mathf.Abs(v.z) < 20f;
        }
        return false;
    }

    public static bool IsDebugValid(this Quaternion q)
    {
        bool hasNan =
             float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z) || float.IsNaN(q.w);

        if (hasNan)
        {
#if TRACING_VERBOSE
            Debug.Log( string.Format ( "Invalid Rotation {0}: {1} ",  debugQuatCount, q ));
#endif
        }
        return !hasNan;

    }
}
