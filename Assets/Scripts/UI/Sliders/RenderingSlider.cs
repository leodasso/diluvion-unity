using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpiderWeb;

public class RenderingSlider : SliderExtension {

    public List<string> qualityNames;

    protected override void Start ()
    {
        base.Start();
        slider.value = QualitySettings.GetQualityLevel();
        OnSliderAdjust();
    }

    public override void OnSliderAdjust ()
    {
        base.OnSliderAdjust();
        int newValue = (int)slider.value;
        string finalName = Localization.GetFromLocLibrary(qualityNames[newValue], qualityNames[newValue]);
        settingText.text = finalName;
    }

    public override void Apply ()
    {
        base.Apply();

        QualitySettings.SetQualityLevel((int)slider.value, true);
        CameraQuality.SetQualityAll(QualitySettings.GetQualityLevel());
    }
}
