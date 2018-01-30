using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WinMRSnippets; 

public class StaticControllerRenderer : MonoBehaviour {


    public bool RenderOnlyOne = true ;
    public bool ShowPartNames = true; 
    public Transform Parent;
    public Material GLTFMaterial; 

    private int renderedCount = 0;
    private List<string> controllersInProgress = new List<string>(); 
     
	// Use this for initialization
	void Start () {
        ControllerModelProvider.Instance.ControllerModelAvailable += Instance_ControllerModelAvailable; 
        ControllerModelProvider.Instance.StartListening();
        
    }

    private void Instance_ControllerModelAvailable(object sender, NewControllerModelAvailableEventArgs e)
    {
        Debug.Log("Controller available" + e.ProductId );
        if (RenderOnlyOne && renderedCount > 0)
            return;

        // this line prevents us from loading the same controller twice.. 
        // feel free to filter if you want only one.. 
        // product id includes handedness so you can try that too..  
        if ( !controllersInProgress.Contains(e.ProductId)  && 
            ( controllersInProgress.Count == 0 || !RenderOnlyOne ) )
        {
            controllersInProgress.Add(e.ProductId); 
            //TODO 
            GameObject go = new GameObject();
            go.transform.parent = Parent;
            go.name = "Controller" + e.ProductId;          
            StartCoroutine(Attach(go, Parent, e.ProductId));
        } 
         
    }


    private IEnumerator Attach(GameObject target, Transform parent, string productId)
    {
        Debug.Log("Attaching");
        yield return ControllerHelpers.AttachModelById (target, parent, productId,  GLTFMaterial, GLTFMaterial);

        target.transform.localPosition = Camera.main.transform.position;
        target.transform.Rotate( new Vector3( -90, 0, 0 )); 

        Debug.Log(target.transform.position);
        Debug.Log(target.transform.rotation.eulerAngles.ToString());
        // Do what ever extra procesing ... 

        controllersInProgress.Remove(productId);
        renderedCount++;
         
        if ( RenderOnlyOne )
        {
            StopListening(); 
        }

        if ( ShowPartNames )
        {
            var partsRenderer = GetComponent<MotionControllerPartsLabelRenderer>();
            if ( partsRenderer != null )
                partsRenderer.MotionController = target; 
        }

       
    }

    // Update is called once per frame
    void Update () {
		
	}

    void StopListening ()
    {
        if (ControllerModelProvider.Instance.IsListening)
        {
            ControllerModelProvider.Instance.ControllerModelAvailable -= Instance_ControllerModelAvailable; 
            ControllerModelProvider.Instance.StopListening();
    
        }
    }
    private void OnDestroy()
    {
        StopListening(); 
    }
}
