using UnityEngine;
using Rewired;
using SpiderWeb;

public class MouseSensSlider : SliderExtension {

    protected override void Start ()
    {
        base.Start();
        slider.value = ReInput.mapping.GetInputBehavior(0, "Camera").mouseXYAxisSensitivity;

        OnSliderAdjust();
    }

    public override void OnSliderAdjust ()
    {
        base.OnSliderAdjust();

        ReInput.mapping.GetInputBehavior(0, "Camera").mouseXYAxisSensitivity = slider.value;
        settingText.text = MouseSensetivity();
        ReInput.userDataStore.Save();
    }

    string MouseSensetivity ()
    {
        float value = ReInput.mapping.GetInputBehavior(0, "Camera").mouseXYAxisSensitivity;
        return Calc.NiceRounding(value, 2).ToString();
    }
}