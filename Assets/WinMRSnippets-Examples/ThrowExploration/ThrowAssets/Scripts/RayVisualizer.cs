using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(LineRenderer))]
public class RayVisualizer : MonoBehaviour {

    public Vector3 startPosition;
    public Vector3 endPosition;


    [SerializeField]
    private Color color = Color.black;

    [SerializeField]
    private float width = 0.05f;

    public float Width
    {
        get { return width;  }
        set
        {
            width = value; 
            if ( lineRenderer != null)
            {
                lineRenderer.SetWidth(width, width); 
            }
        }
    }
    public Color Color
    {
        get { return color;  }
        set {
            color = value;
            if (lineRenderer != null)
            {
                lineRenderer.SetColors(color, color);
                lineRenderer.GetComponent<Renderer>().material.color = color; 
            } 
        }
    }

    public int Index
    {
        get
        {
            return index;
        }
        set
        {
            SetIndex(value);
        }
    }


    private LineRenderer lineRenderer;
    [SerializeField]
    private int index = -1 ;


    // Use this for initialization
    void Start () {
        lineRenderer = GetComponent<LineRenderer>(); 
        if ( lineRenderer == null )
        {
            Debug.LogError("requires linerender");
            Destroy(this); 
        }
        SetIndex(Index);
        Color = color;
        Width = width; 

	}
	
	// Update is called once per frame
	void Update () {
	    if ( index != -1 && lineRenderer != null )
        {
            lineRenderer.SetPosition(0, startPosition);
            lineRenderer.SetPosition(1, endPosition); 
        }
	}


    private void SetIndex(int newIndexValue )
    {
        index = newIndexValue; 
       // Color = colors[newIndexValue % colors.Length];
                
    }

    static Color[] colors = new Color[]
    {
        Color.red ,
        Color.green ,
        Color.yellow ,
        Color.blue , 
        Color.black   
         
    }; 

    public void SetVectors ( Vector3 start, Vector3 end )
    {
        startPosition = start;
        endPosition = end; 
    }    
}
