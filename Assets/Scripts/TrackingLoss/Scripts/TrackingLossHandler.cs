using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_5
using UnityEngine.VR.WSA;
#else 
using UnityEngine.XR.WSA;
#endif 

public class TrackingLossHandler : MonoBehaviour {

    void Start()
    {
         WorldManager.OnPositionalLocatorStateChanged += 
            WorldManager_OnPositionalLocatorStateChanged;
    }

    private void WorldManager_OnPositionalLocatorStateChanged(PositionalLocatorState oldState, 
        PositionalLocatorState newState)
    {
        if (newState == PositionalLocatorState.Active)
        {
            // Handle becoming active
        }
        else
        {
            // Handle becoming rotational only
        }
    }
}
