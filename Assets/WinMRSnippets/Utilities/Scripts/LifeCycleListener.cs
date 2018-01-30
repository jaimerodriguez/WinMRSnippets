using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeCycleListener : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    private float lastTime = 0; 
	// Update is called once per frame
	void Update ()
    {
        float lap = 3f; 
        if (   Time.time >  ( lastTime + lap ) )
        {
            Debug.Log("Running: " + Time.time);
            lastTime = Time.time; 
        }
	}

    private void OnApplicationPause(bool pause)
    {
        Debug.Log ("OnApplicationPause: " + pause ); 
    }

    private void OnApplicationFocus(bool focus)
    {
        Debug.Log ("OnApplicationFocus: " + focus );
    }
}
