using UnityEngine;
using System.Collections;

public class Actions : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// 
	// ********************ATIONS CALLED BY THE USER ***********************
	//

	public void changeRenderGreen()
	{
		Renderer rend = GetComponent<Renderer> ();
		rend.material.color = Color.green;
	}

	public void changeRenderRed()
	{
		Renderer rend = GetComponent<Renderer> ();
		rend.material.color = Color.red;
	}

	public void changeRenderGray()
	{
		Renderer rend = GetComponent<Renderer> ();
		rend.material.color = Color.gray;
	}

	public void changeRenderYellow()
	{
		Renderer rend = GetComponent<Renderer> ();
		rend.material.color = Color.yellow;
	}
	public void verticalVelocity()
	{
		GetComponent<Rigidbody> ().velocity = new Vector3 (0, 5, 0);
	}

}
