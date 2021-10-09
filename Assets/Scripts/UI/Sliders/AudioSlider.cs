using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpiderWeb;

public class AudioSlider : SliderExtension {

    public string rtpcName;

    protected override void Start ()
    {
        base.Start();
        slider.onValueChanged.AddListener(delegate { SetAudio(); });
        slider.value = SpiderSound.GetRtpc(rtpcName, null);
    }

    public void SetAudio ()
    {
        PlayerPrefs.SetFloat(rtpcName, slider.value);
        SpiderSound.TweakRTPC(rtpcName, slider.value, null);
    }
}
