# WinMRSnippets
Sample code snippets for Windows Mixed Reality Application Developers.   
The primary goal for these snippets is for people to learn, but you can also use this as a foundation of code to paste/include into your project. 

## What is included?   
- The **WinMRSnippets** folder has the reusable scripts.  This includes: 
	- **GamepadWatcher** - to detect when a gamepad is added/removed. It is event-driven instead of Unity's GetControllerNames() polling approach. 
	- **WinMRInputModule**, **WinMRGraphicsRayCaster**, and **WinMRPhysicsRayCaster**. Together these three classes give you an input module for motion controllers, so you can easily handle Unity UI or 3D UI on an existing project.  See notes below, on how to use it, and there is a sample scene. 
	- **MotionControllerVisualizer** renders and tracks the WinMR motion controllers in a scene. This class is a very slightly modified version of the MotionControllerVisualizer from the [Windows Mixed Reality Toolkit for Unity](https://github.com/microsoft/mixedrealitytoolkit-unity). The only reason for this copy is that some folks have found it cumbersome to include the whole toolkit in their projects. Here, if you copy the MotionControllers folder into your project you get something lighter/easier to incorporate into existing project code.
	- **BoundaryGeometryVisualizer** helps you draw the tracked Area for Windows Mixed reality (when in room scale).
 
- **WinMRSnippets-Examples** includes examples for some of the classes above and a few extra scenarios that did not warrant a reusable class. Here is what you can look at: 
	- **ControllerListenersForLearning** has scenes and scripts that you can use to get familiar with the various Unity APIs to track WinMR controllers.  The **UnityInputListener** class uses Unity's Input APIs (which work on Steam & Rift) and **InteractionManager** sample uses Unity's WSA specific wrapper for controllers.  The latter is the recommended way for tracking motion controllers in Windows MR. 
	- **SpacesAndXRDevice** shows you how both Stationary and Room scale configurations work in WinMR  
	- **Voice** sample shows you how to use Unity's **KeywordRecognizer** for voice commands.  
 

## How to use the Input Module Sample # 
- To try the input module check the InputModuleTest scene. 
- Ensure your event system has a WinMRInputModule and no standalone input module 
- Ensure you have a WinMRPhysicsRaycaster in the scene- if you want to use 3D objects -- you don't need it otherwise 
- Ensure you have a WinMRGraphicsRaycaster in your canvas to  UI objects  

- Here are options to know about in WinMRInputModule  
  - _AllowedSelectInput_ lets you choose if you want to use gaze (after a timeout), motion controller, or gamepad (A) to select.  You can choose multiple ones, but note that in current 'single' mode, you can only have one active between gaze and controller. If the controller is active, gaze timeout won't be used. 
  - _AllowedCursorInput_ chooses what you can use as a cursor. The default is gaze but after a controller is pressed, we switch to controller. Turn controller off to get back to gaze. 
  - TimeToGazeClick == the length in seconds for a control to have focus when gaze automatically clicks (default is 3 seconds). Make sure if using debug cursor that it matches CircularLoader's FinalTime.. or make FinalTime 0. 
  

## Feedback and/or requests 
If you have any feedback please file an issue. If there is a sample you want to see included please request it too. 


