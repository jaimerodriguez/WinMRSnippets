#define EXPERIMENTING_CURSOR_NORMAL 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class BasicCursor : MonoBehaviour {

    // Use this for initialization
    CircularLoader loader;
    void Start()
    {
        loader = GetComponent<CircularLoader>();
        if (!loader.enabled)
        {
            Destroy(loader);
        }

         
    }

    GameObject currentLookAtTarget = null; 

	// Update is called once per frame
	void Update () {

        Vector3 position = Vector3.zero;
        GameObject target;

 
        Vector3 normal; 
        if (WinMRSnippets.WinMRInputModule.Instance.TryGetCursorCoordinates(out position, out normal, out target))
        {
            this.transform.position = position;
            this.transform.rotation = Quaternion.LookRotation(normal); 
            if (loader != null)
            {
                if (target == null)
                {
                    loader.StopGazeTimerAnimation();
                    currentLookAtTarget = null;
                }
                else if (target != currentLookAtTarget)
                {
                    GameObject handler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(target);
                    if (handler != null && handler != currentLookAtTarget)
                    {
                        loader.StartGazeTimerAnimation();
                        Debug.Log(string.Format ( "new target: {0}, at {1}, rotation:{2}" , target.name , this.transform.position, this.transform.rotation.eulerAngles.ToString()));
                        currentLookAtTarget = handler;
                    }
                }
            }
        }
        else
        {
            //TODO: 
            this.transform.position = new Vector3(0f, 0f, 1.4f);
        } 
	}
}
