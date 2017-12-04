using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

#if UNITY_5
using UnityEngine.VR;
using UnityEngine.VR.WSA.Input;
#else
using UnityEngine.XR;
using UnityEngine.XR.WSA.Input;
#endif

public static class InputExtensions
{
    public static string GetPressedProperty(this InteractionSourceState state)
    {
        StringBuilder sb = new StringBuilder();

#if UNITY_5
        if (state.controllerProperties.thumbstickPressed)
#else
        if (state.thumbstickPressed)
#endif
        {
            sb.AppendFormat("{0} ({1})", Constants.ThumbStick,
#if UNITY_5
                new Vector2((float)state.controllerProperties.thumbstickX, (float)state.controllerProperties.thumbstickY).ToString());
#else
                state.thumbstickPosition.ToString());
#endif
        }
        if ( state.selectPressed )
        {
            sb.AppendFormat("{0} ({1})", Constants.Controller,
#if UNITY_5
                (float)state.selectPressedValue);
#else
                state.selectPressedAmount );
#endif
        }
        if ( state.menuPressed )
        {
            sb.Append(Constants.Menu); 
        }
#if UNITY_5
        if (state.controllerProperties.touchpadPressed)
#else
        if (state.touchpadPressed)
#endif
        {
            sb.Append(Constants.Touchpad); 
        }
         return sb.ToString();
    }



    public static string GetHandName(this InteractionSourceState state)
    {
         return state.source.handedness.ToString();
    }

    static ControllerState pastLeft;
    static ControllerState pastRight;
    static ControllerState diffState;

    static bool IsDifferent(Vector2 v1, Vector2 v2, float threshold)
    {
        Vector3 diff = v1 - v2;
        bool retVal = diff.magnitude > threshold;
        if (retVal)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("{0}-{1} > {2}", v1, v2, threshold));
        }
        return retVal;
    }

    static bool IsDifferent(float f1, float f2, float threshold)
    {
        var diff = Mathf.Abs(f1 - f2);
        bool retVal = (diff > threshold);                   
        return retVal;
    }

    static bool IsDifferent ( Vector3 v1, Vector3 v2 , float threshold )
    {
        Vector3 diff = v1 - v2;
        bool retVal= diff.magnitude > threshold;          
        if (retVal )
        {
            System.Diagnostics.Debug.WriteLine(string.Format("{0}-{1} > {2}", v1, v2, threshold)); 
        }
        return retVal;  
    }

    static bool IsDifferent ( bool b1 , bool b2 )
    {
        return b1 != b2; 
    }

    static bool HasContent (string s)
    {
        return s!= leftPrefix && s != rightPrefix; 
    }

    static string leftPrefix = "Left:";
    static string rightPrefix = "Right:"; 

    public static void TraceState(this ControllerState state, PoseSource filterInput, bool onlyDifferences = true)
    {
        StringBuilder sb = new StringBuilder();        
        bool allChanges = !onlyDifferences;
        float diffV3Threshold = 0.2f;
        float fThreshold = 0.02f;
        float diffV2Threshold = 0.14f;
        bool isEvent = false; 

        bool isDifferent = false; 
        if (state.IsLeftHand)
        {
            sb.Append( leftPrefix );
            diffState = pastLeft; 
        }
        else if (state.IsRightHand)
        {
            sb.Append(rightPrefix );
            diffState = pastRight; 
        } 

        if (state.SelectPressed)
        {
            isEvent = true; 
            sb.AppendFormat("Select Pressed ({0});", state.SelectValue);
        }
        if (state.TouchPadPressed || state.TouchPadTouched)
        {
            isEvent = true;
            sb.AppendFormat("TouchPad {0} {1} ({2},{3});",
                state.TouchPadPressed ? "Pressed" : "",
                state.TouchPadTouched ? "Touched" : "",
                state.TouchPadXValue, state.TouchPadYValue);
        }
        if (state.ThumbstickPressed)
        {
            isEvent = true;
            sb.AppendFormat("Thumbstick pressed({0},{1});", state.ThumbstickXValue, state.ThumbstickYValue);
        }
        if (state.MenuPressed)
        {
            isEvent = true;
            sb.Append("Menu Pressed;");
        }
        if (state.GraspPressed)
        {
            isEvent = true;
            sb.Append("Grasp Pressed;");
        }

        // Grip position  
        if ((filterInput & PoseSource.Grip) != PoseSource.None)
        {
            if (allChanges || (IsDifferent(state.Position, diffState.Position, diffV3Threshold)))
            {
                isDifferent = true;
                sb.AppendFormat("Pos:{0},Rot:{1}", state.Position, state.Rotation);
            } 
        }
        // Pointer position 
        if ((filterInput & PoseSource.Pointer) != PoseSource.None)
        {
            if (allChanges || (IsDifferent(state.PointerPosition, diffState.PointerPosition, diffV3Threshold)))
            {
                isDifferent = true; 
                sb.AppendFormat("PointPos:{0},PointRot:{1}", state.PointerPosition, state.PointerRotation);
            } 
        }

        if ( allChanges || (  IsDifferent (state.ThumbstickXValue, diffState.ThumbstickXValue, fThreshold ) || IsDifferent (state.ThumbstickYValue, diffState.ThumbstickYValue, fThreshold)))
        {
            isDifferent = true;
            sb.AppendFormat("Thumb:{0},{1}", state.ThumbstickXValue, state.ThumbstickYValue );
        }

        string message = sb.ToString();

        if (allChanges || HasContent(message))
        {
            if (isEvent)
            {
                //TODO: should we de-bounce based on time? 
                TraceHelper.LogDiff(message, TraceCacheGrouping.FullState);
            } 
            else
                TraceHelper.LogDiff(message, TraceCacheGrouping.FullState);
        } 
         
        if (isDifferent && !allChanges)
        {
            if (state.IsLeftHand)
            {
                pastLeft = state;
            }
            else if (state.IsRightHand)
            {
                pastRight = state;
            }
        } 
    }



    public static void TraceState(this GamepadState state, bool onlyDifferences = true)
    {
        StringBuilder sb = new StringBuilder();
        bool allChanges = !onlyDifferences;
        
        bool isDifferent = false;

        sb.Append("Gamepad:");  

        if ( state.AButtonPressed )
        {
            sb.Append("A-Pressed");
            isDifferent = true; 
        }

        if (state.BButtonPressed)
        {
            sb.Append("B-Pressed");
            isDifferent = true;
        }

        if (state.XButtonPressed)
        {
            sb.Append("X-Pressed");
            isDifferent = true;
        }

        if (state.YButtonPressed)
        {
            sb.Append("Y-Pressed");
            isDifferent = true;
        }

        if (state.BackButtonPressed)
        {
            sb.Append("Back-Pressed");
            isDifferent = true;
        }

        if (state.StartButtonPressed)
        {
            sb.Append("Start-Pressed");
            isDifferent = true;
        }

        if (state.LeftBumperPressed)
        {
            sb.Append("LB:Pressed");
            isDifferent = true;
        }

        if (state.RightBumperPressed)
        {
            sb.Append("RB:Pressed");
            isDifferent = true;
        }

        if (state.LeftStickClicked)
        {
            sb.Append("LS:Pressed");
            isDifferent = true;
        }

        if (state.RightStickClicked)
        {
            sb.Append("RS:Pressed");
            isDifferent = true;
        }

        if (state.LeftStickX != 0f || state.LeftStickY != 0f )
        {
            sb.AppendFormat("LS ({0},{1})", state.LeftStickX, state.LeftStickY); 
            isDifferent = true;
        }

        if (state.RightStickX != 0f || state.RightStickY != 0f)
        {
            sb.AppendFormat("RS ({0},{1})", state.RightStickX, state.RightStickY);
            isDifferent = true;
        }

        if (state.DPadX != 0f || state.DPadY != 0f)
        {
            sb.AppendFormat("DPad ({0},{1})", state.DPadX, state.DPadY);
            isDifferent = true;
        }

        if (state.LeftTrigger != 0f || state.RightTrigger != 0f)
        {
            sb.AppendFormat("Trigger ({0},{1})", state.LeftTrigger , state.RightTrigger );
            isDifferent = true;
        }


        if ( isDifferent || allChanges  )
        {
            TraceHelper.LogDiff (sb.ToString(), TraceCacheGrouping.XboxController ); 
        } 
    }


    static int debugVectorCount = 0;
    static int debugQuatCount = 0; 
    public static bool IsDebugValid  ( this Vector3 v )
    {

        bool hasNan =
             float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z); 
        
        if ( !hasNan )
        {
            return Mathf.Abs(v.x) < 20f && Mathf.Abs(v.y) < 20f &&  Mathf.Abs(v.z) < 20f; 
        }

        TraceHelper.Log("Invalid Vector" + debugVectorCount++); 
        return false; 
    }

    public static bool IsDebugValid(this Quaternion q)
    {
        bool hasNan =
             float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z) || float.IsNaN(q.w);

        if ( hasNan )
        {
            TraceHelper.Log( string.Format ( "Invalid Rotation {0}: {1} ", 
                        debugQuatCount, q ));
        }
        return !hasNan; 

    }
}