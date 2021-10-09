using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Diluvion;
using SpiderWeb;
using Rewired;

public class JoySensSlider : SliderExtension {

    protected override void Start ()
    {
        base.Start();
        slider.value = PlayerPrefs.GetFloat(OrbitCam.prefsJoyCamSens, 1);
    }

    public override void OnSliderAdjust ()
    {
        base.OnSliderAdjust();

        // return the old sensetivity values to 1 (since we're not using this anymore)
        ReInput.mapping.GetInputBehavior(0, "Camera").joystickAxisSensitivity = 1;
        ReInput.userDataStore.Save();

        PlayerPrefs.SetFloat(OrbitCam.prefsJoyCamSens, slider.value);
        OrbitCam.RefreshPlayerPrefsValues();

        settingText.text = JoySensetivity();
    }



    /// <summary>
    /// Returns string showing the joystick sensitivity
    /// </summary>
    string JoySensetivity ()
    {
        //float value = ReInput.mapping.GetInputBehavior(0, "Camera").joystickAxisSensitivity;
        float value = PlayerPrefs.GetFloat(OrbitCam.prefsJoyCamSens, 1);
        return Calc.NiceRounding(value, 2).ToString();
    }


}
