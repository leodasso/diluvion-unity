using System.Collections;
using System.Collections.Generic;
using Diluvion.Ships;
using UnityEngine;


public class TutorialCaller : Trigger {

    public TutorialObject tutorial;
    [Tooltip("If null, just uses the transform on this game object.")]
    public Transform tutorialFocus;
    public float delay;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        if (!tutorialFocus) tutorialFocus = transform;
	}

    public override void TriggerAction(Bridge otherBridge)
    {
        base.TriggerAction(otherBridge);
        OpenTutorial();
    }


    public void OpenTutorial()
    {
        StartCoroutine(ShowTut());
    }

    IEnumerator ShowTut()
    {
        float t = 0;
        while (t < delay)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        DUI.TutorialPanel.ShowTutorial(tutorial, tutorialFocus);
    }
}
