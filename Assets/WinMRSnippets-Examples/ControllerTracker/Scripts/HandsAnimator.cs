using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WinMRSnippets; 

[RequireComponent(typeof(Animator))]
public class HandsAnimator : MonoBehaviour {

    [SerializeField]
    TrackedController trackedController;
     
    private Animator animator; 
	// Use this for initialization
	void Start () {
        animator = GetComponent<Animator>();
        Debug.Assert(animator != null); 

        if ( trackedController == null)
        {
            Debug.LogError("Tracked controller is required");
            Destroy(this); 
        }

        trackedController.SelectPressed += OnSelectPressed;
        trackedController.GraspPressed += OnGraspPressed;
        trackedController.TouchpadPressed += OnTouchpadPressed;
        trackedController.SelectReleased += OnSelectReleased;
        trackedController.GraspReleased += OnGraspReleased;
        trackedController.TouchpadReleased += OnTouchpadReleased;


    }	 


    void OnSelectPressed(object sender, MotionControllerStateChangedEventArgs args)
    {
        animator.SetBool("TriggerPress", true);
    }

    void OnSelectReleased(object sender, MotionControllerStateChangedEventArgs args)
    {
        animator.SetBool("TriggerPress", false );
    }


    void OnGraspPressed(object sender, MotionControllerStateChangedEventArgs args)
    {
        animator.SetBool("GripPress", true);
    }

    void OnGraspReleased(object sender, MotionControllerStateChangedEventArgs args)
    {
        animator.SetBool("GripPress", false );
    }

    void OnTouchpadPressed (object sender, MotionControllerStateChangedEventArgs args)
    {
        animator.SetBool("EasterEgg", true);
    }

    void OnTouchpadReleased(object sender, MotionControllerStateChangedEventArgs args)
    {
        animator.SetBool("EasterEgg", false);
    }

}

