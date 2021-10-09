using UnityEngine;
using System.Collections;
using HeavyDutyInspector;


public class DUIFadeAndMove : MonoBehaviour
{

    [ComplexHeader("Component for moving UI on and off screen over time", Style.Box, Alignment.Center, ColorEnum.White, ColorEnum.Gray)]
 
    [Button("Set On Position", "SetOnPos",true)]
    public bool setOnPos;
    public Vector2 onAnchorPos;

    [Button("Set Off Position", "SetOffPos", true)]
    public bool setOffPos;
    public Vector2 offAnchorPos;

    public float fadeInTime = 1;
    public float moveInTime = 1;

    float moveProgress = 0;
    float fadeProgress = 0;
    bool visible = false;
    bool onScreen = false;
    Rect progressBarRect;
    RectTransform myRT;
    CanvasGroup cGroup;

    // Use this for initialization
    void Awake ()
    {
   
        myRT = GetComponent<RectTransform>();
        cGroup = GetComponent<CanvasGroup>();

    }

    void SetOnPos()
    {
        onAnchorPos = GetComponent<RectTransform>().anchoredPosition;
    }

    void SetOffPos()
    {
        offAnchorPos = GetComponent<RectTransform>().anchoredPosition;
    }

    public void Move(bool into)
    {
        //Dont start several of the same coroutine
        if (into == onScreen) return;
        StopCoroutine("MoveLerp");
        StartCoroutine("MoveLerp", into);
    }

   
    float targetMove = 1;
    IEnumerator MoveLerp(bool into)
    {
        onScreen = into;
        if (into)
            targetMove = 1;
        else
            targetMove = 0;
       
        while (moveProgress!= targetMove)
        {
            myRT.anchoredPosition = Vector2.Lerp(offAnchorPos, onAnchorPos, moveProgress);
            yield return new WaitForEndOfFrame();
            moveProgress = Mathf.MoveTowards(moveProgress, targetMove, Time.deltaTime/(moveInTime+0.1f));
        }               
    }

    public void Fade(bool vis)
    {
        //Dont start several of the same coroutine
        if (vis == visible) return;
        StopCoroutine("FadeLerp");
        StartCoroutine("FadeLerp", vis);
    }

    public IEnumerator ReturnFade(bool into)
    {
        yield return FadeLerp(into);
    }

    float targetFade = 1;
    IEnumerator FadeLerp(bool into)
    {
        visible = into;
        if (into)
            targetFade = 1;
        else
            targetFade = 0;

        while (fadeProgress != targetFade)
        {
            cGroup.alpha = fadeProgress;
            yield return new WaitForEndOfFrame();
            fadeProgress = Mathf.MoveTowards(fadeProgress, targetFade, Time.deltaTime/(fadeInTime+0.01f));
        }
    }  
}
