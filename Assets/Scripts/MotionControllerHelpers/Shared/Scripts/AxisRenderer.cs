// #define EXPERIMENTING

// #define EXPERIMENTING_LOCALLY 
#define EXPERIMENTING_PARENTINIT 


// #define EXPERIMENTING_ALIGNMENT 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisRenderer : MonoBehaviour {

    LineRenderer pointerRay;
    LineRenderer gripRay;
    GameObject gripObject, pointerObject;
    Vector3 pointerPosition, pointerForward, gripPosition, gripForward;
    Quaternion pointerRotation, gripRotation;
    
    float AxisLengthScaleFactor = 4.0f; 
    float scale = .05f ;
    Vector3 scaleVector = Vector3.one; 
    float ElementScale = 0.05f;
    string LineShaderName = "Standard";
    string ElementShaderName = "Standard";
    Color[] Colors = new Color[] { Color.red, Color.yellow };
    float RayWidth = .01f;
    Material material;
    bool UseParentTransform = false ;     
    bool UseWorldSpace = true ;
    bool AllowParentTransform = true; 

    Transform parentTransform = null; 

    bool ValidatesConfiguration ()
    {
        bool isValid = true; 
        if ( UseWorldSpace )
        {
            isValid = !UseParentTransform ; 
        }
        else if ( UseParentTransform )
        {
            isValid = !UseWorldSpace ; 
        }
        return isValid; 
    }

    public void Init(bool useWorldSpace, Transform parent)
    {
        if (AllowParentTransform)
        {
            this.UseWorldSpace = useWorldSpace ;
            if (!UseWorldSpace)
            {
                Debug.Assert(parent != null); 
                parentTransform = parent;
                UseParentTransform = true; 
            } 
        } 
    }


    // Use this for initialization
    void Start() {
        if ( !ValidatesConfiguration())
        {
            Debug.Log("Invalid Configuration, not rendering debug axis");
            Destroy(this); 
        }

        UseParentTransform = UseParentTransform && AllowParentTransform; 

        if (gripObject == null)
        {
            gripObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pointerObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pointerRay = pointerObject.AddComponent<LineRenderer>();            
            gripRay = gripObject.AddComponent<LineRenderer>();

#if EXPERIMENTING_PARENTINIT
            if ( !UseWorldSpace ) 
            {
                Debug.Assert(parentTransform != null,  "You must call Init if not using WordlSace" );
                gripObject.transform.SetParent(parentTransform);
                pointerObject.transform.SetParent(parentTransform);
            }                    
#elif EXPERIMENTING
            if (!UseWorldSpace)
            {                                
                if ( this.transform.parent != null && 
                     this.transform.parent.transform.parent != null )
                {
                    parentTransform = this.transform.parent.transform.parent.transform;
                    Debug.Log("Axis Renderer Parent: " + parentTransform.gameObject.name); 
                }

                if ( parentTransform != null )
                {
                    gripObject.transform.SetParent(parentTransform);
                    pointerObject.transform.SetParent(parentTransform); 
                }
            } 
#elif EXPERIMENTING_LOCALLY
            if (!UseWorldSpace)
            {
                parentTransform = this.transform; 
                if (parentTransform != null)
                {
                    gripObject.transform.SetParent(parentTransform);
                    pointerObject.transform.SetParent(parentTransform);
                }
            }

#else //ORIGINAL 
if (!UseWorldSpace)
            {
                parentTransform = this.transform; 
                if (parentTransform != null)
                {
                    gripObject.transform.SetParent(parentTransform);
                    pointerObject.transform.SetParent(parentTransform);
                }
            }

#endif

        }

        LineRenderer[] lines = new LineRenderer[] { pointerRay, gripRay };
        int index = 0;
        foreach (var renderer in lines)
        {
            renderer.useWorldSpace = UseWorldSpace ;             
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
            renderer.endWidth = RayWidth;

 
        }

        GameObject[] nodes = new GameObject[] { pointerObject, gripObject };
        index = 0;


        foreach (GameObject node in nodes)
        {
            var material = Resources.Load<Material>("AxisRendererMaterial");
            if (material != null)
            {
                node.GetComponent<Renderer>().material = material;
            }
            else
            {
                var elementShader = Shader.Find(ElementShaderName);
                if (elementShader == null)
                    elementShader = new Shader();
                material = new Material(elementShader);
                node.GetComponent<Renderer>().material = material;
            }

#if EXPERIMENTING_PARENTINIT
            if (parentTransform != null)
            {
                node.transform.localScale = scale * new Vector3(1 / parentTransform.localScale.x, 1 / parentTransform.localScale.y, 1 / parentTransform.localScale.z);
            }
            else
                node.transform.localScale = new Vector3(scale, scale, scale); 
#else
            node.transform.localScale = new Vector3(scale, scale, scale); 
#endif
            node.GetComponent<Renderer>().material.color = Colors[index++ % Colors.Length];
        }
    }     

    public enum ControllerElement
    {
        Pointer, Grip
    };

    public void SetWorldValues(Vector3 position, Vector3 forward, Quaternion rotation, ControllerElement element)
    {
        if (element == ControllerElement.Grip)
        {
            gripPosition = position;
            gripForward = forward * AxisLengthScaleFactor;
            gripRotation = rotation;
        }
        else
        {
            pointerPosition = position;
            pointerForward = forward * AxisLengthScaleFactor;
            pointerRotation = rotation;
        }
    }


    /* // Update is called once per frame
    void Update() {
        Vector3 transformedGripPosition, transformedPointerPosition;
        Vector3 transformedGripForward = Vector3.forward, transformedPointerForward = Vector3.forward;
        //  Quaternion transformedGripQuat = Quaternion.identity , transformedPointerQuat; 

        float scale = 1 / ElementScale;
        if (!UseParentSpace)
        {
            transformedGripPosition = this.transform.parent.InverseTransformPoint(gripPosition);
            // transformedGripForward = this.transform.parent.InverseTransformVector(gripForward)  * scale;
            transformedGripForward = this.transform.parent.InverseTransformVector(gripForward) * scale;
            transformedPointerPosition = this.transform.parent.InverseTransformPoint(pointerPosition);

            //  transformedPointerForward =   this.transform.parent.InverseTransformVector(pointerForward) * scale  ;
            // right length (after scaled, but angle feels off by amount of rotation in the pointer ) 
            //  transformedPointerForward = this.transform.parent.InverseTransformPoint(pointerForward) * scale; 
            // scale is off... and it was off by further than the Vector transform
            // transformedPointerForward = this.transform.parent.InverseTransformDirection(pointerForward) * scale; 

            //transformedPointerForward = pointerForward *scale ;
            // scale if fine.. and it is not far but wiggles... 


            //  transformedPointerForward = gripObject.transform.InverseTransformVector(pointerForward) * scale;
            //solid but off by the amount in the grip rotation..


            //sequence 
            //transformedPointerForward = gripObject.transform.InverseTransformVector(pointerForward) * scale;
            //transformedPointerForward = gripObject.transform.rotation * transformedPointerForward; 


            //  Debug.Log(string.Format("transformed: {0} to {1}", pointerForward.ToString(), transformedPointerForward.ToString())); 


            // transformedPointerQuat = gripObject.transform.rotation * pointerRotation; 
            // pointerObject.transform.Translate ( )
        }
        else
        {
            transformedGripPosition = gripPosition;
            transformedPointerPosition = pointerPosition;
            transformedGripForward = gripForward;
            transformedPointerForward = pointerForward;
            //   transformedPointerQuat =   pointerRotation;
        }
        if (gripRay != null && pointerRay != null)
        {
            gripObject.transform.localPosition = transformedGripPosition;
            gripObject.transform.rotation = gripRotation;

            //if (!UseParentSpace)
            //{
            //    transformedGripForward = gripObject.transform.InverseTransformDirection(gripObject.transform.forward) * 2f * scale;
            //}
            //else
            //{

            //}



            gripRay.SetPosition(0, transformedGripPosition);
            gripRay.SetPosition(1, transformedGripPosition + transformedGripForward);

            pointerObject.transform.localPosition = transformedPointerPosition;
            pointerObject.transform.rotation = pointerRotation;

            // transformedPointerForward = this.transform.parent.InverseTransformDirection(pointerForward) * 2f * scale ; 
            // transformedPointerForward = pointerForward;


            // from :https://answers.unity.com/questions/1021968/difference-between-transformtransformvector-and-tr.html
              
            // TransformPoint: position, rotation, and scale 
            //TransformDirection: rotation only 
            //TransformVector: rotation and scale only
            
            //#1 add the forward direction but inversed 
            //#1  transformedPointerForward = this.transform.parent.InverseTransformDirection(pointerObject.transform.forward) * 2f * scale; 
            //#1 -- shoots straigth.. but ignoring the angle of the pointer object... 
            //#2 add the forward direction but inversed using Point 
            //#2 transformedPointerForward = this.transform.parent.InverseTransformPoint(pointerObject.transform.forward) * 2f * scale; 
            //#2 --  way off and shifting... 
            //#3 add the forward direction but inversed 
            // transformedPointerForward = pointerObject.transform.InverseTransformDirection(pointerObject.transform.forward) * 2f * scale;
            //#1 -- shoots straigth.. but ignoring the angle of the pointer object... 



            //  pointerObject.transform.rotation = transformedPointerQuat ;
            pointerRay.SetPosition(0, transformedPointerPosition);
            pointerRay.SetPosition(1, transformedPointerPosition + transformedPointerForward);
        }
    }

*/

#if USEINLISTENERS
    void Update()
    {
        Vector3 transformedGripPosition, transformedPointerPosition;
        Vector3 transformedGripForward = Vector3.forward, transformedPointerForward = Vector3.forward;
        //  Quaternion transformedGripQuat = Quaternion.identity , transformedPointerQuat; 

        float scale = 1 / ElementScale;
         
        {
            transformedGripPosition = gripPosition;
            transformedPointerPosition = pointerPosition;
            transformedGripForward = gripForward;
            transformedPointerForward = pointerForward;
            
        }
        if (gripRay != null && pointerRay != null)
        {
            gripObject.transform.localPosition = transformedGripPosition;
            gripObject.transform.rotation = gripRotation;
            transformedGripForward = gripObject.transform.InverseTransformDirection(gripObject.transform.forward * 2f * scale);
            gripRay.SetPosition(0, Vector3.zero );
            gripRay.SetPosition(1,   transformedGripForward);

  
            pointerObject.transform.localPosition = transformedPointerPosition;
            pointerObject.transform.rotation = pointerRotation;

            transformedPointerForward = pointerObject.transform.InverseTransformDirection(pointerObject.transform.forward * 2f * scale);
             
            pointerRay.SetPosition(0, Vector3.zero );
            pointerRay.SetPosition(1,   transformedPointerForward);
        }
    }

#else
    void Update()
    {
        if (UseWorldSpace)
        {
            UpdateUsingWorldSpace();
        } 
        else if (UseParentTransform)
            UpdateUsingParentTransform(); 
    }

    void UpdateUsingWorldSpace()
    {       
           gripObject.transform.position = gripPosition;         
           pointerObject.transform.position = pointerPosition;
           gripObject.transform.localRotation = gripRotation;
           pointerObject.transform.localRotation = pointerRotation; 
           gripRay.useWorldSpace = true;
           pointerRay.useWorldSpace = true; 
           gripRay.SetPosition(0, gripPosition);
           gripRay.SetPosition(1, gripPosition + gripForward);         
           pointerRay.SetPosition(0, pointerPosition);           
           pointerRay.SetPosition(1, pointerPosition+ pointerForward);         
    }

    void UpdateUsingParentTransform ()
    {
#if EXPERIMENTING
        //Quaternion before = pointerRotation;
        //Quaternion after = pointerRotation * Quaternion.Inverse(parentTransform.rotation); 
        //pointerObject.transform.localRotation = after ;
        //gripObject.transform.localRotation = gripRotation * parentTransform.rotation;
        //Debug.Log(string.Format( "Experimenting -- before:{0}, after{1} ", before.eulerAngles, after.eulerAngles ));

        //Quaternion before = pointerRotation;
        //Quaternion after = pointerRotation * Quaternion.Inverse(parentTransform.rotation);
        //pointerObject.transform.rotation = before;
        //gripObject.transform.rotation = gripRotation ;
         

        gripObject.transform.localPosition = gripPosition;
        pointerObject.transform.localPosition = pointerPosition;


        gripRay.useWorldSpace = false ;
        pointerRay.useWorldSpace = false;

        Vector3 transformedGripRoot = gripObject.transform.InverseTransformPoint(gripPosition);
        Vector3 transformedPointerRoot = pointerObject.transform.InverseTransformPoint(pointerPosition);

        Vector3 transformedGripForward = gripObject.transform.InverseTransformPoint(gripPosition + gripForward);
        transformedGripForward -= transformedGripRoot;
         
        Vector3 transformedPointerForward = pointerObject.transform.InverseTransformPoint(pointerPosition + pointerForward);       
        transformedPointerForward -= transformedPointerRoot;
         
        gripRay.SetPosition(0, Vector3.zero  );        
        gripRay.SetPosition(1, transformedGripForward );
        pointerRay.SetPosition(0, Vector3.zero);
        pointerRay.SetPosition(1, transformedPointerForward );

#elif EXPERIMENTING_LOCALLY

        Vector3 worldGrip = parentTransform.InverseTransformPoint(gripPosition);
        gripObject.transform.localPosition = gripObject.transform.TransformPoint(worldGrip);

        pointerObject.transform.localPosition = parentTransform.InverseTransformPoint(pointerPosition); 
        
        gripRay.useWorldSpace = false;
        pointerRay.useWorldSpace = false;

        Vector3 transformedGripRoot = gripObject.transform.InverseTransformPoint(gripPosition);
        Vector3 transformedPointerRoot = pointerObject.transform.InverseTransformPoint(pointerPosition);

        Vector3 transformedGripForward = gripObject.transform.InverseTransformPoint(gripPosition + gripForward);
        transformedGripForward -= transformedGripRoot;

        Vector3 transformedPointerForward = pointerObject.transform.InverseTransformPoint(pointerPosition + pointerForward);
        transformedPointerForward -= transformedPointerRoot;

        gripRay.SetPosition(0, Vector3.zero);
        gripRay.SetPosition(1, transformedGripForward);
        pointerRay.SetPosition(0, Vector3.zero);
        pointerRay.SetPosition(1, transformedPointerForward);

#elif EXPERIMENTING_PARENTINIT

        if ( gripPosition != Vector3.zero )
        {
            bool isDebugging = false; 
            if ( isDebugging )
                Debug.Log("Stop"); 
        }
        gripObject.transform.localPosition = gripPosition;
        pointerObject.transform.localPosition = pointerPosition;
        pointerObject.transform.localRotation = pointerRotation * parentTransform.rotation;
        gripObject.transform.localRotation = gripRotation * parentTransform.rotation;


        // WORLDS 
        //gripRay.SetPosition(0, gripPosition);
        //gripRay.SetPosition(1, gripPosition + gripForward);
        //pointerRay.SetPosition(0, pointerPosition);
        //pointerRay.SetPosition(1, pointerPosition + pointerForward);

        gripRay.useWorldSpace = false;         
        Vector3 transformedGripRoot = gripObject.transform.InverseTransformPoint(parentTransform.TransformPoint(gripPosition)); 
        Vector3 transformedGripForward = gripObject.transform.InverseTransformPoint(parentTransform.TransformPoint(gripPosition+gripForward));

        gripRay.SetPosition(0, transformedGripRoot);
        gripRay.SetPosition(1, transformedGripForward);

        Vector3 transformedPointerRoot = pointerObject.transform.InverseTransformPoint(parentTransform.TransformPoint(pointerPosition));
        Vector3 transformedPointerForward = pointerObject.transform.InverseTransformPoint(parentTransform.TransformPoint(pointerPosition + pointerForward));

        pointerRay.SetPosition(0, transformedPointerRoot);
        pointerRay.SetPosition(1, transformedPointerForward);




#else //ORIGINAL 

        gripObject.transform.localPosition = gripPosition;
        pointerObject.transform.localPosition = pointerPosition;
        pointerObject.transform.localRotation = pointerRotation ;
        gripObject.transform.localRotation = gripRotation;
         gripRay.useWorldSpace = false ;
        pointerRay.useWorldSpace = false ;

        Vector3 transformedGripForward = gripObject.transform.InverseTransformPoint (gripPosition+gripForward);
        Vector3 transformedGripRoot = gripObject.transform.InverseTransformPoint(gripPosition);
        transformedGripForward -= transformedGripRoot; 

        Vector3 transformedPointerForward = pointerObject.transform.InverseTransformPoint(pointerPosition + pointerForward);
        Vector3 transformedPointerRoot = pointerObject.transform.InverseTransformPoint(pointerPosition);
        transformedPointerForward -= transformedPointerRoot; 
        
        gripRay.SetPosition(0, Vector3.zero  );        
        gripRay.SetPosition(1, transformedGripForward );
        pointerRay.SetPosition(0, Vector3.zero);
        pointerRay.SetPosition(1, transformedPointerForward );

#endif
    }
#endif

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
