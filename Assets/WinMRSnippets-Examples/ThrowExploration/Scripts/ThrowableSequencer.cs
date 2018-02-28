﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WinMRSnippets; 


[RequireComponent(typeof(TrackedController))]
public class ThrowableSequencer : MonoBehaviour {

    TrackedController trackedController;
    public GameObject visualizerPrefab;
    public GameObject throwTargetPrefab; 

    private GameObject currentVisualizer; 

	// Use this for initialization
	void Start () {
        Physics.gravity = new Vector3(0, -1, 0); 
        trackedController = GetComponent<TrackedController>();
        trackedController.SelectPressed += OnSelectPressed;
        trackedController.SelectReleased += OnSelectReleased;
        trackedController.GraspPressed += OnGraspPressed; 
	}

    void OnGraspPressed(object sender, MotionControllerStateChangedEventArgs args)
    {
        Debug.Log("Grasp Pressed"); 
        if ( currentVisualizer != null )
        {
            Destroy(currentVisualizer);
            currentVisualizer = null; 
        }
       

        if ( throwTargetPrefab != null )
        {
            GameObject [] children = GameObject.FindGameObjectsWithTag("FootballContainer"); 
            foreach ( var go in children )
            {
                Destroy(go);
            }

            GameObject newChild = Instantiate(throwTargetPrefab);
            newChild.transform.parent = this.transform;
            newChild.transform.localPosition = Vector3.zero ;
            newChild.transform.localRotation = Quaternion.Euler(90, 0, 0); 
             
        }
    }


    void OnSelectPressed(object sender, MotionControllerStateChangedEventArgs args)
    {
        if (!isRecording)
        {
            isRecording = true;
            if ( currentVisualizer != null )
            {
                Destroy(currentVisualizer); 
            }

            GameObject go = Instantiate(visualizerPrefab);
            if (go != null)
            {
                VelocityVisualizer visualizer = go.GetComponent<VelocityVisualizer>();
                currentVisualizer = visualizer.gameObject;
            }
        }     
    }
	
    void OnSelectReleased(object sender, MotionControllerStateChangedEventArgs args)
    {         
        if ( currentVisualizer  != null )
        {
            var rb = transform.GetComponentInChildren<Rigidbody>();
            if (rb != null)
            {
                var visualizer = currentVisualizer.GetComponent<VelocityVisualizer>();
                if (visualizer != null)
                {
#if TRACING_VERBOSE
            Debug.Log("Throwing"); 
#endif
                    visualizer.SetVectors(0, args.NewState.GripPosition, args.NewState.GripPosition + args.NewState.Velocity);
                    visualizer.SetVectors(1, args.NewState.GripPosition, args.NewState.GripPosition + args.NewState.AngularVelocity);
                    visualizer.SetVectors(2, args.NewState.GripPosition, args.NewState.GripPosition + GetAverageVelicity());


                    Vector3 objectVelocity, objectAngularVelocity;


                    Throwing.GetThrownObjectVelAngVel(args.NewState.GripPosition, args.NewState.Velocity, args.NewState.AngularVelocity,
                        rb.transform.TransformPoint(rb.centerOfMass), out objectVelocity, out objectAngularVelocity);

                    visualizer.SetVectors(3, args.NewState.GripPosition, args.NewState.GripPosition + objectVelocity);
                    // visualizer.SetVectors(3, args.NewState.GripPosition, args.NewState.GripPosition + objectAngularVelocity  );
                    Debug.Log(string.Format("origin:{0}, velo:{1},ang{2}, obj velo{3}, obj ang{4} ",
                        args.NewState.GripPosition,
                        args.NewState.GripPosition + args.NewState.Velocity,
                        args.NewState.GripPosition + args.NewState.AngularVelocity,
                        args.NewState.GripPosition + objectVelocity,
                        args.NewState.GripPosition + objectAngularVelocity
                        ));
                    rb.transform.parent.transform.parent = null;
                    rb.velocity = args.NewState.Velocity;
                   //  rb.angularVelocity = args.NewState.AngularVelocity; 
                    rb.isKinematic = false;
                    rb.AddTorque(new Vector3(0, 0, .1f));
                }
            } 
        }

        isRecording = false;
        readings = 0; 
    }

    Vector3[] historicalVelocities = new Vector3[200];
    int readings = 0; 
    bool isRecording = false;  

    Vector3 GetLastVelocities ( )
    {
        int start = readings % historicalVelocities.Length;
        int desired = 5; 
        int accounted = 0; 
        Vector3 sum = Vector3.zero; 
        for ( int x = start-1; x >= 0 && accounted < desired ; x-- )
        {
            sum += historicalVelocities[x];
            accounted++; 
        }
        if ( accounted < desired && readings > historicalVelocities.Length)
        {             
            for ( int x = historicalVelocities.Length-1; accounted < desired;  x-- )
            {
                sum += historicalVelocities[x];
                accounted++;
            }
        }
        return sum / accounted; 
    }

    Vector3 GetAverageVelicity ()
    {
        return GetLastVelocities(); 

        int totalReadings = Mathf.Max(readings, historicalVelocities.Length );
        Vector3 sum = Vector3.zero; 
        for (int x = 0; x < totalReadings; x++ )
        {
            sum += historicalVelocities[x]; 
        }
        return sum / totalReadings; 
    }

    // Update is called once per frame
    void Update () {
		if ( isRecording )
        {
            var state = trackedController.GetState();
            historicalVelocities[readings++ % historicalVelocities.Length] = state.Velocity; 
        }
	}
}