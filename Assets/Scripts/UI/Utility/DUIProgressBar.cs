using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using HeavyDutyInspector;
using SpiderWeb;

public enum BarDirection { Vertical, Horizontal };
public class DUIProgressBar : MonoBehaviour
{

    [ComplexHeader("Component for moving a progressbar", Style.Box, Alignment.Center, ColorEnum.White, ColorEnum.Gray)]
    public BarDirection barDirection;
    public float buffer;
    public float lerpSpeed = 2;
    public bool lerpColor;
    [HideConditional(true, "lerpColor", true)]
    public Color emptyColor;
    [HideConditional(true, "lerpColor", true)]
    public Color fullColor;
    Image image;
    RectTransform progressRT;
 
    Rect progressBarRect;
    float currentProgress;
    float targetProgress;

	// Use this for initialization
	void Start () {

        progressRT = GetComponent<RectTransform>();
        progressBarRect = progressRT.rect;
        image = GetComponent<Image>();
   
	}



    public void HardSetProgressBar(float percentage)
    {
        currentProgress = percentage;
        targetProgress = currentProgress;
        SetProgressBar();
    }

    public void SmoothSetProgressBar(float percentage)
    {
        targetProgress = percentage;
    }

    Color lerpedColor;
    Vector2 currentPosition = new Vector2();
    public void SetProgressBar()
    {      
        if (barDirection == BarDirection.Horizontal)
        {
            currentPosition.x = currentProgress * progressBarRect.width - progressBarRect.width+ buffer;
            currentPosition.y = 0;
        }
        else
        {
            currentPosition.x = 0;
            currentPosition.y = currentProgress * progressBarRect.height - progressBarRect.height+ buffer;
        }

        lerpedColor = Color.Lerp(emptyColor, fullColor, currentProgress);
        image.CrossFadeColor(lerpedColor, 0.1f, false, false);
        progressRT.anchoredPosition = currentPosition;
    }


    void Update()
    {
        if (currentProgress != targetProgress)
        {
			currentProgress = Mathf.Lerp(currentProgress, targetProgress, Time.deltaTime * lerpSpeed);
            SetProgressBar();
        }

    }

    /*
    //Animate the bar
    float progressBarX = TimeControl.Get().captainTimeUsage * progressBarRect.width - progressBarRect.width;
    progressBar.anchoredPosition = new Vector2(progressBarX, 0);

    // Animate the position
    Vector2 gotoPos = new Vector2(0, gotoY);
    rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, gotoPos, Time.unscaledDeltaTime * 5);*/

}
