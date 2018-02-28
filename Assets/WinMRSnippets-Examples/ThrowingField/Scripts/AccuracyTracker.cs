using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

public class AccuracyTracker : MonoBehaviour {

    MeshRenderer myRenderer = null;
    // Use this for initialization
    void Start()
    {
        myRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        var readings = UnityEngine.XR.WSA.Input.InteractionManager.GetCurrentReading();

        if (readings.Length > 0)
        {
            foreach (var reading in readings)
            {
                Color c = GetColor(reading.properties.sourceLossRisk);
                myRenderer.material.color = c;
            }
        }
    }

    double readings = 0;
    Color GetColor(InteractionSourcePositionAccuracy f)
    {
        switch (f)
        {
            case InteractionSourcePositionAccuracy.High:
                return Color.green;
            case InteractionSourcePositionAccuracy.Approximate:
                return Color.yellow;
            default:
                return Color.red;
        }
    }

    Color GetColor(double f)
    {
        readings += f;

        if (f > 1.2)
            return Color.red;
        else if (f > .9)
            return Color.yellow;
        else
            return Color.green;
    }
}
