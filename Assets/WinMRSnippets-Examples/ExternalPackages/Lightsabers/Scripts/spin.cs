using UnityEngine;
using System.Collections;

public class spin : MonoBehaviour {

	public Vector3 spinXYZ;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		transform.Rotate(spinXYZ*Time.deltaTime);
	}
}
