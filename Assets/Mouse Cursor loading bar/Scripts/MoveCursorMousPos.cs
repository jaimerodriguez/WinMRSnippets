using UnityEngine;
using System.Collections;

public class MoveCursorMousPos : MonoBehaviour {

	// Use this for initialization
	// these are the two cursors gameobjects
	public Transform[] cursors;
	// this is the gameobject the cursors are going to look at (the camera for example)
	public Transform lookAt;

	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		// hide cursor
		Cursor.visible = false;

		// place the cursors at the mouse position always
		//get mouse position into the real world
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = 0.5f;
		Vector3 mouseWorld = Camera.main.ScreenToWorldPoint (mousePos);

		for(int i=0;i<1;i++)
		{
			cursors[i].position=mouseWorld;
			cursors[i].LookAt(lookAt.transform.position);
		}

	
	
	}
}
