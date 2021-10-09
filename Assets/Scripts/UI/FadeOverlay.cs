using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/*An independant canvas that displays on top of all other UI. Used for transitioning between scenes.
 */
public class FadeOverlay : MonoBehaviour {

    public static FadeOverlay instance;

    public CanvasGroup canvasGroup;
    public Image overlayImage;
    public delegate void OnTransition();
    public OnTransition onTransition;

    bool didJiggle;

    #region static functions

    /// <summary>
    /// Returns an instance of the fade overlay. If one doesn't already exist, creates one.
    /// </summary>
    public static FadeOverlay FadeInstance()
    {
        if ( instance != null ) return instance;
        return CreateInstance();
    }

    /// <summary>
    /// Creates an instance of the fade overlay, and returns the instance.
    /// </summary>
    public static FadeOverlay CreateInstance()
    {
        if ( instance != null ) return instance;

        GameObject go = Resources.Load("fade overlay") as GameObject;
        GameObject instanceGO = Instantiate(go, Vector3.zero, Quaternion.identity);
        instance = instanceGO.GetComponent<FadeOverlay>();

        return instance;
    }

    /// <summary>
    /// Fades the overlay in. This brings in the black (or selected color) overlay on top of everything else, 
    /// including UI.
    /// </summary>
    public static void FadeIn()             { FadeIn(1, Color.black); }
    public static void FadeIn(float time)   { FadeIn(time, Color.black); }
    public static void FadeIn(float time, Color color) { FadeIn(time, color, null); }
    public static void FadeIn(float time, Color color, OnTransition onTransitionAction)
    {
        FadeInstance().I_FadeIn(time, color);

        if ( onTransitionAction != null )
            FadeInstance().onTransition += onTransitionAction;
    }

    public static void FadeOut() {                          FadeOut(1); }
    public static void FadeOut(float time) {                FadeOut(time, Color.black); }
    public static void FadeOut(float time, Color color) {   FadeOut(time, 0, color);  }
    public static void FadeOut(float time, float delayTime, Color color)
    {
        FadeInstance().I_FadeOut(time, delayTime, color);
    }

    public static void FadeInThenOut(float time, Color color) { FadeInThenOut(time, color, null); }
    public static void FadeInThenOut(float time, Color color, OnTransition onTransitionAction)
    {
        FadeInstance().I_FadeInThenOut(time, time, color);

        if ( onTransitionAction != null )
            FadeInstance().onTransition += onTransitionAction;
    }

    #endregion

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (canvasGroup == null)
            canvasGroup = GetComponentInChildren<CanvasGroup>();

        if ( overlayImage == null )
            overlayImage = GetComponentInChildren<Image>();

    }


    public void I_FadeIn(float fadeTime, Color color) {     StartCoroutine(FadeInInstance(fadeTime, color)); }
    IEnumerator FadeInInstance(float fadeTime, Color color)
    {
        overlayImage.color = color;
        yield return StartCoroutine(Fade(canvasGroup.alpha, 1, fadeTime));
        
        // Call the transition
        onTransition?.Invoke();
        onTransition = null;

        // Hold the overlay at full alpha for a second
        yield return StartCoroutine(Fade(1, 1, 1));
    }


    public void I_FadeOut(float fadeTime, float delayTime, Color color) {    StartCoroutine(FadeOutInstance(fadeTime, delayTime, color)); }
    IEnumerator FadeOutInstance(float fadeTime, float delayTime, Color color)
    {
        // Wait for the delay time
        float t = 0;
        while (t < delayTime )
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }    

        overlayImage.color = color;
        yield return StartCoroutine(Fade(canvasGroup.alpha, 0, fadeTime));
    }


    public void I_FadeInThenOut(float fadeTimeIn, float fadeTimeOut, Color color)
    {
        StartCoroutine(FadeInThenOutInstance(fadeTimeIn, fadeTimeOut, color));
    }
    
    IEnumerator FadeInThenOutInstance(float fadeTimeIn, float fadeTimeOut, Color color)
    {
        yield return StartCoroutine(FadeInInstance(fadeTimeIn, color));
        yield return StartCoroutine(FadeOutInstance(fadeTimeOut, 0, color));
    }

    /// <summary>
    /// Fades from start alpha to end alpha, in the given fadeTime (in seconds).
    /// </summary>
    /// <param name="fadeTime">Time in seconds the fade will take.</param>
    /// <returns></returns>
    IEnumerator Fade(float startAlpha, float endAlpha, float fadeTime)
    {
        float lerp = 0;
        float alpha = startAlpha;

        // jiggle to fix a bug with the canvas sorting orders
        if (!didJiggle) {
            canvasGroup.alpha = 0;
            yield return null;
            canvasGroup.alpha = 1;
            GetComponent<Canvas>().sortingOrder = 6;
            didJiggle = true;
        }

        while ( lerp < 1 )
        {
            lerp += Time.unscaledDeltaTime / fadeTime;
            alpha = Mathf.Lerp(startAlpha, endAlpha, lerp);
            canvasGroup.alpha = alpha;
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }
}
