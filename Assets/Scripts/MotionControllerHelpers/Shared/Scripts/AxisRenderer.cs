using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisRenderer : MonoBehaviour {

    LineRenderer pointerRay;
    LineRenderer gripRay;
    GameObject gripObject, pointerObject;
    Vector3 pointerPosition, pointerForward, gripPosition, gripForward;
    Quaternion pointerRotation, gripRotation;

    public float NodeScale = 0.02f;
    public float ElementScale = 0.05f; 
    public string LineShaderName = "Standard" ;
    public string ElementShaderName = "Standard";
    public Color[] Colors = new Color[] { Color.red, Color.yellow } ;
    public float RayWidth = .01f;
    public Material material ; 

    // Use this for initialization
    void Start() {
        
        if (gripObject == null)
        {
            gripObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pointerObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pointerRay = pointerObject.AddComponent<LineRenderer>();
            gripRay = gripObject.AddComponent<LineRenderer>();
        }

        
        Debug.Assert(pointerRay != null, "unexpected pointerRay");
        Debug.Assert(gripRay != null , "unexpected grip Ray" );

        LineRenderer[] lines = new LineRenderer[] { pointerRay, gripRay };

         
        int index = 0;
        foreach (var renderer in lines)
        {
            if (renderer == null)
            {
                TraceHelper.Log("renderer is null");
            }

            renderer.useWorldSpace = true;
            renderer.positionCount = 2;

            var material = Resources.Load<Material>("AxisRendererMaterial");
            if (material != null)
            {
                renderer.material = material;
            }
            else
            {
                var lineShader = Shader.Find(LineShaderName);
                if (lineShader == null)
                    lineShader = new Shader();

                renderer.material = new Material(lineShader);
            } 
            renderer.material.color = Colors[index++ % Colors.Length];
            renderer.startWidth = RayWidth;
            renderer.endWidth = RayWidth ;
        }

        GameObject[] nodes = new GameObject[] { pointerObject, gripObject };
        index = 0;
        
       
        foreach (GameObject g in nodes)
        {
            var material = Resources.Load<Material>("AxisRendererMaterial");
            if (material != null)
            {
                g.GetComponent<Renderer>().material = material;
            }
            else
            {
                var elementShader = Shader.Find(ElementShaderName);
                if (elementShader == null)
                    elementShader = new Shader();
                material = new Material(elementShader);
                g.GetComponent<Renderer>().material = material;
            } 
            g.transform.localScale = new Vector3(ElementScale, ElementScale, ElementScale);                                         
            material.color = Colors[index++ % Colors.Length]; 
        }         
    }

    public enum ControllerElement
    {
        Pointer, Grip  
    };

    public void SetValues(Vector3 position, Vector3 forward, Quaternion rotation, ControllerElement element)
    {
        if (element == ControllerElement.Grip)
        {
            gripPosition = position;
            gripForward = forward;
            gripRotation = rotation;
        }
        else
        {
            pointerPosition = position;
            pointerForward = forward;
            pointerRotation = rotation;
        }
    }

    // Update is called once per frame
    void Update () {

        if ( gripRay != null && pointerRay != null )
        {
            gripObject.transform.localPosition = gripPosition;
            gripObject.transform.rotation = gripRotation;   
            gripRay.SetPosition(0, gripPosition);
            gripRay.SetPosition(1, gripPosition + gripForward);
            
            pointerObject.transform.localPosition = pointerPosition;
            pointerObject.transform.rotation = pointerRotation; 
            pointerRay.SetPosition(0, pointerPosition);
            pointerRay.SetPosition(1, pointerPosition + pointerForward);
        }
    }

    private void OnDestroy()
    {
        TraceHelper.Log("Cleaning up AxisRenderer");
        if (gripObject != null)
        {
            GameObject.Destroy(gripObject);
            gripObject = null;
        }

        if (pointerObject != null)
        {
            GameObject.Destroy(pointerObject);
            pointerObject = null;
        }

    }
}
