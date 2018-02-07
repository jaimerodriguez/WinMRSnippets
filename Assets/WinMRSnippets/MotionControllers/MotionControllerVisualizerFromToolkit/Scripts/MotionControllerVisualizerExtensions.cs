#define EXTEND_TOOLKIT_MOTION_CONTROLLER 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using UnityEngine.XR.WSA.Input;
using WinMRSnippets; 

using GLTF;

#if ENABLE_WINMD_SUPPORT
using Windows.Foundation;
using Windows.Storage.Streams; 
#endif

namespace WinMRSnippets
{
    public interface IControllerVisualizer
    {
        GameObject SpawnTouchpadVisualizer(Transform parentTransform);
        //Vector3 Position { get; }
        //Vector3 WorldNormal { get; }     
    }
} 

#if EXTEND_TOOLKIT_MOTION_CONTROLLER
namespace HoloToolkit.Unity.InputModule
{
   

    public partial class MotionControllerVisualizer : MonoBehaviour , IControllerVisualizer 
    {
        [Tooltip("Shows a Debug Axis Renderer. Do not use in production; it is not polished UI")]
        [SerializeField]
        private bool ShowDebugAxis = false;

        [Tooltip ("The part of the controller to pivot around for position & rotation. Grip is recommended")]
        [SerializeField]
        private InteractionSourceNode nodeType = InteractionSourceNode.Grip ;

        private DebugAxisRenderer axisRendererLeft, axisRendererRight;
       

        private List<uint> inProgressSources = new List<uint>(); 
        private void LoadControllerModelFromProvider ( InteractionSource source )
        {
            if (!inProgressSources.Contains(source.id))
            {
                inProgressSources.Add(source.id); 
                GameObject go = new GameObject();
                go.transform.parent = ControllersRoot;
                go.name = "Controller " + source.id;              
                var coroutine = StartCoroutine(Attach(go, ControllersRoot, source));
            } 
        }

        private IEnumerator Attach(GameObject target, Transform parent, InteractionSource source)
        {            
            yield return WinMRSnippets.ControllerHelpers.AttachModel(target, parent, source, GLTFMaterial, GLTFMaterial);
            inProgressSources.Remove(source.id);
            FinishControllerSetup(target, source.handedness.ToString(), source.id);

            if (ShowDebugAxis)
            {                 
                //Deactivate target so we can call Init and set parent property on component without Awake/Start running 
                target.SetActive(false); 
                DebugAxisRenderer axisRenderer = null;
                if (source.handedness == InteractionSourceHandedness.Left)
                    axisRenderer= axisRendererLeft = target.AddComponent<DebugAxisRenderer>();
                else
                    axisRenderer= axisRendererRight = target.AddComponent<DebugAxisRenderer>();
                axisRenderer.Init(false, parent); 
                target.SetActive(true);
            }
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

            if (
                 (hasPointerPosition = state.sourcePose.TryGetPosition(out pointerPosition, InteractionSourceNode.Pointer)) &&
                 (hasPointerForward = state.sourcePose.TryGetForward(out pointerForward, InteractionSourceNode.Pointer)) &&
                 (hasPointerRotation = state.sourcePose.TryGetRotation(out pointerRotation, InteractionSourceNode.Pointer)) && 
                 (hasGripForward = state.sourcePose.TryGetForward(out gripForward, InteractionSourceNode.Grip)) &&
                 (hasGripPosition = state.sourcePose.TryGetPosition(out gripPosition, InteractionSourceNode.Grip)) &&                 
                 (hasGripRotation = state.sourcePose.TryGetRotation(out gripRotation, InteractionSourceNode.Grip)) 
                )
             {
                DebugAxisRenderer axis = (state.IsLeftHand() ? axisRendererLeft : axisRendererRight);
                if (axis != null)
                {
                    if ( hasPointerPosition && hasPointerRotation )
                        axis.SetWorldValues(pointerPosition, pointerForward , pointerRotation, DebugAxisRenderer.ControllerElement.Pointer);
                    if (hasGripPosition && hasGripRotation )
                        axis.SetWorldValues(gripPosition, gripForward , gripRotation, DebugAxisRenderer.ControllerElement.Grip);
                }
            }
        }
        
    }


    public static class Extensions
    {
        public static bool Update(this MotionControllerInfo currentController, InteractionSourceState sourceState, 
            InteractionSourceNode nodeType , bool updatePosition = true, bool updateRotation = false )
        {
            Vector3 newPosition;
            bool hasPosition = false , hasRotation = false ;
            if (updatePosition && (hasPosition = sourceState.sourcePose.TryGetPosition(out newPosition, nodeType)))
            {
#if DEBUG
                if (newPosition.IsDebugValid())
#endif
                {

                    currentController.ControllerParent.transform.localPosition = newPosition;
                }
#if DEBUG && TRACING_VERBOSE 
                else
                {
                    Debug.Log("Skipping newPosition:" + newPosition.ToString());
                } 
#endif
            } 
            Quaternion newRotation;
            if (updateRotation && (hasRotation = sourceState.sourcePose.TryGetRotation(out newRotation, nodeType))) 
            {
                currentController.ControllerParent.transform.localRotation = newRotation;
            } 
        
            return hasRotation && hasPosition ;             
        }
 

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


