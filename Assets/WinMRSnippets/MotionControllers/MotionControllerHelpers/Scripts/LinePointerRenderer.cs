#define TEST_VELOCITY 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
using UnityEngine.XR.WSA.Input;

namespace WinMRSnippets
{
    [RequireComponent(typeof(LineRenderer))]
    public class LinePointerRenderer : PointerRendererBase
    {        

        private LineRenderer lineRenderer;
        private bool lastActivatedState;

        [SerializeField]
        protected float maxLength = 5f; 


        void Start()
        {
            base.Start();
            lineRenderer = GetComponent<LineRenderer>(); 
            if ( lineRenderer == null )
            {
                Debug.LogError ("This renderer requires a LineRenderer component");
                Destroy(this); 
            }
        }

        protected override void RenderPointer()
        {
            if (isActivated)
            {
                lineRenderer.enabled = true;
                MotionControllerState state = trackedController.GetState();

                Vector3 position, forward;
                Quaternion rotation;
                if (sourcePose == InteractionSourceNode.Grip)
                {
                    position = state.GripPosition;
                    forward = state.GripForward;

                }
                else
                {
                    position = state.PointerPosition;
                    forward = state.PointerForward;
                }

#if TEST_VELOCITY
                forward = state.Velocity; 
#else 
                forward *= maxLength;
#endif 

                lineRenderer.SetPosition(0, position);
                lineRenderer.SetPosition(1, position + forward);
            }
            else
            {
                if (lastActivatedState != isActivated)
                {
                    lineRenderer.enabled = false;
                }
            }
            lastActivatedState = isActivated;
        }
    }
} 