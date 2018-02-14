using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DebugExtensions  {

	
    public static void Debug_LogOnUnityThread ( this UnityEngine.Debug debug , string message )
    {
        
        if (UnityEngine.WSA.Application.RunningOnAppThread())
        {
            Debug.Log(message);
        }
        else
        {
            UnityEngine.WSA.Application.InvokeOnAppThread(() =>
            {
                Debug.Log(message);
            }, false);             
        }
    }
}
