using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

#if UNITY_5 
using UnityEngine.VR;
#else 
using UnityEngine.XR;
#endif 


using System.Linq; 

public class BoundaryManager : MonoBehaviour {

    public GameObject origin;

#if !UNITY_5
    // Use this for initialization
    void Start() {
        DumpXRDevice();
        StartVoice();
        StartCoroutine(TraceCameraPosition(3f)); 
    }

    Dictionary<string, System.Action> commands; 
    void StartVoice ( )
    {
        commands = new Dictionary<string, System.Action>();
        commands.Add("dimensions", OnGetDimensions);
        commands.Add("stationary", OnTryStationary);
        commands.Add("geometry", OnGetGeometry);
        commands.Add("room scale", OnTryRoomscale);
        commands.Add("recenter", OnRecenter );
        commands.Add("positional", OnTogglePositional); 
        KeywordRecognizer recognizer = new KeywordRecognizer(commands.Keys.ToArray());
        recognizer.OnPhraseRecognized += Recognizer_OnPhraseRecognized;
        recognizer.Start();
        TraceHelper.Log("voice command started: dimensions, stationary, geometry, room scale, recenter, positional"); 
    }

    void OnGetDimensions ()
    {
        Vector3 dimensions;
        if (TryGetDimensions(out dimensions))
        {
            TraceHelper.Log(string.Format("Dimensions: {0} ", dimensions));
        }
        else
            TraceHelper.Log("Try Get dimensions returned false ");
    }
    void OnTryStationary ()
    {
        SetTrackingSpaceType(TrackingSpaceType.Stationary); 
    }

    IEnumerator TraceCameraPosition ( float delay )
    {
        yield return new WaitForSeconds(delay);

        TraceHelper.Log(string.Format("Camera at {0} @ {1}", Camera.main.transform.position,
           Camera.main.transform.rotation.ToEulerAngles() ));
    }

    void OnTryRoomscale ()
    {
        SetTrackingSpaceType(TrackingSpaceType.RoomScale );
    }

    void OnRecenter ()
    {
        InputTracking.Recenter();
        StartCoroutine(TraceCameraPosition(3f));
    }

    void OnGetGeometry ()
    {
        TryGetGeometry(); 
    }
    private void Recognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {

        System.Action keywordAction;

        // if the keyword recognized is in our dictionary, call that Action.
        
        if (commands.TryGetValue(args.text, out keywordAction))
        {
            TraceHelper.Log("executing " + args.text);
            keywordAction.Invoke();
            
        }
        else
        {
            TraceHelper.Log("Missed " + args.text); 
        }
    }

    // Update is called once per frame
    void Update() {
       

        //if (Input.GetButtonDown(UnityInputAxis.MotionController_Menu_Left))
        //{
        //    TogglePositionalTracking(); 
        //}
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
        TraceHelper.Log("XRDevice");
        TraceHelper.Log(string.Format("UserPresence: {0}", XRDevice.userPresence));
        TraceHelper.Log(string.Format("Headset Present: {0}", IsPresent ));
        TraceHelper.Log(string.Format("FOVZoomFactor: {0}", XRDevice.fovZoomFactor));
        TraceHelper.Log(string.Format("TrackingSpaceType: {0}", XRDevice.GetTrackingSpaceType()));
        TraceHelper.Log(string.Format("Refresh Rate: {0}", XRDevice.refreshRate));
        TraceHelper.Log(string.Format("model: {0}", XRDevice.model));
        TraceHelper.Log(string.Format("Boundary configured: {0}", IsBoundaryConfigured));
        TraceHelper.Log(string.Format("Screen resolution: {0},{1} @ {2} ",
                    Screen.currentResolution.width, Screen.currentResolution.height, Screen.currentResolution.refreshRate)); 

    }

    public bool SetTrackingSpaceType(TrackingSpaceType newSpaceType)
    {
        TrackingSpaceType current = XRDevice.GetTrackingSpaceType();
        if (current != newSpaceType)
        {
            bool newValue = XRDevice.SetTrackingSpaceType(newSpaceType);
            Debug.Assert(newValue != false, "Failed to set tracking space type");
            current = XRDevice.GetTrackingSpaceType();
            Debug.Assert(current == newSpaceType, "Unexpected space type after set");

            if (origin != null)
            {
                if (current == TrackingSpaceType.Stationary)
                {
                    origin.transform.localPosition = new Vector3(0f, .5f, 0f);
                }
                else
                {
                    origin.transform.localPosition = Vector3.zero; 
                } 
            }
            StartCoroutine(TraceCameraPosition(3f));
            return newValue;
        }
        else
        {
            TraceHelper.Log("Ignoring Tracking spaceType " + newSpaceType); 
        }
        return true;
    }

    public bool IsBoundaryConfigured
    {
        get
        {
            return UnityEngine.Experimental.XR.Boundary.configured  ;
        }
    }

   
    public bool TryGetDimensions(out Vector3 dimensions, UnityEngine.Experimental.XR.Boundary.Type boundaryType = UnityEngine.Experimental.XR.Boundary.Type.PlayArea )
    {
        bool retVal = UnityEngine.Experimental.XR.Boundary.TryGetDimensions(out dimensions, boundaryType); 
        TraceHelper.Log("TryGetDimensions " + (retVal ? "succeeded" : "failed"));
        return retVal;

    }

    public List<Vector3> TryGetGeometry( UnityEngine.Experimental.XR.Boundary.Type boundaryType = UnityEngine.Experimental.XR.Boundary.Type.PlayArea)
    {
        List<Vector3> geometry = new List<Vector3>() ; 
        bool retVal = UnityEngine.Experimental.XR.Boundary.TryGetGeometry( geometry, boundaryType);
        TraceHelper.Log("TryGetGeometry " + (retVal ? "succeeded" : "failed"));
        if ( retVal )
        {
            geometry.ForEach((v) => { TraceHelper.Log(v.ToString()); }); 
        } 

        return geometry;
    }

    void OnTogglePositional()
    {        
        InputTracking.disablePositionalTracking = !InputTracking.disablePositionalTracking;
        TraceHelper.Log("TogglePositional" + InputTracking.disablePositionalTracking);
    }

#endif 


}



