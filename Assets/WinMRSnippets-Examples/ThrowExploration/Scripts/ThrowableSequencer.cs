// #define USE_THROWING_AVERAGES 
#define USE_THROWING_SCRIPTS 
using System.Collections;
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
        Physics.gravity = new Vector3(0, -9.81f, 0); 
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
            //foreach ( var go in children )
            //{
            //    Destroy(go);
            //}

            GameObject newChild = Instantiate(throwTargetPrefab);
            newChild.transform.parent = this.transform;
            newChild.transform.localPosition = Vector3.zero ;
            // Tweaking angle so ball is released at a more naturally looking angle
            newChild.transform.localRotation = Quaternion.Euler(140, 0, 0); 
             
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
                    visualizer.SetVectors(1, args.NewState.GripPosition, args.NewState.GripPosition + GetAverageVelicity());                   
                    Vector3 throwingScriptVelocity, throwingScriptAngularVelocity;
                    Throwing.GetThrownObjectVelAngVel(args.NewState.GripPosition, args.NewState.Velocity, args.NewState.AngularVelocity,
                        rb.transform.TransformPoint(rb.centerOfMass), out throwingScriptVelocity, out throwingScriptAngularVelocity);

                    visualizer.SetVectors(2, args.NewState.GripPosition, args.NewState.GripPosition + throwingScriptVelocity);

                    visualizer.SetVectors(3, args.NewState.GripPosition, args.NewState.GripPosition + args.NewState.AngularVelocity);


                    Vector3 objectVelocity, objectAngularVelocity;
                    float forceMuliplier = 1.0f;

#if USETHROWING_SCRIPTS
                    objectVelocity = throwingScriptVelocity; 
                    objectAngularVelocity = throwingScriptAngularVelocity;
                    forceMuliplier = 1.0f; 
#elif USE_THROWING_AVERAGES
                    objectVelocity = GetAverageVelicity(5); 
                    objectAngularVelocity = args.NewState.AngularVelocity;
                    forceMuliplier = 1.0f;

#else


                    objectVelocity = args.NewState.Velocity;
                    objectAngularVelocity = args.NewState.AngularVelocity; 
                    forceMuliplier = .8f; 
#endif
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

                    //rb.velocity = args.NewState.Velocity;
                    //rb.angularVelocity = args.NewState.AngularVelocity; 

                    // Eric updates
                    rb.AddForce(objectVelocity * forceMuliplier, ForceMode.Impulse);
                    rb.velocity = objectVelocity;

                    rb.maxAngularVelocity = 25f;
                    rb.AddRelativeTorque(new Vector3(0f, 0f, -100f), ForceMode.Impulse);

                    rb.isKinematic = false;
                    rb.useGravity = true;
                }
            } 
        }

        isRecording = false;
        readings = 0; 
    }

    Vector3[] historicalVelocities = new Vector3[200];
    int readings = 0; 
    bool isRecording = false;  

    Vector3 AverageNLastVelocities( int desired )
    {
        int start = readings % historicalVelocities.Length;
        int max = System.Math.Max(historicalVelocities.Length, readings); 
        if (desired > max)
            desired = max ;
        
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

    Vector3 GetAverageVelicity ( int desired = 8 )
    {
        return AverageNLastVelocities( desired ); 
 
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
