//TODO: SANITIZE 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WinMRSnippets
{
    public class DebugAxisRenderer : MonoBehaviour
    {
        LineRenderer pointerRay;
        LineRenderer gripRay;
        GameObject gripObject, pointerObject;
        Vector3 pointerPosition, pointerForward, gripPosition, gripForward;
        Quaternion pointerRotation, gripRotation;


        float AxisLengthScaleFactor = 5.0f;
        float scale = .05f;
        Vector3 scaleVector = Vector3.one;
        string LineShaderName = "Standard";
        string ElementShaderName = "Standard";
        Color[] Colors = new Color[] { Color.red, Color.yellow };
        float RayWidth = .01f;

        bool UseParentTransform = false;
        bool UseWorldSpace = true;
        bool AllowParentTransform = true;

        Transform parentTransform = null;

        bool ValidatesConfiguration()
        {
            bool isValid = true;
            if (UseWorldSpace)
            {
                isValid = !UseParentTransform;
            }
            else if (UseParentTransform)
            {
                isValid = !UseWorldSpace;
            }
            return isValid;
        }

        public void Init(bool useWorldSpace, Transform parent)
        {
            if (AllowParentTransform)
            {
                this.UseWorldSpace = useWorldSpace;
                if (!UseWorldSpace)
                {
                    Debug.Assert(parent != null);
                    parentTransform = parent;
                    UseParentTransform = true;
                }
            }
        }


        // Use this for initialization
        void Start()
        {
            if (!ValidatesConfiguration())
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

                if (!UseWorldSpace)
                {
                    Debug.Assert(parentTransform != null, "You must call Init if not using WordlSace");
                    gripObject.transform.SetParent(parentTransform);
                    pointerObject.transform.SetParent(parentTransform);
                }
            }

            LineRenderer[] lines = new LineRenderer[] { pointerRay, gripRay };
            int index = 0;
            foreach (var renderer in lines)
            {
                renderer.useWorldSpace = UseWorldSpace;
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


                if (parentTransform != null)
                {
                    node.transform.localScale = scale * new Vector3(1 / parentTransform.localScale.x, 1 / parentTransform.localScale.y, 1 / parentTransform.localScale.z);
                }
                else
                {
                    node.transform.localScale = new Vector3(scale, scale, scale);
                }
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
            pointerRay.SetPosition(1, pointerPosition + pointerForward);
        }

        void UpdateUsingParentTransform()
        {
            if (gripPosition != Vector3.zero)
            {
                bool isDebugging = false;
                if (isDebugging)
                    Debug.Log("Stop");
            }
            gripObject.transform.localPosition = gripPosition;
            pointerObject.transform.localPosition = pointerPosition;
            pointerObject.transform.localRotation = pointerRotation * parentTransform.rotation;
            gripObject.transform.localRotation = gripRotation * parentTransform.rotation;


            gripRay.useWorldSpace = false;
            Vector3 transformedGripRoot = gripObject.transform.InverseTransformPoint(parentTransform.TransformPoint(gripPosition));
            Vector3 transformedGripForward = gripObject.transform.InverseTransformPoint(parentTransform.TransformPoint(gripPosition + gripForward));

            gripRay.SetPosition(0, transformedGripRoot);
            gripRay.SetPosition(1, transformedGripForward);

            Vector3 transformedPointerRoot = pointerObject.transform.InverseTransformPoint(parentTransform.TransformPoint(pointerPosition));
            Vector3 transformedPointerForward = pointerObject.transform.InverseTransformPoint(parentTransform.TransformPoint(pointerPosition + pointerForward));

            pointerRay.SetPosition(0, transformedPointerRoot);
            pointerRay.SetPosition(1, transformedPointerForward);

        }


        private void OnDestroy()
        {
#if TRACING_VERBOSE
        Debug.Log("Cleaning up AxisRenderer");
#endif
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
} 