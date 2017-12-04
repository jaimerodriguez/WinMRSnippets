using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionControllerPartsLabelRenderer : MonoBehaviour {

    public GameObject Parent;
    public GameObject MotionController;
    public GameObject TextPrefab; 
    bool isParsed = false;  
	// Use this for initialization
	void Start () {
        Debug.Log("Start");      
	}

    void ShowParts ()
    {
        if (!isParsed && Parent != null && MotionController != null)
        {
           
            List<string> parts = new List<string> () {
                  "home", "menu" ,"grasp", "select" /*, "thumbstick_press",  "touchpad_press" */};

             
            var transforms = MotionController.GetComponentsInChildren<Transform>(); 
            foreach ( var partTransform in transforms )
            {
                string name = partTransform.gameObject.name; 
                if ( parts.Contains(name.ToLower()))
                {
                    GameObject newtextElement = Instantiate(TextPrefab);
                    var text = newtextElement.GetComponentInChildren<TextMesh>();
                    text.text = name;
                    newtextElement.transform.SetParent(Parent.transform);
                    newtextElement.transform.position = partTransform.position ; 
                }
            }             
            isParsed = true; 
        }
    }
	
	// Update is called once per frame
	void Update () {
        ShowParts(); 
	}
}
