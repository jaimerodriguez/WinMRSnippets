using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class KeywordsSample : MonoBehaviour {

    KeywordRecognizer keywordRecognizer;

  

	// Use this for initialization
	void Start () {

        GamepadWatcher.Current.Start(); 
        if ( !HasMicPermission ()  )
        {
            TraceHelper.Log("No Mic permission");           
        }

        StartCoroutine(StartMicAsync( 8 )); 
             
    }

    IEnumerator StartMicAsync( float delay )
    {
        float endTime = Time.time  + delay ;

        while (Time.time < endTime)
        {
            yield return new WaitForSeconds(1.0f); 
        }
        StartMic(); 
    }

    void StartMic ()
    {
        if (keywordRecognizer == null)
        {
            TraceHelper.Log("Starting keyword recognizer");
            keywordRecognizer = new KeywordRecognizer(new string[] { "activate", "turn off", "three word phrase" });
            keywordRecognizer.OnPhraseRecognized += (args) =>
            {
                TraceHelper.LogOnUnityThread (string.Format("Phrase: {0}, Conf: {1} ",
                                args.text, args.confidence));
            };
            keywordRecognizer.Start();
            TraceHelper.Log("Started keyword recognizer");
            
        }
    }

    // Update is called once per frame
    void Update () {
		
	}


    bool HasMicPermission()
    {

#if ENABLE_WINMD_SUPPORT
        var info = Windows.Devices.Enumeration.DeviceAccessInformation.CreateFromDeviceClass(Windows.Devices.Enumeration.DeviceClass.AudioCapture);
        info.AccessChanged += Info_AccessChanged;
        return
                info.CurrentStatus == Windows.Devices.Enumeration.DeviceAccessStatus.Allowed; 

#else 
        return true; 
#endif
    }


#if ENABLE_WINMD_SUPPORT
    private void Info_AccessChanged(Windows.Devices.Enumeration.DeviceAccessInformation sender, Windows.Devices.Enumeration.DeviceAccessChangedEventArgs args)
    {
        TraceHelper.LogOnUnityThread ("Mic access changed" + args.Status );
    }
#endif 
}

 