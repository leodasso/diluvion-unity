using UnityEngine;
using System.Collections;
using SpiderWeb;
using Diluvion;

public class DepthToRTPC : MonoBehaviour {

    public float deepest = -2000;
    public float shallowest = -100;

    public float minRTPC = 0;
    public float maxRTPC = 100;

    public string rtpcName = "a name";
    public float currentValue = 0;

    float adjustedDeepest = 0;
    float depth = 0;
    float RTPCrange = 0;

    AKMusicBox musicBox;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {

        if ( OrbitCam.Get() == null ) return;
        adjustedDeepest = deepest - shallowest;
        depth = OrbitCam.Get().transform.position.y - shallowest;

        float depthRatio = depth / adjustedDeepest;

        RTPCrange = maxRTPC - minRTPC;
        float setRTPC = (RTPCrange * depthRatio) + minRTPC;

        currentValue = Mathf.Clamp(setRTPC, minRTPC, maxRTPC);

        SpiderSound.TweakRTPC(rtpcName, currentValue, null);
	}
}
