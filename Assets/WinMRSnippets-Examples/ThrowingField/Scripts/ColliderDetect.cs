using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderDetect : MonoBehaviour {

    public ParticleSystem ps;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("It went through!!!");
        ps.Play();
    }

}
