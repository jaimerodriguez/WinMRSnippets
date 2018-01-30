using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;


namespace WinMRSnippets.Samples 
{

    public class KeywordsSample : MonoBehaviour
    {
        #region  UNITY_EDITOR_PROPERTIES 
        [SerializeField]
        [Tooltip("Total seconds to wait before launching Recognizer")]
        private float startDelay = 5f ; 

        #endregion 

        private KeywordRecognizer keywordRecognizer;        
        // Use this for initialization
        void Start()
        {
            if (!HasMicPermission())
            {
                Debug.LogError ("No Mic permission");
            }

            StartCoroutine(StartMicAsync(startDelay));

        }

        IEnumerator StartMicAsync(float delay)
        {
            float endTime = Time.time + delay;

            while (Time.time < endTime)
            {
                yield return new WaitForSeconds( startDelay/5);
            }
            StartMic();
        }

        void StartMic()
        {
            if (keywordRecognizer == null)
            {            
#if TRACING_VERBOSE
               Debug.Log("Starting keyword recognizer");
#endif
                keywordRecognizer = new KeywordRecognizer(new string[] { "activate", "turn off", "three word phrase" });
                keywordRecognizer.OnPhraseRecognized += (args) =>
                {                     
                    UnityEngine.WSA.Application.InvokeOnAppThread(() =>
                    {
                        Debug.Log(string.Format("Phrase: {0}, Conf: {1} ",
                                args.text, args.confidence));
                    }, false);                     
                };
                keywordRecognizer.Start();                 
            }
        }

        // Update is called once per frame
        void Update()
        {

        }


        bool HasMicPermission()
        {
#if ENABLE_WINMD_SUPPORT
        var info = Windows.Devices.Enumeration.DeviceAccessInformation.CreateFromDeviceClass(Windows.Devices.Enumeration.DeviceClass.AudioCapture);
        info.AccessChanged += Info_AccessChanged;
        return info.CurrentStatus == Windows.Devices.Enumeration.DeviceAccessStatus.Allowed; 

#else
            return true;
#endif
        }


#if ENABLE_WINMD_SUPPORT
    private void Info_AccessChanged(Windows.Devices.Enumeration.DeviceAccessInformation sender, Windows.Devices.Enumeration.DeviceAccessChangedEventArgs args)
    {
        UnityEngine.WSA.Application.InvokeOnAppThread(() =>
        {
            Debug.Log("Mic access changed" + args.Status );
        }, false); 
         
    }
#endif
    }
} 