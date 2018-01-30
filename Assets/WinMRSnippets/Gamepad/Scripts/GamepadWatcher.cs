using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// An event facade for Windows.Gaming.Input.Gamepad APIs  
/// </summary>
public class GamepadWatcher
{
    private EventHandler _onGamepadAdded;
    private EventHandler _onGamepadRemoved;

    /// <summary>
    /// AutoStart will start listeners as soon as Singleton instance is instantiated. Change default value to true and never have to call Gamepadatcher.Start() 
    /// </summary>
    private static bool AutoStart = false ; 

    public event EventHandler GamepadAdded
    {
        add
        {
            if (listeners < 1)
                throw new InvalidOperationException("Invalid listener count. Did you explicitly call start?");

            _onGamepadAdded = (EventHandler) Delegate.Combine(_onGamepadAdded, value); 

        }
        remove
        {
            _onGamepadAdded = (EventHandler) Delegate.Remove (_onGamepadAdded, value);
        }
    }
    public event EventHandler GamepadRemoved
    {
        add
        {              
            if (listeners< 1)
               throw new InvalidOperationException("Invalid listener count. Did you explicitly call start?");

            _onGamepadRemoved = (EventHandler)Delegate.Combine(_onGamepadRemoved, value); 

        }
        remove
        {
            _onGamepadRemoved = (EventHandler) Delegate.Remove(_onGamepadRemoved, value);
        }
    }
       
    private static GamepadWatcher instance;
    private int listeners = 0;  
    
    private GamepadWatcher()
    {

    }

    public static GamepadWatcher Current
    {
        get
        {
            if ( instance  == null )
            {
                instance = new GamepadWatcher();
                if (AutoStart)
                {
                    instance.Start();
                } 

            }
            return instance; 
        }
    }
    public void Start ( )
    {

#if TRACING_VERBOSE
        Debug.Log("GamepadWatcher started");
        Debug.Log(string.Format("{0} gamepads are present", GetControllersPresent()));
#endif 
        StartListening(); 
        
    }


    private void StartListening()
    {

#if ENABLE_WINMD_SUPPORT
        Windows.Gaming.Input.Gamepad.GamepadAdded +=   OnGamepadAdded;  
        Windows.Gaming.Input.Gamepad.GamepadRemoved +=   OnGamepadRemoved;      
#else 
        Debug.LogError("This class is meant to be used with Windows.Gaming.Input. it does not listen for events when not targetting WINMD (or when in the editor)"); 
#endif
        listeners++;
    }

    private void StopListening ()
    {
        listeners--;
#if ENABLE_WINND_SUPPORT
       if ( listeners == 0 ) 
        {             
            Windows.Gaming.Input.Gamepad.GamepadAdded -=   OnGamepadAdded;  
            Windows.Gaming.Input.Gamepad.GamepaRemoved -=   OnGamepadRemoved;               
        } 
#endif
    }

    public bool IsPresent
    {
        get
        {
            return GetControllersPresent() > 0; 
        }
    }

    public int ControllersPresent
    {
        get
        {  
            return GetControllersPresent();
        }
    }

    private int GetControllersPresent()
    {
#if ENABLE_WINMD_SUPPORT
      return Windows.Gaming.Input.Gamepad.Gamepads.Count;  
#else
        //TODO: this is placeholder editor code. there might be other controllers to check for. 
        var joysticks = Input.GetJoystickNames();
        if (joysticks.Length > 0)
        {
            var xboxControllers = joysticks.Where((joystick) => joystick.ToLower().Contains("xbox"));
            return xboxControllers.Count();
        }
        return 0;
#endif
    }

    public void Stop()
    {
        StopListening(); 

    }

#if ENABLE_WINMD_SUPPORT
    void OnGamepadAdded ( object sender,  Windows.Gaming.Input.Gamepad gamepad )
    {    
        var eh = _onGamepadAdded; 
        if ( eh != null ) 
        { 
            UnityEngine.WSA.Application.InvokeOnAppThread(() => 
           { 
#if TRACING_VERBOSE
                  Debug.Log( string.Format("Gamepad added. {0} controllers present", ControllersPresent ));  
#endif
                eh ( this,  null ); 
            } , true ) ; 
        }
        
    }  
    
    void OnGamepadRemoved ( object sender,  Windows.Gaming.Input.Gamepad gamepad )
    {         
        var eh = _onGamepadRemoved ; 
        if ( eh != null ) 
        { 
            UnityEngine.WSA.Application.InvokeOnAppThread(() => 
            { 
                Debug.Log ("Gamepad removed. IsPresent is :" + IsPresent );
                eh ( this,  null ); 
            } , true ) ; 
        }                
    }    
#endif

}
