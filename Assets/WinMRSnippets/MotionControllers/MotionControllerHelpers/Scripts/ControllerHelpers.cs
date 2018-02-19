 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;
using UnityEngine.XR;


namespace WinMRSnippets
{

    public static class ControllerHelpers
    {
        public static IEnumerator AttachModel(GameObject target, Transform parent,
                    InteractionSource source, Material colorMaterial, Material noColorMaterial,  float timeOut = 15f )
        {            
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



        public static IEnumerator AttachModel(GameObject target, Transform parent,
                    XRNode source, Material colorMaterial, Material noColorMaterial, float timeOut = 15f )
        {
             
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



        public static IEnumerator AttachModelById(GameObject target, Transform parent, string productId,
                        Material colorMaterial, Material noColorMaterial, float timeOut = 15f )
        {
            float startTime = Time.time;
            float endTime = startTime + timeOut;            
            bool modelFound = false;
#if TRACING_VERBOSE
            Debug.Log(string.Format("Attaching model {0} to {1}", productId, target.name));
#endif
            while (Time.time < endTime && !modelFound)
            {
                if (ControllerModelProvider.Instance.Contains(productId))
                {
                    byte[] gltfModel = ControllerModelProvider.Instance.GetModelData(productId);
                    if (gltfModel != null)
                    {                        
                        GLTF.GLTFComponentStreamingAssets gltfScript = target.AddComponent<GLTF.GLTFComponentStreamingAssets>();
                        gltfScript.ColorMaterial = colorMaterial;
                        gltfScript.NoColorMaterial = noColorMaterial;
                        gltfScript.GLTFData = gltfModel;
                        yield return gltfScript.LoadModel();
                        modelFound = true;
                    }
                }
                else
                {
                    yield return new WaitForSeconds(0.3f);
                }
            }
#if TRACING_VERBOSE
            if (!modelFound)
            {
                Debug.Log( string.Format ( "No model found for {0} after {1} seconds" + productId, timeOut ));
            } 
            else
            {
                Debug.Log(string.Format("Model {0} attached within {1} seconds" ,  productId, (Time.time - startTime)));
            }
#endif             
        }


        public static GameObject SpawnTouchpadVisualizer(Transform parentTransform, GameObject touchpad, Material material)
        {
            Debug.Log("SpawnTouchpadVisualizer"); 
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
} 