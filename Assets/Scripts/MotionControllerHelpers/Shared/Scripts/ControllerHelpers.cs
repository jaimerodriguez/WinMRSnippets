#define VERBOSE_STATE 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_5
using UnityEngine.VR.WSA.Input;
using XRNode = UnityEngine.VR.VRNode; 
#else 
using UnityEngine.XR.WSA.Input;
using UnityEngine.XR; 
#endif 
public class ControllerHelpers
{
    public static  IEnumerator AttachModel(GameObject target, Transform parent, 
                InteractionSource source , Material colorMaterial, Material noColorMaterial )
    {
        float timeOut = 15f ;
        float endTime = Time.time + timeOut;
        while (Time.time < endTime )
        {
            string id = ControllerModelProvider.Instance.GetProductId(source);
            if (id != string.Empty)
            {
                yield return AttachModelById (target, parent, id, colorMaterial, noColorMaterial); 
              break;
            }
        }  
    }


 
    public static IEnumerator AttachModel(GameObject target, Transform parent,
                XRNode source, Material colorMaterial, Material noColorMaterial)
    {
        float timeOut = 15f;
        float endTime = Time.time + timeOut;
        while (Time.time < endTime)
        {
            string id = ControllerModelProvider.Instance.GetProductId(source);
            if (id != string.Empty)
            {
                yield return AttachModelById(target, parent, id, colorMaterial, noColorMaterial);
                break;
            }
        }
    }

 

    public static IEnumerator AttachModelById (GameObject target, Transform parent, string productId, 
                    Material colorMaterial, Material noColorMaterial)
    {
        float timeOut = 15f;
        float endTime = Time.time + timeOut;
        bool modelFound = false;
#if TRACING_VERBOSE 
        TraceHelper.Log(string.Format ( "Attaching model {0} to {1}", productId, target.name));
#endif 
        while (Time.time < endTime && !modelFound)
        {
            if (ControllerModelProvider.Instance.Contains(productId ))
            {
                byte[] gltfModel = ControllerModelProvider.Instance.GetModelData(productId );
                if (gltfModel != null)
                {
                    TraceHelper.Log("parsing gltf");
                    GLTF.GLTFComponentStreamingAssets gltfScript = target.AddComponent<GLTF.GLTFComponentStreamingAssets>();
                    gltfScript.ColorMaterial = colorMaterial;
                    gltfScript.NoColorMaterial = noColorMaterial;
                    gltfScript.GLTFData = gltfModel;
                    yield return gltfScript.LoadModel();
#if TRACING_VERBOSE
                    TraceHelper.Log("Model attached for " + productId );
#endif 
                    modelFound = true;
                }
            }
            else
            {
#if VERBOSE_STATE
                TraceHelper.LogDiff("No model for " + productId, TraceCacheGrouping.ControllerModel); 
#endif
                yield return new WaitForSeconds(0.3f);
            }
        }
        TraceHelper.Log("Leaving attach model");
    }




    public static GameObject SpawnTouchpadVisualizer(Transform parentTransform , GameObject touchpad,  Material material )
    {
        GameObject touchVisualizer;
        if (touchpad != null)
        {
            touchVisualizer = GameObject.Instantiate(touchpad);
        }
        else
        {
            touchVisualizer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            touchVisualizer.transform.localScale = new Vector3(0.0025f, 0.0025f, 0.0025f);
            touchVisualizer.GetComponent<Renderer>().material = material;
        }        

        GameObject.Destroy(touchVisualizer.GetComponent<Collider>());
        touchVisualizer.transform.parent = parentTransform;
        touchVisualizer.transform.localPosition = Vector3.zero;
        touchVisualizer.transform.localRotation = Quaternion.identity;
        touchVisualizer.SetActive(false);
        return touchVisualizer;
    }
}
