using UnityEngine;
using System.Collections;
using HeavyDutyInspector;

public class DUITension : MonoBehaviour
{
    public DUIFadeAndMove fadeAndMove;
    public DUIProgressBar progressBar;
    
    bool snapped;
    bool dying;
    void Start()
    {
        fadeAndMove.Move(true);
    }

    public void SetTension(float t)
    {
        progressBar.SmoothSetProgressBar(t);
    }

    public void Kill()
    {
        if (dying) return;
        StartCoroutine(KillMe());
    }

    IEnumerator KillMe()
    {
        dying = true;
        yield return fadeAndMove.ReturnFade(false);
        Destroy(gameObject);
    }

    #region Test Buttons

    [Button("Move In", "MoveIn", true)]
    public bool moveIn;
    [Button("Move Out", "MoveOut", true)]
    public bool moveOut;
    [Button("Fade In", "FadeIn", true)]
    public bool fadeIn;
    [Button("Fade Out", "FadeOut", true)]
    public bool fadeOut;
    [Button("Set Progress", "SetProgress", true)]
    public bool setProgress;
    public float targetProgressbarAmount;

    void MoveIn()
    {
        fadeAndMove.Move(true);
    }

    void MoveOut()
    {
        fadeAndMove.Move(false);
    }
    
    void FadeIn()
    {
        fadeAndMove.Fade(true);
    }

    void FadeOut()
    {
        fadeAndMove.Fade(false);
    }

    void SetProgress()
    {
        progressBar.SmoothSetProgressBar(targetProgressbarAmount);
    }
    #endregion
}
