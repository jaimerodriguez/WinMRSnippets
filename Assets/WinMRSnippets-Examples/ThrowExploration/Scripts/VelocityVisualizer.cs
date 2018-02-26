using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 


[Serializable]
public class  RayPairing 
{
    public string gameObjectName;
    public int index; 

    public RayPairing ( int newIndex , string name )
    {
        gameObjectName = name;
        index = newIndex; 
    }
}; 

public class VelocityVisualizer : MonoBehaviour {


    public RayPairing[] mappings;
    private Dictionary<int, RayVisualizer> map = new Dictionary<int, RayVisualizer>(); 

    // Use this for initialization
	void Start () {
        if (mappings.Length == 0)
        {
            mappings = new RayPairing[]
            {
                new RayPairing (0, "velocity" ), 
                new RayPairing (1 , "angularVelocity"),
                new RayPairing (2 , "averagedVelocity"),
                new RayPairing (3 , "combinedVelocity") 
            }; 
        }
        
        
        foreach ( var mapping in mappings )
        {
            var child = this.transform.Find(mapping.gameObjectName);  
            if (child != null )
            {
                var vis = child.gameObject.GetComponent<RayVisualizer>(); 
                if ( vis != null )
                {
                    map.Add(mapping.index, vis); 
                }
            }
        }
	}
	 
    public void SetVectors ( int index, Vector3 start, Vector3 end )
    {
        if ( map.ContainsKey(index))
        {
            map[index].SetVectors(start, end); 
        }
    }
}
