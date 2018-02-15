
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech; 
using UnityEngine.XR;
using System.Linq;

using WinMRSnippets.Samples.Utilities;  

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

            commands = new List<Command>() 
            {
                new Command(KeyCode.Alpha0, "Help", OnHelp) ,
                new Command(KeyCode.Alpha1, "Stationary", OnTryStationary) ,
                new Command(KeyCode.Alpha2, "RoomScale", OnTryRoomscale),
                new Command(KeyCode.Alpha3, "Dimensions", OnGetDimensions) ,
                new Command(KeyCode.Alpha4, "Geometry", OnGetGeometry),
                new Command(KeyCode.Alpha5, "Recenter", OnRecenter) ,
                new Command(KeyCode.Alpha6, "Positional", OnTogglePositional)
            };

            var panelView = GameObject.FindObjectOfType<CommandPanelView>();
            if ( panelView != null )
                panelView.PopulateCommands(commands); 

            DumpXRDevice();
            StartCoroutine(StartVoice(5f));
            StartCoroutine(TraceCameraPosition(3f));
        }

        Dictionary<string, System.Action> voiceCommands ;
        IEnumerator StartVoice(float delay )
        {
            yield return new WaitForSeconds(5);

            voiceCommands = new Dictionary<string, System.Action>();
            voiceCommands.Add("dimensions", OnGetDimensions);
            voiceCommands.Add("stationary", OnTryStationary);
            voiceCommands.Add("geometry", OnGetGeometry);
            voiceCommands.Add("room scale", OnTryRoomscale);
            voiceCommands.Add("recenter", OnRecenter);
            voiceCommands.Add("positional", OnTogglePositional);
            voiceCommands.Add("help", OnHelp); 
             
            KeywordRecognizer recognizer = new KeywordRecognizer(voiceCommands.Keys.ToArray());
            recognizer.OnPhraseRecognized += Recognizer_OnPhraseRecognized;
            recognizer.Start();
            System.Diagnostics.Debug.Assert(recognizer.IsRunning);
            OnHelp();                       
        }

       

        private List<Command> commands = new List<Command>(); 
       
         
        void Update ()
        {
            foreach ( Command c in commands )
            {
                if ( Input.GetKeyUp (c.key))
                {
                    c.action.Invoke();
                    Debug.Log("Invoking for " + c.key);
                    break; 
                }
            }
            if (lastUserPesence != XRDevice.userPresence)
            {
                Debug.Log("User presence changed. It is now: " + lastUserPesence);
                lastUserPesence = XRDevice.userPresence; 
            }

        }

        private UserPresenceState lastUserPesence = UserPresenceState.Unsupported  ; 


        void OnHelp ( )
        {
            Debug.Log("Voice recognition commands: ");

#if !UNITY_EDITOR
            foreach (Command c in commands)
            {
                Debug.Log(string.Format("Say '{0}' or press {1}", c.name.ToUpper(), c.key.ToString())) ;
            }
#endif              
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
            if (voiceCommands.TryGetValue(args.text, out keywordAction))
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



