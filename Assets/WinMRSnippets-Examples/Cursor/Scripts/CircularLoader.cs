using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class CircularLoader : MonoBehaviour
{
    
	// these are the color properties of the loading bar
    public Color GazeActionTimerStart;  
    public Color GazeActionTimerEnd;
    
    // these are the values of the filling amount that are displayed for the developper
	public Image CircleImage;
	public float initialValue;
	public float endValue;
	private float value;
	public float elapsed;


    //this is the final time it takes to the loading bar to complete
    public float FinalTime = 0.0f;


    private Color current;
    private float speed;



    private bool UseGazeForSelect {
        get
        {
            return  WinMRSnippets.WinMRInputModule.Instance.UseGazeForSelect;
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

        GazeActionTimerStart.a = 1.0f;
        GazeActionTimerEnd.a = 1.0f; 

         
        if ( FinalTime == 0f )
        {
            FinalTime = WinMRSnippets.WinMRInputModule.Instance.TimeToGazeClick; 
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
                CircleImage.color = GazeActionTimerStart;
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
                current = Color.Lerp(GazeActionTimerStart, GazeActionTimerEnd, value + 0.001f);
                CircleImage.fillAmount = value;
                //set colors
                CircleImage.color = (Color)current;
                yield return new WaitForEndOfFrame();
		}
        Debug.Log("Progress completed");
    }	 
}
