#define EXTEND_TOOLKIT_MOTION_CONTROLLER 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_5
using UnityEngine.VR.WSA.Input;
#else 
using UnityEngine.XR.WSA.Input;
#endif 
using GLTF;

#if ENABLE_WINMD_SUPPORT
using Windows.Foundation;
using Windows.Storage.Streams; 
#endif


public interface IControllerVisualizer
{
    GameObject SpawnTouchpadVisualizer(Transform parentTransform);
    //Vector3 Position { get; }
    //Vector3 WorldNormal { get; }     
}


#if EXTEND_TOOLKIT_MOTION_CONTROLLER
namespace HoloToolkit.Unity.InputModule
{
   

    public partial class MotionControllerVisualizer : MonoBehaviour , IControllerVisualizer 
    {
        public bool ShowDebugAxis = false;
        AxisRenderer axisRendererLeft, axisRendererRight;

#if !UNITY_5
        private InteractionSourceNode nodeType; 
#endif 

        private List<uint> inProgressSources = new List<uint>(); 
        private void LoadControllerModelFromProvider ( InteractionSource source )
        {
            if (!inProgressSources.Contains(source.id))
            {
                inProgressSources.Add(source.id); 
                GameObject go = new GameObject();
                go.transform.parent = ControllersRoot;
                go.name = "Controller " + source.id;
                TraceHelper.Log("MotionControllerInput:AddDevice:" + source.id);
                var coroutine = StartCoroutine(Attach(go, ControllersRoot, source));
            } 
        }

        private IEnumerator Attach(GameObject target, Transform parent, InteractionSource source)
        {
            yield return ControllerHelpers.AttachModel(target, parent, source, GLTFMaterial, GLTFMaterial);
            inProgressSources.Remove(source.id);
            FinishControllerSetup(target, source.handedness.ToString(), source.id);

            if (ShowDebugAxis)
            {
                TraceHelper.LogOnUnityThread("Attaching Debug Axis to " + source.handedness);
                AxisRenderer axisRenderer = null;
                if (source.handedness == InteractionSourceHandedness.Left)
                    axisRenderer= axisRendererLeft = target.AddComponent<AxisRenderer>();
                else
                    axisRenderer= axisRendererRight = target.AddComponent<AxisRenderer>();

              //   axisRenderer.SetParent(this.transform.parent); 
            }

#if SKIPNPUTMODULE 
            WinMRSnippets.Samples.Input.MotionControllerInputModule.Instance.AddController(source.id );
            WinMRSnippets.Samples.Input.MotionControllerInputModule.Instance.AddController(source.id);
#endif
        } 

        private Transform ControllersRoot 
        { 
            get 
            {
                return this.gameObject.transform ; 
            }
        }

     

     
        private void UpdateDebugAxis ( InteractionSourceState state )
        {
            Vector3 pointerForward = Vector3.zero , gripForward = Vector3.zero, gripPosition = Vector3.zero, pointerPosition = Vector3.zero;             
            bool    hasPointerPosition = false , hasGripPosition = false , hasGripRotation = false , 
                    hasPointerRotation = false , hasPointerForward = false , hasGripForward = false ;
            Quaternion pointerRotation = Quaternion.identity, gripRotation = Quaternion.identity ;
            Ray pointerRay;  

 //          TraceHelper.Log("Updating debug axis");

            if (
#if !UNITY_5
                 (hasPointerPosition = state.sourcePose.TryGetPosition(out pointerPosition, InteractionSourceNode.Pointer)) &&
                 (hasPointerForward = state.sourcePose.TryGetForward(out pointerForward, InteractionSourceNode.Pointer)) &&
                 (hasPointerRotation = state.sourcePose.TryGetRotation(out pointerRotation, InteractionSourceNode.Pointer)) && 
                 (hasGripForward = state.sourcePose.TryGetForward(out gripForward, InteractionSourceNode.Grip)) &&
                 (hasGripPosition = state.sourcePose.TryGetPosition(out gripPosition, InteractionSourceNode.Grip)) &&                 
                 (hasGripRotation = state.sourcePose.TryGetRotation(out gripRotation, InteractionSourceNode.Grip)) 
                
#else  

                 (hasPointerPosition = hasPointerForward = state.sourcePose.TryGetPointerRay(out pointerRay )) &&                  
                 //(hasGripForward = state.sourcePose.TryGetForward(out gripForward )) &&
                 (hasGripPosition = state.sourcePose.TryGetPosition(out gripPosition )) &&                 
                 (hasGripRotation = state.sourcePose.TryGetRotation(out gripRotation ))
#endif           
                 )
             {
#if UNITY_5
                if (hasPointerPosition && hasPointerForward)
                {
                    pointerPosition = pointerRay.origin;
                    pointerForward = pointerRay.direction;
                } 
#endif 
                AxisRenderer axis = (state.IsLeftHand() ? axisRendererLeft : axisRendererRight);
                if (axis != null)
                {
                    if ( hasPointerPosition && hasPointerRotation )
                        axis.SetWorldValues(pointerPosition, pointerForward , pointerRotation, AxisRenderer.ControllerElement.Pointer);
                    if (hasGripPosition && hasGripRotation )
                        axis.SetWorldValues(gripPosition, gripForward , gripRotation, AxisRenderer.ControllerElement.Grip);
                }
            }
#if DEBUG
            else
                TraceHelper.LogModulus ("Failed to update debug axis", TraceVariables.InteractionManagerLoop);
#endif
            //TraceHelper.Assert(hasPointerForward && hasGripForward && hasGripPosition &&
            //                     hasPointerPosition && hasGripRotation && hasPointerRotation, "Show debug axis should not fail");


#if SKIPNPUTMODULE 
            WinMRSnippets.Samples.Input.MotionControllerInputModule.Instance.SetPosition( state.source.id, pointerPosition);
            WinMRSnippets.Samples.Input.MotionControllerInputModule.Instance.SetForwardPointer (state.source.id, pointerForward);
            WinMRSnippets.Samples.Input.MotionControllerInputModule.Instance.SetButtonStates(state.source.id,
                    state.selectPressed, state.grasped, state.menuPressed ); 
#endif 
        }


        LineRenderer pointer = null; 
        public void DrawPointer ( InteractionSourceState state )
        {
            return; 

            if ( pointer == null )
            {
                pointer = this.gameObject.AddComponent<LineRenderer>();
                pointer.useWorldSpace = false ;
                 
                pointer.positionCount = 2;
                var material = Resources.Load<UnityEngine.Material>("AxisRendererMaterial");
                if (material != null)
                {
                    pointer.material = material;
                }
                float RayWidth = 0.025f; 
                pointer.startWidth = RayWidth;
                pointer.endWidth = RayWidth;
             }

            Vector3 pointerPosition = Vector3.zero;
            Vector3 pointerForward = Vector3.forward;

            if ( 
                state.sourcePose.TryGetPosition(out pointerPosition, InteractionSourceNode.Pointer)  && 
                state.sourcePose.TryGetForward( out pointerForward, InteractionSourceNode.Pointer) 
                )
            {
                pointer.SetPosition(0, pointerPosition);
                pointer.SetPosition(1, pointerPosition + pointerForward * 2 );
            }

            

        }
    }


    public static class Extensions
    {
#if !UNITY_5
        public static bool Update(this MotionControllerInfo currentController, InteractionSourceState sourceState, 
            InteractionSourceNode nodeType , bool updatePosition = true, bool updateRotation = false )
        {
            Vector3 newPosition;
            bool retValPosition = false , retValRotation = false ;
            if (updatePosition && (retValPosition = sourceState.sourcePose.TryGetPosition(out newPosition, nodeType)))
            {
#if DEBUG
                if (newPosition.IsDebugValid())
#endif
                {

                    currentController.ControllerParent.transform.localPosition = newPosition;
                }
#if DEBUG
                else
                {
                    Debug.Log("Skipping newPosition:" + newPosition.ToString());
                } 
#endif  
            } 
            Quaternion newRotation;
            if (updateRotation && (retValRotation = sourceState.sourcePose.TryGetRotation(out newRotation, nodeType))) 
            {
                currentController.ControllerParent.transform.localRotation = newRotation;
            } 

           // TraceHelper.Log(string.Format("{0}:Pos{1},Rot{2}", nodeType, newPosition, newRotation)); 
           // TraceHelper.Assert( retValPosition && retValRotation, "Update should not fail"); 
            return retValRotation && retValPosition ;             
        }
#else
        public static bool Update(this MotionControllerInfo currentController, InteractionSourceState sourceState, object unused  )
        {
            Vector3 newPosition;
            bool retValPosition, retValRotation;
            if (retValPosition = sourceState.sourcePose.TryGetPosition(out newPosition ))
            {
                currentController.ControllerParent.transform.localPosition = newPosition;
            }
            Quaternion newRotation;
            if (retValRotation = sourceState.sourcePose.TryGetRotation(out newRotation ))
            {
                currentController.ControllerParent.transform.localRotation = newRotation;
            }

            // TraceHelper.Log(string.Format("{0}:Pos{1},Rot{2}", nodeType, newPosition, newRotation)); 
            TraceHelper.Assert(retValPosition && retValRotation, "Update should not fail");
            return retValRotation && retValPosition;
        }
#endif


        public static bool IsLeftHand ( this InteractionSourceState state )
        {
            return state.source.handedness == InteractionSourceHandedness.Left; 
        }
        public static bool IsRightHand(this InteractionSourceState state)
        {
            return state.source.handedness == InteractionSourceHandedness.Right;
        }
    }
}
#endif


