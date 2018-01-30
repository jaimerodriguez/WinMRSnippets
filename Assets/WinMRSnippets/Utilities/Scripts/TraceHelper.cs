using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace WinMRSnippets.DebugHelpers
{

    public enum TraceCacheGrouping : int
    {
        Grip,
        Pointer,
        LastPosition,
        FullState,
        Joysticks,
        IgnoreController,
        ForwardPosition,
        AxisInfo,
        ControllerModel,
        XboxController,
        TrackingState,
        LifeCycle,
        All
    }

    public class TraceHelper
    {
        //   [System.Diagnostics.Conditional("DEBUG")]
        public static void Log(string message)
        {
            Debug.Log(message);

        }

        //   [System.Diagnostics.Conditional("DEBUG")]
        public static void LogModulus(string message, TraceFrequency modulus)
        {
            if ((modulus.Count++ % modulus.Modulus) == 0)
            {
                Log(message);
            }
        }

        //  [System.Diagnostics.Conditional("DEBUG")]
        public static void LogDiff(string message, TraceCacheGrouping i, float timeOut = 0f)
        {
            int index = (int)i;
            string cached = cachedStrings[index];
            if (string.Compare(cached, message) != 0)
            {
                Log(message);
                cachedStrings[index] = message;
            }
        }


        //  [System.Diagnostics.Conditional("DEBUG")]
        public static void LogDiffModulus(string message, TraceCacheGrouping i, TraceFrequency modulus)
        {
            int index = (int)i;
            if ((modulus.Count++ % modulus.Modulus) == 0)
            {
                string cached = cachedStrings[index];
                if (message != cached)
                {
                    Log(message);
                    cachedStrings[index] = message;
                }
            }
        }


        static string[] cachedStrings = new string[(int)TraceCacheGrouping.All];
        static Vector3[] cachedVectors = new Vector3[(int)TraceCacheGrouping.All];

        //  [System.Diagnostics.Conditional("DEBUG")]
        public static void LogDiff(Vector3 v1, string message, TraceCacheGrouping i)
        {
            int index = (int)i;

            float threshold = 0.05f;
            Vector3 v2 = cachedVectors[index];
            Vector3 diff = v1 - v2;
            if ((Mathf.Abs(diff.x) > threshold) || (Mathf.Abs(diff.y) > threshold) || (Mathf.Abs(diff.z) > threshold))
            {
                Log(message);
            }
            cachedVectors[index] = v1;
        }

        //  [System.Diagnostics.Conditional("DEBUG")]
        public static void LogOnUnityThread(string message)
        {
            if (UnityEngine.WSA.Application.RunningOnAppThread())
            {
                TraceHelper.Log(message);
            }
            else
            {
                UnityEngine.WSA.Application.InvokeOnAppThread(() =>
               {
                   TraceHelper.Log(message);
               }, false);
            }
        }

        //   [System.Diagnostics.Conditional("DEBUG")]
        public static void Assert(bool condition, string message)
        {
            Debug.Assert(condition, message);
        }


    }


    public class TraceFrequency
    {
        public int id = 0;
        public int Modulus = 200;
        public int Count = 0;
    }


    /// <summary>
    /// TV: Trace Variables 
    /// </summary>
    public static class TraceVariables
    {

        static TraceVariables()
        {
            UnityLoop = new TraceFrequency { id = GetNextId(), Modulus = 1000 };
            InteractionManagerLoop = new TraceFrequency { id = GetNextId(), Modulus = 1000 };
        }
        public static TraceFrequency UnityLoop;
        public static TraceFrequency InteractionManagerLoop;

        private static int idCount = 0;
        private static int GetNextId()
        {
            return idCount++;
        }



    }

} 
