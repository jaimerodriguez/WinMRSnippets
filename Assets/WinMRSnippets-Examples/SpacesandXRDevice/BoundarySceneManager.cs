﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech; 
using UnityEngine.XR;
using System.Linq;


namespace WinMRSnippets.Samples
{
    public class BoundarySceneManager : MonoBehaviour
    {

        #region UNITY EDITOR PROPERTIES 


#pragma warning disable CS0649 
        [SerializeField]
        [Tooltip("Marker for Origin. Optional. We reference it to move it on stationary so we don't clip it")] 
        private GameObject origin;

        [SerializeField]
        [Tooltip("Game object for track area. Optional. We hide in Stationary as it would not be in right place")] 
        private GameObject trackedArea;

#pragma warning restore 

        #endregion

        // Use this for initialization
        void Start()
        {
#if UNITY_EDITOR
                Debug.LogError ("This scene does not work well in the editor. A lot of the native APIs don't work") ; 
#endif 

            DumpXRDevice();
            StartCoroutine(StartVoice(5f));
            StartCoroutine(TraceCameraPosition(3f));
        }

        Dictionary<string, System.Action> commands;
        IEnumerator StartVoice(float delay )
        {
            yield return new WaitForSeconds(5); 

            commands = new Dictionary<string, System.Action>();
            commands.Add("dimensions", OnGetDimensions);
            commands.Add("stationary", OnTryStationary);
            commands.Add("geometry", OnGetGeometry);
            commands.Add("room scale", OnTryRoomscale);
            commands.Add("recenter", OnRecenter);
            commands.Add("positional", OnTogglePositional);
            commands.Add("help", OnHelp); 
             
            KeywordRecognizer recognizer = new KeywordRecognizer(commands.Keys.ToArray());
            recognizer.OnPhraseRecognized += Recognizer_OnPhraseRecognized;
            recognizer.Start();
            System.Diagnostics.Debug.Assert(recognizer.IsRunning);
            OnHelp();                       
        }

        void OnHelp ( )
        {
            Debug.Log("Voice recognition commands: ");
            Debug.Log("Say 'STATIONARY' to set space type to stationary");
            Debug.Log("Say 'ROOM SCALE' to switch to room scale");
            Debug.Log("Say 'DIMENSIONS' to get play space dimensions");
            Debug.Log("Say 'GEOMETRY' to get play space geometry");
            Debug.Log("Say 'RECENTER' to recenter ");
            Debug.Log("Say 'POSITIONAL' to Toggle Positional Tracking (Off and on)");
            Debug.Log("Say 'HELP' to see the commands again");
        }

        void OnGetDimensions()
        {
            Vector3 trackedAreaDimensions;
            Vector3 playAreaDimensions; 
            TryGetDimensions(out trackedAreaDimensions, UnityEngine.Experimental.XR.Boundary.Type.TrackedArea);
            TryGetDimensions(out playAreaDimensions, UnityEngine.Experimental.XR.Boundary.Type.PlayArea);

             
        }

        void OnTryStationary()
        {
            if ( SetTrackingSpaceType(TrackingSpaceType.Stationary))
            {
                if ( trackedArea != null )
                {
                    trackedArea.SetActive(false); 
                }
            }
            
        }

        IEnumerator TraceCameraPosition(float delay)
        {
            yield return new WaitForSeconds(delay);

            Debug.Log(string.Format("Camera at {0}, with a {1} rotation", 
                    Camera.main.transform.position, Camera.main.transform.rotation.eulerAngles ));
        }

        void OnTryRoomscale()
        { 
            if (SetTrackingSpaceType(TrackingSpaceType.RoomScale))
            {
                if (trackedArea != null)
                    trackedArea.SetActive(true); 
            }
      
        }

        void OnRecenter()
        {
            InputTracking.Recenter();
            StartCoroutine(TraceCameraPosition(3f));
        }

        void OnGetGeometry()
        {
            TryGetGeometry( UnityEngine.Experimental.XR.Boundary.Type.PlayArea);
            TryGetGeometry(UnityEngine.Experimental.XR.Boundary.Type.TrackedArea); 
        }
        private void Recognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
        {

            System.Action keywordAction;
            // if the keyword recognized is in our dictionary, call that Action.
            if (commands.TryGetValue(args.text, out keywordAction))
            {
#if TRACING_VERBOSE
            Debug.Log("executing " + args.text);
#endif
                keywordAction.Invoke();

            }
#if TRACING_VERBOSE
            else
            {
                Debug.Log("Missed " + args.text);
            }
#endif
        }
         
        public bool IsPresent
        {
            get
            {
                return XRDevice.isPresent;
            }
        }

        public void DumpXRDevice()
        {
            Debug.Log("XRDevice");
            Debug.Log(string.Format("UserPresence: {0}", XRDevice.userPresence));
            Debug.Log(string.Format("Headset Present: {0}", IsPresent));
            Debug.Log(string.Format("FOVZoomFactor: {0}", XRDevice.fovZoomFactor));
            Debug.Log(string.Format("TrackingSpaceType: {0}", XRDevice.GetTrackingSpaceType()));            
            Debug.Log(string.Format("Refresh Rate: {0}", XRDevice.refreshRate));
            Debug.Log(string.Format("model: {0}", XRDevice.model));
            Debug.Log(string.Format("Boundary configured: {0}", IsBoundaryConfigured));
            Debug.Log(string.Format("Tracking Space Type:{0}", XRDevice.GetTrackingSpaceType())); 

            //Debug.Log(string.Format("Screen resolution: {0},{1} @ {2} ",
            //            Screen.currentResolution.width, Screen.currentResolution.height, Screen.currentResolution.refreshRate));

        }

        public bool SetTrackingSpaceType(TrackingSpaceType newSpaceType)
        {
            TrackingSpaceType currentSpaceType = XRDevice.GetTrackingSpaceType();
            bool result = true ;
            if (currentSpaceType != newSpaceType)
            {
                result = XRDevice.SetTrackingSpaceType(newSpaceType);
                Debug.Assert(result != false, "Failed to set tracking space type");
                currentSpaceType = XRDevice.GetTrackingSpaceType();
                Debug.Assert(currentSpaceType == newSpaceType, "Unexpected space type after set");
                if (origin != null)
                {
                    if (currentSpaceType == TrackingSpaceType.Stationary)
                    {
                        origin.transform.localPosition = new Vector3(0f, .5f, 0f);
                    }
                    else
                    {
                        origin.transform.localPosition = Vector3.zero;
                    }
                }
            }  
            Debug.Log("Space type is now: " + currentSpaceType.ToString());  
            return result ;
        }

        public bool IsBoundaryConfigured
        {
            get
            {
                return UnityEngine.Experimental.XR.Boundary.configured;
            }
        }


        public bool TryGetDimensions(out Vector3 dimensions, UnityEngine.Experimental.XR.Boundary.Type boundaryType = UnityEngine.Experimental.XR.Boundary.Type.PlayArea)
        {
            bool retVal = UnityEngine.Experimental.XR.Boundary.TryGetDimensions(out dimensions, boundaryType);
            if (retVal)
            {
                Debug.Log(string.Format("Dimensions for {0} are {1}", boundaryType, dimensions));
            }
            else
            {
                Debug.Log(string.Format("TryGetDimensions for {0} failed", boundaryType));
            } 
            return retVal;
        }

        public List<Vector3> TryGetGeometry(UnityEngine.Experimental.XR.Boundary.Type boundaryType = UnityEngine.Experimental.XR.Boundary.Type.PlayArea)
        {
            List<Vector3> geometry = new List<Vector3>();           
            bool retVal = UnityEngine.Experimental.XR.Boundary.TryGetGeometry(geometry, boundaryType);
            
            if (retVal)
            {
                Debug.Log(string.Format("TryGetGeometry for {0} succeeded", boundaryType ));
                geometry.ForEach((v) => { Debug.Log(v.ToString()); });                
            }
            else
            {
                Debug.Log(string.Format("TryGetGeometry for {0} failed", boundaryType));
            }
            return geometry;              
        }

        void OnTogglePositional()
        {
            InputTracking.disablePositionalTracking = !InputTracking.disablePositionalTracking;
            Debug.Log("Positional Tracking is " + (InputTracking.disablePositionalTracking ? "Disabled" : "Enabled" )); 
        }
    }
} 



