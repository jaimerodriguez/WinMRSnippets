using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class CircularLoader : MonoBehaviour
{
    
	// these are the color propierties of the loading bar
    public Color start;
    public Color end;
    private Color current;
	float speed;

    // these are the values of the filling amount that are displayed for the developper
	public Image CircleImage;
	public float initialValue;
	public float endValue;
	private float value;
	public float elapsed;
     
	//this is the final time it takes to the loading bar to complete
	public float FinalTime = 0.0f ;

    private bool UseGazeForSelect {
        get
        {
            return  WinMRSnippets.Samples.Input.WinMRInputModule.Instance.UseGazeForSelect;
        }
    } 
    void Start()
    {       
    	//set the loading bar characteristics
        if ( CircleImage != null )
        {
            Debug.Assert(CircleImage.type == Image.Type.Filled);
            Debug.Assert(CircleImage.fillMethod == Image.FillMethod.Radial360); 
        }

        start.a = 1.0f;
        end.a = 1.0f; 

         
        if ( FinalTime == 0f )
        {
            FinalTime = WinMRSnippets.Samples.Input.WinMRInputModule.Instance.TimeToGazeClick; 
        }
		// get the filling speed and set the evolutionary time variables
		speed=1/FinalTime;

        Debug.Log("Circular loader started ");
    }


    // this part prepares the loading efffect 
    public void StartGazeTimerAnimation()
    {
        if (UseGazeForSelect)
        {
            StopAllCoroutines();
            StartCoroutine(ShowLoadingProgress());
        }
    }

    public void StopGazeTimerAnimation ()
    {
        if (UseGazeForSelect)
        {
            if (value < endValue)
            {
                value = endValue;
                CircleImage.color = start;
                CircleImage.fillAmount = 1.0f;
            }
        } 
    }
  
	private IEnumerator ShowLoadingProgress()
	{
		value = initialValue;
		elapsed = 0;
        Debug.Log("Start progress"); 
		while (value <= endValue) 
		{           
                elapsed += Time.fixedDeltaTime;
                value = elapsed * speed;
                current = Color.Lerp(start, end, value + 0.001f);
                CircleImage.fillAmount = value;
                //set colors
                CircleImage.color = (Color)current;
                yield return new WaitForEndOfFrame();
		}
        Debug.Log("Progress completed");
    }	 
}
