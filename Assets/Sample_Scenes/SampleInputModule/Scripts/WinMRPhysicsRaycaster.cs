using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; 

namespace WinMRSnippets.Samples.Input
{
    /// <summary>
    /// Simple event system using physics raycasts.
    /// </summary>
    [AddComponentMenu("Event/Physics Raycaster")]
    [RequireComponent(typeof(Camera))]
    public class WinMRPhysicsRaycaster : BaseRaycaster
    {
        /// <summary>
        /// Const to use for clarity when no event mask is set
        /// </summary>
        protected const int kNoEventMaskSet = -1;

        protected Camera m_EventCamera;

        /// <summary>
        /// Layer mask used to filter events. Always combined with the camera's culling mask if a camera is used.
        /// </summary>
        [SerializeField]
        protected LayerMask m_EventMask = kNoEventMaskSet;

        protected WinMRPhysicsRaycaster()
        { }

        public override Camera eventCamera
        {
            get
            {
                if (m_EventCamera == null)
                    m_EventCamera = GetComponent<Camera>();
                return m_EventCamera ?? Camera.main;
            }
        }


        /// <summary>
        /// Depth used to determine the order of event processing.
        /// </summary>
        public virtual int depth
        {
            get { return (eventCamera != null) ? (int)eventCamera.depth : 0xFFFFFF; }
        }

        /// <summary>
        /// Event mask used to determine which objects will receive events.
        /// </summary>
        public int finalEventMask
        {
            get { return (eventCamera != null) ? eventCamera.cullingMask & m_EventMask : kNoEventMaskSet; }
        }

        /// <summary>
        /// Layer mask used to filter events. Always combined with the camera's culling mask if a camera is used.
        /// </summary>
        public LayerMask eventMask
        {
            get { return m_EventMask; }
            set { m_EventMask = value; }
        }

        Vector2 m_lastPosition = Vector2.zero;
        public float hitTestDistance = 0f; 

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            float hitDistance;
            if (hitTestDistance > 0.01f)
                hitDistance = hitTestDistance;
            else
                hitDistance = eventCamera.farClipPlane;


            Ray ray = new Ray(eventData.pointerCurrentRaycast.worldPosition, eventData.pointerCurrentRaycast.worldNormal);
           
            var hits = Physics.RaycastAll(ray, hitDistance, finalEventMask);

            if (hits.Length > 1)
                System.Array.Sort(hits, (r1, r2) => r1.distance.CompareTo(r2.distance));

            RaycastResult? closestResult = null;
            if (hits.Length != 0)
            {
                for (int b = 0, bmax = hits.Length; b < bmax; ++b)
                {
                    var result = new RaycastResult
                    {
                        gameObject = hits[b].collider.gameObject,
                        module = this,
                        distance = hits[b].distance,
                        worldPosition = hits[b].point,
                        worldNormal = hits[b].normal,
                     //    screenPosition = hits[b].pointerPosition,
                        index = resultAppendList.Count,
                        sortingLayer = 0,
                        sortingOrder = 0
                    };
                    resultAppendList.Add(result);
                    if (b == 0)
                        closestResult = result;
                }

                if (closestResult.HasValue)
                {
                    eventData.pointerCurrentRaycast = closestResult.Value ;
                   // eventData.position = closestResult.Value.ScreenPosition;
                    eventData.delta = eventData.position - m_lastPosition;
                    m_lastPosition = eventData.position;
                }
            }
        }

    } 
}
