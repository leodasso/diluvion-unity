using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HeavyDutyInspector;


public class TweakBaseMaterial : MonoBehaviour {

    public enum PropertyToTweak { Emission };
    public PropertyToTweak tweak;
    public Material mat;
    [SerializeField]
    Material matCopy;
    public float timeToLerp = 1;
    public float startValue = 0.01f;
    public float endValue = 1;
    [SerializeField]
    List<Renderer> mr = new List<Renderer>();

    [Button("Setup", "Setup", true)]
    public bool setup;
    [Button("Start", "StartTweaking", true)]
    public bool start;
    [Button("Reset Tweak", "Reset", true)]
    public bool reset;

    bool tweaking = false;
    [SerializeField]
    Color emissionColor;

    public void StartTweaking()
    {
        tweaking = true;
    }
    void Setup()
    {

        mr = new List<Renderer>(GetComponentsInChildren<Renderer>());
        matCopy = new Material(mat);
        emissionColor =  matCopy.GetColor("_EmissionColor");
        foreach (Renderer r in mr)
        {
            r.useLightProbes = false;
            r.reflectionProbeUsage = 0;
            r.sharedMaterial = matCopy;
        }

    }

	// Update is called once per frame
	void Update ()
    {

        if (!tweaking) return;
        if(tweak == PropertyToTweak.Emission)
        {
            LerpEmission();
        }

	}

    
    float emissionTime = 0;
    float emissionValue = 0;

    public void Reset()
    {
        emissionTime = 0;
        tweaking = false;
    }
    Color finalColor;
    public void LerpEmission()
    {
        emissionTime += Time.deltaTime;
        if (emissionTime > timeToLerp) return;
        emissionValue = Mathf.Lerp(startValue, endValue, emissionTime / timeToLerp);
        emissionColor = Color.white * emissionValue;
        matCopy.SetColor("_EmissionColor", emissionColor);     
      

    }
}
