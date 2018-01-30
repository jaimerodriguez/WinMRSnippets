
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
using UnityEngine.XR;
using UnityEngine.XR.WSA; 
 

public class TrackingWatcher : MonoBehaviour {

	// Use this for initialization
	void Start () {
#if TRACING_VERBOSE
        Debug.Log("Tracking Watcher started"); 
#endif 
        WorldManager.OnPositionalLocatorStateChanged += WorldManager_OnPositionalLocatorStateChanged;
        
	}

    private void WorldManager_OnPositionalLocatorStateChanged(PositionalLocatorState oldState, PositionalLocatorState newState)
    {
        Debug.Log("WorldManager_OnPositionalLocatorStateChanged: " + newState.ToString()); 
        //TODO: Add your real code here to deal with loss tracking appropriately
    }

    
    public float updateFrequency = 5f;

#if TRACING_VERBOSE
    float lastPositionPublishTime = 0f;     
    void Update () {


        switch (WorldManager.state)
        {
            case PositionalLocatorState.Active:               
            case PositionalLocatorState.Activating:
            case PositionalLocatorState.Inhibited:
            case PositionalLocatorState.OrientationOnly:
            case PositionalLocatorState.Unavailable:
            default:
                WinMRSnippets.DebugHelpers.TraceHelper.LogDiff("TrackingWatcher State: " +  WorldManager.state, 
                    WinMRSnippets.DebugHelpers.TraceCacheGrouping.TrackingState); 
                 break;
        }

        if ( Time.time > (lastPositionPublishTime + updateFrequency ))
        {
            WinMRSnippets.DebugHelpers.TraceHelper.LogDiff("Camera is at " + Camera.main.transform.position +" " + Time.time  , WinMRSnippets.DebugHelpers.TraceCacheGrouping.LastPosition );
            lastPositionPublishTime = Time.time; 
        }
     }
#endif 

    private void OnDestroy()
    {
        WorldManager.OnPositionalLocatorStateChanged -= WorldManager_OnPositionalLocatorStateChanged; 
    }
}
