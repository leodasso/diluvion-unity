using UnityEngine;
using System;
using Sirenix.OdinInspector;

[Serializable]
public class Atmosphere {



	[Header("Fog")]
	[HideLabel]
	public Color fogColor;
	
	/*
	[HideLabel]
	public Color aboveColor;
    [HideLabel]
	public Color belowColor;
	*/

    [Header("Ambient Light")]
    [HideLabel]
    public Color ambientLightColor;

    [Header("Directional Light")]
    [HideLabel]
    public Color directionalLightColor = Color.white;

	public Vector2 fogDist;

    [MinValue(0)]
	public float bloomIntensity;

	//[MinValue(0)] 
	//public float fogExposure = 1;


    [MinValue(0)]
    public float directionalLightIntensity;
}
