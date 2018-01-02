# WinMRSnippets
Sample code snippets for Windows Mixed Reality Application Developers


Until this gets feature, code complete & stable, here is a brief log. 


**12/4 Status:  Initial check-in code is quite raw...  Expect two days of clean-up are needed, but public interfaces won't change much.** 


# How to use te Input Module Sample # 
- To try the input module check the InputModuleTest scene. 
- Ensure your event system has a WinMR Input Module and no standalone input module 
- Ensure you have a WinMRPhysicsRaycaster in the scene- if you want to use 3D objects -- you don't need it otherwise 
- Ensure you have a WinMRGraphicsRaycaster in your canvas to  UI objects  

- Here are options to know about in WinMRInputModule  
  - _AllowedSelectInput_ let's you choose if you want to use gaze (after a timeout), motion controller, or gamepad (A) to select.  You can choose multiple ones, but note that in current 'single' mode, you can only have one active between gaze and controller.     
So, if controler is active, gaze timeout won't be used. 
  - _AllowedCursorInput_ chooses what you can use as cursor. Default is gaze but after a controller is pressed, we swith to controller. Turn controller off to get back to gaze. 
  - TimeToGazeClick == the length in seconds for a control to have focus when gaze automatically clicks (default is 3 seconds). Make sure if using debug cursor that it matches CircularLoader's FinalTime.. or make FinalTime 0. 
  
  
# To-dos: # 

## WinMRInputModule ## 
  LayerMasks for raycasting not yet implemented 
  Weird 'focus' & selection issue as you cross roots (two siblings). The prior state is persisted. this looks like Unity issue, but will look more into it. 
  Cursor (in sample scene) is raw. needs proper normals 
  Make Input choices Masks within Unity editor  
  Proper documentation 


## Sample controller and game pad code: ##
- Outside of the extra verbosity in these samples, they should be fine. the goal is for people to learn from them. 


## All-up: ## 
- Refactoring needed everywhere. 
- Create Clean, polished samples... 
-  **Lots more** 
