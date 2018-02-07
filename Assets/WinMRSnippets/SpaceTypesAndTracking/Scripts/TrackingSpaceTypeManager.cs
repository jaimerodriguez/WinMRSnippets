#define TRACING_VERBOSE 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;



namespace WinMRSnippets
{
    public class TrackingSpaceTypeManager : MonoBehaviour
    {
        [Tooltip("Desired spacetype")]
        [SerializeField]
        private TrackingSpaceType spaceType;

        // Use this for initialization
        void Awake ()
        {
            TrackingSpaceTypeHelper.Set(spaceType);
        }
    } 

    public class TrackingSpaceTypeHelper
    {
          public static bool Set ( TrackingSpaceType newSpaceType )
          {
            

#if TRACING_VERBOSE
                TrackingSpaceType prior = XRDevice.GetTrackingSpaceType(); 
#endif 
                bool result = XRDevice.SetTrackingSpaceType(newSpaceType);

#if TRACING_VERBOSE
                TrackingSpaceType current = XRDevice.GetTrackingSpaceType();
                Debug.Log(string.Format("Set {0}. TrackingSpacetype is now {1}; it was {2} before.",
                    result ? "Succeeded" : "Failed", prior, current)); 
#endif 
                return result; 
          }
    }
} 