using UnityEngine;
using System.Collections;
using SpiderWeb;

public class LoadVolume : MonoBehaviour
{

    [SerializeField]
    float mVolume;
    [SerializeField]
    float sfxVolume;
    [SerializeField]
    float muVolume;
    
	// Use this for initialization
	void Start ()
    {
        mVolume = PlayerPrefs.GetFloat("Master_Volume",30f);
        sfxVolume = PlayerPrefs.GetFloat("SFX_Volume", 50f);
        muVolume = PlayerPrefs.GetFloat("Music_Volume",50f);
        
        SetAudio("Master_Volume", mVolume);
        SetAudio("SFX_Volume", sfxVolume);
        SetAudio("Music_Volume", muVolume);
    }

    void SetAudio(string audioRTPCName, float value)
    {       
        Debug.Log("Setting "+audioRTPCName+" to: " + value);
        
        SpiderSound.TweakRTPC(audioRTPCName, value, null);
    }
}
