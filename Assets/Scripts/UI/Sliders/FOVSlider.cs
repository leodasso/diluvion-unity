using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Diluvion;

public class FOVSlider : SliderExtension {


    public override void OnSliderAdjust ()
    {
        base.OnSliderAdjust();

        settingText.text = FOVText();
        OrbitCam.SetFov(slider.value);
    }


    /// <summary>
    /// Returns a formatted string for the fov value
    /// </summary>
    string FOVText ()
    {
        string returnString = "";
        if (slider.value >= 0) returnString += "+";
        float roundedValue = Mathf.Round(slider.value * 10) / 10;
        return returnString + roundedValue.ToString();
    }
}