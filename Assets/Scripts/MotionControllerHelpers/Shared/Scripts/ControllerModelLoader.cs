using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using UnityEngine.XR;
using UnityEngine.XR.WSA.Input;
 

#if UNITY_EDITOR_WIN
using System.Runtime.InteropServices;
#endif


using HoloToolkit.Unity.InputModule;
using GLTF;


#if !UNITY_EDITOR && UNITY_WSA && ENABLE_WINMD_SUPPORT
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Input.Spatial;
#endif




public class NewControllerModelAvailableEventArgs: EventArgs
{
    public string ProductId { get; private set;  }
    public byte[] Data { get; private set;  }

    public uint Id { get; private set;  }

    public NewControllerModelAvailableEventArgs ( uint id, string productId, byte[] data  )
    {
        Id = id;
        ProductId =  productId;
        Data = data; 
    }
}


public class ControllerModelProvider
{

#if UNITY_EDITOR_WIN
    [DllImport("MotionControllerModel")]
    private static extern bool TryGetMotionControllerModel([In] uint controllerId, [Out] out uint outputSize, [Out] out IntPtr outputBuffer);
#endif


    #region private members 
    private Dictionary<string, byte[]> cachedModels;
    private Dictionary<uint, string> activeControllers;
    private List<string> loadingModels;
    private static ControllerModelProvider _instance;
    #endregion 


    public bool IsListening { get; private set;  }

    public event System.EventHandler<NewControllerModelAvailableEventArgs> ControllerModelAvailable; 
    private ControllerModelProvider( )
    {
        cachedModels = new Dictionary<string, byte[]>();
        activeControllers = new Dictionary<uint, string>();
        loadingModels = new List<string>(); 
    }

    public static ControllerModelProvider Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ControllerModelProvider();
            }
            return _instance;
        }
    }
    

    public void StartListening ( )
    {
        TraceHelper.Log("ControllerModelProvider StartListening"); 
        if ( !IsListening)
        {
            Start();                         
        }
    }

#if UNITY_EDITOR
    void LoadControllerModels ()
    {           
        System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Application.streamingAssetsPath);  
        TraceHelper.Log("looking at " + Application.streamingAssetsPath ); 
        string prefix = "controller_" ; 
        System.IO.FileInfo[] files = dir.GetFiles(prefix+"*.gltf");
        foreach ( var file in files ) 
        {
            
            TraceHelper.Log("processing" + file.FullName ); 
            byte [] bytes = System.IO.File.ReadAllBytes(file.FullName);
            int index = file.FullName.IndexOf( prefix , StringComparison.CurrentCultureIgnoreCase); 
            index += prefix.Length ;
            int len = file.FullName.IndexOf(".gltf") - index; 
            string key = file.FullName.Substring(index, len );    
            TraceHelper.Log("Adding: " + key); 
            cachedModels.Add(key, bytes);  
        } 

        foreach ( var key in cachedModels.Keys )
        {

            UnityEngine.WSA.Application.InvokeOnAppThread(() =>
            {
                TraceHelper.Log("ControllerModelProvider::AddController: " + key );
                var eh = ControllerModelAvailable;
                if (eh != null)
                    eh(this, new NewControllerModelAvailableEventArgs(0 , key, cachedModels[key]));
            }, true); 
        }
    }

#endif 


    public void StopListening()
    {
        if (IsListening)
        {
            Stop();
        }
    }



    private void Start ()
    {
#if UNITY_EDITOR && UNITY_WSA 
        InteractionManager.InteractionSourceDetected += InteractionManager_InteractionSourceDetected;
        InteractionManager.InteractionSourceLost += InteractionManager_InteractionSourceLost;
        IsListening = true; 

#elif !UNITY_EDITOR && UNITY_WSA && ENABLE_WINMD_SUPPORT
        TraceHelper.Log("ControllerModelProvider Using WSA");
        UnityEngine.WSA.Application.InvokeOnUIThread(() =>
        {
            var spatialInteractionManager = SpatialInteractionManager.GetForCurrentView();           
            if (spatialInteractionManager != null)
            {
                spatialInteractionManager.SourceDetected += SpatialInteractionManager_SourceDetected;
                spatialInteractionManager.SourceLost += SpatialInteractionManager_SourceLost;
                IsListening = true ;        
            }
        }, true);
 
#endif
    }


#if UNITY_EDITOR && UNITY_WSA 
    private void InteractionManager_InteractionSourceLost(InteractionSourceLostEventArgs args)
    {
        OnSourceLost(args.state.source.id); 
    }

    private void InteractionManager_InteractionSourceDetected(InteractionSourceDetectedEventArgs args )
    {
       if ( args.state.source.kind == InteractionSourceKind.Controller )
        {
            string productId = MakeProductIdHash(args.state.source);
            if (!cachedModels.ContainsKey(productId) && !loadingModels.Contains(productId))
            {
                loadingModels.Add(productId);
                IntPtr controllerModel = new IntPtr();
                uint outputSize = 0;
                if (TryGetMotionControllerModel(args.state.source.id, out outputSize, out controllerModel))
                {
                    byte[] fileBytes;
                    fileBytes = new byte[Convert.ToInt32(outputSize)];
                    Marshal.Copy(controllerModel, fileBytes, 0, Convert.ToInt32(outputSize));
                    AddController(args.state.source.id, productId, fileBytes); 
                }
                else
                {
#if TRACING_VERBOSE || TRACING_ERROR 
                    Debug.LogError("Failed to load design-time controller model. This is not expected");                    
#endif 
                    loadingModels.Remove(productId); 
                }
            }
        }
    }


#elif !UNITY_EDITOR && UNITY_WSA && ENABLE_WINMD_SUPPORT
    private void SpatialInteractionManager_SourceLost(SpatialInteractionManager sender, SpatialInteractionSourceEventArgs args)
    {
        OnSourceLost (args.State.Source.id);  
    }

    private void SpatialInteractionManager_SourceDetected(SpatialInteractionManager sender, SpatialInteractionSourceEventArgs args)
    {       
        SpatialInteractionSource source = args.State.Source;
        if (source.Kind == SpatialInteractionSourceKind.Controller)
        {                         
            SpatialInteractionController controller = source.Controller;
            if (controller != null)
            {
                string productId = MakeProductIdHash(controller , source.Handedness );
                if (!cachedModels.ContainsKey(productId) && !loadingModels.Contains(productId) )
                {
                    loadingModels.Add(productId); 
                    LoadOffThread( args.State.Source.Id,  controller, source.Handedness ); 
                } 
            }
        }
    }

    void LoadOffThread ( uint controllerId , SpatialInteractionController controller , SpatialInteractionSourceHandedness handedness )
    {
 
     System.Diagnostics.Debug.WriteLine  ("ControllerModelProvider Loading Model");
      System.Threading.Tasks.Task.Run(() =>
      {
          try
          {
              var getModelTask = controller.TryGetRenderableModelAsync().AsTask();
              getModelTask.Wait();
              var modelStream = getModelTask.Result;
              byte[] fileBytes = new byte[modelStream.Size];
              using (DataReader reader = new DataReader(modelStream))
              {                  
                   var loadTask = reader.LoadAsync((uint)modelStream.Size).AsTask();
                   loadTask.Wait();                
                   reader.ReadBytes(fileBytes);
                   System.Diagnostics.Debug.WriteLine ("LoadOffThread read: " + fileBytes.Length); 
              }
              string productId = MakeProductIdHash(controller ,handedness );
              AddController(controllerId, productId, fileBytes ) ;

          } catch  ( System.Exception ex )
          {
              System.Diagnostics.Debug.WriteLine(ex.Message);
              System.Diagnostics.Debug.WriteLine(ex.StackTrace); 
          }
      });          
    }

#endif

    void OnSourceLost ( uint id  )
    {
#if TRACING_VERBOSE
        Debug.Log("ControllerModelLoader::SourceLost: " + id); 
#endif
    }
     

    private void Stop()
    {

#if UNITY_EDITOR && UNITY_WSA
        InteractionManager.InteractionSourceDetected -= InteractionManager_InteractionSourceDetected;
        InteractionManager.InteractionSourceLost -= InteractionManager_InteractionSourceLost;
        IsListening = false; 

#elif !UNITY_EDITOR && UNITY_WSA && ENABLE_WINMD_SUPPORT
        TraceHelper.Log ("ControllerModelProvider Stopping WSA Event listening");
        UnityEngine.WSA.Application.InvokeOnUIThread(() =>
        {
            var spatialInteractionManager = SpatialInteractionManager.GetForCurrentView();
            if (spatialInteractionManager != null)
            {
                spatialInteractionManager.SourceDetected -= SpatialInteractionManager_SourceDetected;
                spatialInteractionManager.SourceLost -= SpatialInteractionManager_SourceLost;
                IsListening = false; 
            }
        }, true);
#endif
    }


#if ENABLE_WINMD_SUPPORT
    

    

#endif




    public byte[] GetModelData( InteractionSource source )
    {
        string id = MakeProductIdHash(source);
        return GetModelData(id);
    }


 
    /// <summary>
    /// Returns model data based solely on handness.  This is all the 'data' we have when using Unity 5 (MRTP) builds and the Unity Input.Tracking APIs. Avoid this combination when possible. 
    /// </summary>
    /// <param name="node">XRNode </param>
    /// <returns>GLTF Controller model as byte[] </returns>
    public byte[] GetModelData( XRNode node )
    {
        string handedness = string.Empty;    
        if (node == XRNode.LeftHand || node == XRNode.RightHand)
        {
            handedness = (node == XRNode.LeftHand) ? Constants.LeftHandness : Constants.RightHandeness;

            foreach (var key in cachedModels.Keys )
            {
                if ( key.ToLower().Contains ( handedness ))
                {
                    return GetModelData(key); 
                }
            }
        }
        return null; 
    }


    public bool Contains(XRNode node)
    {
        string id = GetProductId(node);
        return cachedModels.ContainsKey(id);
    }

    /// <summary>
    /// Returns a productId match based solely on handness.  This is all the 'data' we have when using Unity 5 (MRTP) builds and the Unity Input.Tracking APIs. Avoid this combination when possible. 
    /// NOTE: This method will not return a product id for models not yet cached. 
    /// </summary>
    /// <param name="node">XRNode </param>
    /// <returns>Controller productId hash</returns>
    public string GetProductId(XRNode node)
    {
        if (node == XRNode.LeftHand || node == XRNode.RightHand)
        {
            string handedness = (node == XRNode.LeftHand) ? Constants.LeftHandness : Constants.RightHandeness;
            foreach (var key in cachedModels.Keys)
            {
                if (key.ToLower().Contains(handedness))
                {
                    return key;
                }
            }
        }
        return string.Empty;
    }
 



    public bool Contains(string productId )
    {        
        return cachedModels.ContainsKey(productId );
    }

    public string GetProductId(InteractionSource source)
    {
        string id = MakeProductIdHash(source); 
        return id; 
    }

   
 

    public byte[]  GetModelData ( string id )
    {
        if (cachedModels.ContainsKey(id))
        {
            return cachedModels[id];
        }
        else
        {
            throw new System.InvalidOperationException("Model not found. Call Load Model Data instead of Get Model Data");
        }
    }
   
    public bool Contains( InteractionSource  source )
    {
        string id = MakeProductIdHash(source );
        return cachedModels.ContainsKey(id);
    }


    public bool Contains (uint id )
    {
        return activeControllers.ContainsKey(id);  
    }
  

    private string MakeProductIdHash( InteractionSource source )
    {
        ushort vendorId = 0;
        ushort productId = 0;
        ushort productVersion = 0;
#if UNITY_5
        InteractionController c = new InteractionController();
        if(source.TryGetController(out c))
        {
            vendorId = c.vendorId;
            productId = c.productId;
            productVersion = c.productVersion;
        }
#else
            vendorId = source.vendorId;
            productId = source.productId;
            productVersion = source.productVersion;
#endif
        return string.Format("{0}-{1}-{2}-{3}", vendorId , productId, productVersion , source.handedness ).ToLower() ;
    }

    // void AddController(uint id, SpatialInteractionController controller, byte[] data, SpatialInteractionSourceHandedness handedness)
    void AddController(uint id, string productId , byte[] data )
    {         
        if (loadingModels.Contains(productId))
        {
            loadingModels.Remove(productId);
        }

        if (cachedModels.ContainsKey(productId))
        {
            if (!activeControllers.ContainsKey(id))
            {
                activeControllers.Add(id, productId);
            }
        }
        else
        {
            cachedModels.Add(productId, data);
            activeControllers.Add(id, productId);
        }


        UnityEngine.WSA.Application.InvokeOnAppThread(() =>
        {
            TraceHelper.Log("ControllerModelProvider::AddController: " + productId);
            var eh = ControllerModelAvailable;
            if (eh != null)
                eh(this, new NewControllerModelAvailableEventArgs(id, productId, data));
        }, true);
    }

    #region WINMD 

#if ENABLE_WINMD_SUPPORT
    public bool Contains (  SpatialInteractionController controller , SpatialInteractionSourceHandedness handedness)
    {
        string id = MakeProductIdHash(controller, handedness );
        return cachedModels.ContainsKey (id); 
    }


    private string MakeProductIdHash ( SpatialInteractionController controller, SpatialInteractionSourceHandedness handedness)
    {
        return string.Format("{0}-{1}-{2}-{3}", controller.VendorId, controller.ProductId, controller.Version , handedness ).ToLower(); 
    }

   

    private string MakeFileName(string productid)
    {
        return Constants.ControllerFilePrefix + productid + Constants.GLTFFileExtension;
    }

    private void SaveModel ( string productId , byte [] fileBytes )
    {
        System.Diagnostics.Debug.WriteLine ("Saving Model: " + productId); 
        try
        {
            var folder = Windows.Storage.ApplicationData.Current.TemporaryFolder;
            string name = MakeFileName(productId); 
            var createFileTask = folder.CreateFileAsync(name, Windows.Storage.CreationCollisionOption.ReplaceExisting).AsTask();
            createFileTask.Wait();
            Windows.Storage.StorageFile file = createFileTask.Result;
            Windows.Storage.FileIO.WriteBytesAsync(file, fileBytes).AsTask().Wait();            
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine (ex.Message);
            System.Diagnostics.Debug.WriteLine(ex.StackTrace);

        }         
    }

    byte [] LoadModel (  Windows.Storage.StorageFolder folder, string path )
    {

        try
        {
            var task = folder.GetFileAsync(path).AsTask();
            task.Wait();
            var streamTask = task.Result.OpenReadAsync().AsTask();
            streamTask.Wait();
            using (var stream = streamTask.Result)
            {
                using (var reader = new Windows.Storage.Streams.DataReader(stream))
                {
                    var len = stream.Size;
                    reader.LoadAsync((uint)len).AsTask().Wait();
                    byte[] content = new byte[len];
                    reader.ReadBytes(content);
                    return content;
                }
            }
        } catch ( Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.Message);
            System.Diagnostics.Debug.WriteLine(ex.StackTrace); 
        }
        return null;          
    }
#endif


    #endregion

}