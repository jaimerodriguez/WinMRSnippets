using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; 

public class GamepadWatcher
{
    private EventHandler _onGamepadAdded;
    private EventHandler _onGamepadRemoved;

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
            }
            return instance; 
        }
    }
    public void Start ( )
    {
        TraceHelper.Log("GamepadWatcher started");
        TraceHelper.Log(string.Format("{0} gamepads are present", GetControllersPresent()));
#if ENABLE_WINMD_SUPPORT
        Windows.Gaming.Input.Gamepad.GamepadAdded +=   OnGamepadAdded;  
        Windows.Gaming.Input.Gamepad.GamepadRemoved +=   OnGamepadRemoved;            
#endif
        listeners++; 
    }


    public bool? IsPresent
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

private int GetControllersPresent ()
{
#if ENABLE_WINMD_SUPPORT
      return Windows.Gaming.Input.Gamepad.Gamepads.Count;  
#else
    //always return true on editor.. so we plan for 'worst case'  
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
        listeners--;
#if ENABLE_WINND_SUPPORT
       if ( listeners == 0 ) 
        {             
            Windows.Gaming.Input.Gamepad.GamepadAdded -=   OnGamepadAdded;  
            Windows.Gaming.Input.Gamepad.GamepaRemoved -=   OnGamepadRemoved;               
        } 
#endif

    }

#if ENABLE_WINMD_SUPPORT
    void OnGamepadAdded ( object sender,  Windows.Gaming.Input.Gamepad gamepad )
    {       
        TraceHelper.LogOnUnityThread("gamepad added. IsConnected is " + ControllersPresent );
        var eh = _onGamepadAdded; 
        if ( eh != null ) 
        { 
           UnityEngine.WSA.Application.InvokeOnAppThread(() => 
            { 
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
                eh ( this,  null ); 
            } , true ) ; 
        }        
        TraceHelper.LogOnUnityThread("gamepad removed. IsConnected is :" + IsPresent );
    }    
#endif

}
