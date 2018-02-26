using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WinMRSnippets; 


[RequireComponent(typeof(TrackedController))]
public class ThrowableSequencer : MonoBehaviour {

    TrackedController trackedController;
    public GameObject visualizerPrefab;

    private GameObject currentVisualizer; 

	// Use this for initialization
	void Start () {

        trackedController = GetComponent<TrackedController>();
        trackedController.SelectPressed += OnSelectPressed;
        trackedController.SelectReleased += OnSelectReleased;
        trackedController.GraspPressed += OnGraspPressed; 
	}

    void OnGraspPressed(object sender, MotionControllerStateChangedEventArgs args)
    {
        if ( currentVisualizer != null )
        {
            Destroy(currentVisualizer);
            currentVisualizer = null; 
        }
    }


    void OnSelectPressed(object sender, MotionControllerStateChangedEventArgs args)
    {
        isRecording = true;
        GameObject go = Instantiate(visualizerPrefab);
        if (go != null)
        {
            VelocityVisualizer visualizer = go.GetComponent<VelocityVisualizer>();
            currentVisualizer = visualizer.gameObject ; 
        }
    
    }
	
    void OnSelectReleased(object sender, MotionControllerStateChangedEventArgs args)
    {
         
        if ( currentVisualizer  != null )
        {
            var visualizer = currentVisualizer.GetComponent<VelocityVisualizer>(); 
            if (visualizer != null )
            {
                visualizer.SetVectors(0, args.NewState.GripPosition, args.NewState.GripPosition + args.NewState.Velocity);
                visualizer.SetVectors(1, args.NewState.GripPosition, args.NewState.GripPosition + args.NewState.AngularVelocity);
                visualizer.SetVectors(2, args.NewState.GripPosition, args.NewState.GripPosition + GetAverageVelicity());
                visualizer.SetVectors(3, args.NewState.GripPosition, args.NewState.GripPosition + Vector3.zero); 
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
