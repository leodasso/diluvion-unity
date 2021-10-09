using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DUIFullScreenOverlay : MonoBehaviour 
{
    public CanvasGroup cGroup;
    public float fadeTime = 1;
    public float holdTime = 1;

    float startTime = 0;


    IEnumerator Start()
    {

        if ( cGroup == null ) cGroup = GetComponentInChildren<CanvasGroup>();
        if (cGroup == null)
        {
            Debug.LogError(gameObject.name + " has no canvas group!");
            Destroy(gameObject);
        }

        cGroup.alpha = 1;

        yield return new WaitForSeconds(holdTime);

        FadeOut();
        yield break;
    }

    public void FadeIn()
    {
        StartCoroutine(Fade(0, 1));
    }

    public void FadeOut()
    {
        StartCoroutine(Fade(1, 0));
    }

    IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float lerp = 0;
        float alpha = 0;

        while (lerp < 1)
        {
            lerp += Time.deltaTime / fadeTime;
            alpha = Mathf.Lerp(startAlpha, endAlpha, lerp);
            cGroup.alpha = alpha;
            yield return null;
        }

        yield break;
    }
}
