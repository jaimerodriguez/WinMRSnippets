using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Collections;

namespace WinMRSnippets
{
    /// <summary>
    /// GraphicsRayCaster that can handle the special WinMRPointerData (which has VR pointer data for rayCasting 2D UI )
    /// Most of this code comes from Unity's GraphicsRayCaster.  https://bitbucket.org/Unity-Technologies/ui, that is why naming convention is mixed, to easily compare to original
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class WinMRGraphicsRaycaster : BaseRaycaster
    {

#region UNITY EDITOR PROPERTIES 

        [SerializeField]
        [Tooltip("Length of Ray for hit testing.\n\rLeave default value (0) to use Camera's Far Clip Plane")]
        private float hitTestDistance = 0.0f;

        [SerializeField]
        [Tooltip("Type of elements that will block raycast from reaching canvas, when canvas is behind this element")]
        private GraphicRaycaster.BlockingObjects blockingObjects = GraphicRaycaster.BlockingObjects.None;

        [SerializeField]
        [Tooltip("Filter colliders in these layers")]
        protected LayerMask blockingMask = -1;


        #endregion


        protected Canvas _canvas;
        protected Canvas _Canvas { get { return _canvas ?? (_canvas = GetComponent<Canvas>()); } }

        protected Camera _eventCamera;
        public override Camera eventCamera
        {
            get
            {
                return _eventCamera ?? (_eventCamera = (_Canvas.worldCamera != null ? _Canvas.worldCamera : Camera.main));
            }
        }

    
        [NonSerialized]
        readonly List<TGraphicRaycastResult> m_RaycastResults = new List<TGraphicRaycastResult>();
        Vector2 m_lastPosition = Vector2.zero;



        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (_Canvas != null)
            {

                float hitDistance;
                if (hitTestDistance > 0.01f)
                    hitDistance = hitTestDistance;
                else
                    hitDistance = eventCamera.farClipPlane;


                Ray ray = new Ray(eventData.pointerCurrentRaycast.worldPosition, eventData.pointerCurrentRaycast.worldNormal);


                if (blockingObjects != GraphicRaycaster.BlockingObjects.None)
                {
                     
                    if ( (blockingObjects == GraphicRaycaster.BlockingObjects.ThreeD) || (blockingObjects == GraphicRaycaster.BlockingObjects.All ) )
                    {
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, hitDistance, blockingMask))
                        {
                            hitDistance = Mathf.Min(hit.distance, hitDistance);
                        }
                    }

                    if ( (blockingObjects == GraphicRaycaster.BlockingObjects.TwoD) || (blockingObjects == GraphicRaycaster.BlockingObjects.All) ) 
                    {
                        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, eventData.pointerCurrentRaycast.depth, blockingMask);
                        if (hit.collider != null)
                        {
                            float currentHit = hit.fraction * eventData.pointerCurrentRaycast.depth;
                            if (currentHit < hitDistance)
                            {
                                hitDistance = currentHit;
                            }
                        }
                    }
                }

                m_RaycastResults.Clear();


                Raycast(_Canvas, ray, hitDistance, eventCamera, m_RaycastResults);

                RaycastResult? closestResult = null;

                for (var index = 0; index < m_RaycastResults.Count; index++)
                {
                    var go = m_RaycastResults[index].graphic.gameObject;

                    RaycastResult castResult = new RaycastResult
                    {
                        gameObject = go,
                        module = this,
                        distance = m_RaycastResults[index].distance,
                        screenPosition = m_RaycastResults[index].pointerPosition,
                        worldPosition = m_RaycastResults[index].position,
                        index = resultAppendList.Count,
                        depth = m_RaycastResults[index].graphic.depth,
                        sortingLayer = _Canvas.sortingLayerID,
                        sortingOrder = _Canvas.sortingOrder
                    };
                    resultAppendList.Add(castResult);
                    if (!closestResult.HasValue || castResult.distance < closestResult.Value.distance)
                    {
                        closestResult = castResult;
                    }
                }
                if (closestResult.HasValue)
                {
                    eventData.pointerCurrentRaycast = closestResult.Value;
                    eventData.position = closestResult.Value.screenPosition;
                    eventData.delta = eventData.position - m_lastPosition;
                    m_lastPosition = eventData.position;
                }
            }
        }
       


        struct TGraphicRaycastResult
        {
            public Graphic graphic;
            public float distance;
            public Vector3 position;
            public Vector2 pointerPosition;
            public override string ToString()
            {
                return string.Format("[{0} - {1}, {2}]", graphic.gameObject.name, distance, pointerPosition);
            }
        }




        [NonSerialized]
        static readonly List<TGraphicRaycastResult> s_SortedGraphics = new List<TGraphicRaycastResult>();
        private static void Raycast(Canvas canvas, Ray ray, float hitDistance, Camera eventCamera, List<TGraphicRaycastResult> results)
        {
            
            // Necessary for the event system
            IList<Graphic> foundGraphics = GraphicRegistry.GetGraphicsForCanvas(canvas);
            for (int i = 0; i < foundGraphics.Count; ++i)
            {
                Graphic graphic = foundGraphics[i];
                // -1 means it hasn't been processed by the canvas, which means it isn't actually drawn
                if (graphic.depth == -1 || !graphic.raycastTarget)
                    continue;

                Transform trans = graphic.transform.transform;
                Vector3 transForward = trans.forward;
                // http://geomalgorithms.com/a06-_intersect-2.html
                float distance = (Vector3.Dot(transForward, trans.position - ray.origin) / Vector3.Dot(transForward, ray.direction));

                // Check to see if the go is behind the camera.
                if (distance < 0)
                    continue;

                // is behind some blocking 2D or 3D
                if (distance >= hitDistance)
                    continue;

                Vector3 position = ray.GetPoint(distance);
                Vector2 pointerPosition = eventCamera.WorldToScreenPoint(position);

                if (!RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, pointerPosition, eventCamera))
                    continue;

                if (graphic.Raycast(pointerPosition, eventCamera ))
                {
                    s_SortedGraphics.Add(new TGraphicRaycastResult()
                    {
                        graphic = graphic,
                        distance = distance,
                        pointerPosition = pointerPosition,
                        position = position
                    });
                }
            }

            s_SortedGraphics.Sort((g1, g2) => g2.graphic.depth.CompareTo(g1.graphic.depth));
            for (int i = 0; i < s_SortedGraphics.Count; ++i)
            {
                results.Add(s_SortedGraphics[i]);
            }

            s_SortedGraphics.Clear();
        }
    }
} 