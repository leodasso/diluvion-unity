using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class DUITravelSelector : MonoBehaviour {

    float alpha;
    CanvasGroup canvas;

	// Use this for initialization
	void Start () {

        canvas = GetComponent<CanvasGroup>();
        canvas.alpha = 0;
        alpha = 0;
	}
	
	// Update is called once per frame
	void Update () {

        // animate fading of transparency
        canvas.alpha = Mathf.Lerp(canvas.alpha, alpha, Time.unscaledDeltaTime * 8);
	
	}

    /// <summary>
    /// Places the selector over the given object and fades in
    /// </summary>
    /// <param name="selected"></param>
    public void ApplySelection(GameObject selected)
    {
        alpha = 1;
        transform.position = selected.transform.position;
    }

    /// <summary>
    /// Fades out the selector
    /// </summary>
    public void RemoveSelection()
    {
        alpha = 0;
    }
}
