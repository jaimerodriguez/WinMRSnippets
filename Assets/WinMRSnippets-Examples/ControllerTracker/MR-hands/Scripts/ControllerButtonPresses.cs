using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.WSA.Input;

public class ControllerButtonPresses : MonoBehaviour {

    public bool isRightController;
    private bool pressTrigger = false;
    private bool pressGrip = false;
    private bool pressThumbstickUp = false;
    private bool pressTouchpadR = false;
    private bool pressTouchpadL = false;


    void Start () {
        InteractionManager.InteractionSourcePressed += InteractionManager_InteractionSourcePressed;
        InteractionManager.InteractionSourceReleased += InteractionManager_InteractionSourceReleased;
        InteractionManager.InteractionSourceUpdated += InteractionManager_InteractionSourceUpdated;
	}

    private void InteractionManager_InteractionSourceUpdated(InteractionSourceUpdatedEventArgs obj)
    {
        if ((obj.state.source.handedness == InteractionSourceHandedness.Right && isRightController) || (obj.state.source.handedness == InteractionSourceHandedness.Left && !isRightController))
        {
            if (obj.state.thumbstickPosition.y > 0.5f && !pressThumbstickUp)
            {
                Debug.Log("Thumbstick Up");
                gameObject.GetComponent<Animator>().SetBool("TriggerPress", false);
                gameObject.GetComponent<Animator>().SetBool("GripPress", false);
                gameObject.GetComponent<Animator>().SetBool("ThumbstickUp", true);
                pressThumbstickUp = true;
            }
            else if (obj.state.thumbstickPosition.y < 0.5f)
            {
                gameObject.GetComponent<Animator>().SetBool("ThumbstickUp", false);
                pressThumbstickUp = false;
            }

        }

        if (!obj.state.touchpadPressed)
        {
            gameObject.GetComponent<Animator>().SetBool("EasterEgg", false);
            pressTouchpadR = false; pressTouchpadL = false;
        }
        else if (obj.state.touchpadPressed && obj.state.source.handedness == InteractionSourceHandedness.Right && !pressTouchpadR)
        {
            pressTouchpadR = true;
        }
        else if (obj.state.touchpadPressed && obj.state.source.handedness == InteractionSourceHandedness.Left && !pressTouchpadL)
        {
            pressTouchpadL = true;
        }

        if (pressTouchpadR && pressTouchpadL)
        {
            Debug.Log("Both Touchpads Pressed");
            gameObject.GetComponent<Animator>().SetBool("EasterEgg", true);
        }
    }

    private void InteractionManager_InteractionSourceReleased(InteractionSourceReleasedEventArgs obj)
    {
        if ((obj.state.source.handedness == InteractionSourceHandedness.Right && isRightController) || (obj.state.source.handedness == InteractionSourceHandedness.Left && !isRightController))
        {
            if (pressTrigger)
            {
                gameObject.GetComponent<Animator>().SetBool("TriggerPress", false);
                pressTrigger = false;
            }
            else if (pressGrip)
            {
                gameObject.GetComponent<Animator>().SetBool("GripPress", false);
                pressGrip = false;
            }
        }
    }

    private void InteractionManager_InteractionSourcePressed(InteractionSourcePressedEventArgs obj)
    {
        if ((obj.state.source.handedness == InteractionSourceHandedness.Right && isRightController) || (obj.state.source.handedness == InteractionSourceHandedness.Left && !isRightController))
        {
            gameObject.GetComponent<Animator>().SetBool("GripPress", false);
            gameObject.GetComponent<Animator>().SetBool("TriggerPress", false);
            gameObject.GetComponent<Animator>().SetBool("ThumbstickUp", false);
            gameObject.GetComponent<Animator>().SetBool("EasterEgg", false);
            if (obj.state.selectPressedAmount > 0.1f && !pressTrigger)
            {
                Debug.Log("Select Pressed");
                gameObject.GetComponent<Animator>().SetBool("TriggerPress", true);
                pressTrigger = true;
            }
            if (obj.state.grasped && !pressGrip)
            {
                Debug.Log("Grasp Pressed");
                gameObject.GetComponent<Animator>().SetBool("GripPress", true);
                pressGrip = true;
            }
        }
    }


}
