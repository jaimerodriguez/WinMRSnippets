using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_5 
using UnityEngine.VR;
using UnityEngine.VR.WSA;
#else
using UnityEngine.XR;
using UnityEngine.XR.WSA; 
#endif

public class TrackingWatcher : MonoBehaviour {

	// Use this for initialization
	void Start () {
        TraceHelper.Log("Tracking Watcher started"); 
        WorldManager.OnPositionalLocatorStateChanged += WorldManager_OnPositionalLocatorStateChanged;
	}

    private void WorldManager_OnPositionalLocatorStateChanged(PositionalLocatorState oldState, PositionalLocatorState newState)
    {
        TraceHelper.Log("WorldManager_OnPositionalLocatorStateChanged: " + newState.ToString()); 
    }

    // Update is called once per frame
    void Update () {

        switch (WorldManager.state)
        {
            case PositionalLocatorState.Active:
                // handle active
                break;
            case PositionalLocatorState.Activating:
            case PositionalLocatorState.Inhibited:
            case PositionalLocatorState.OrientationOnly:
            case PositionalLocatorState.Unavailable:
            default:
                TraceHelper.LogDiff("TrackingWatcher State: " +  WorldManager.state, TraceCacheGrouping.TrackingState); 
                break;
        }

    }
    private void OnDestroy()
    {
        WorldManager.OnPositionalLocatorStateChanged -= WorldManager_OnPositionalLocatorStateChanged; 
    }
}
