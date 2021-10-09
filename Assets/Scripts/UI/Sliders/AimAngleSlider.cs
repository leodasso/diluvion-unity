using System.Collections;
using System.Collections.Generic;
using Diluvion.Ships;
using UnityEngine;

public class AimAngleSlider : SliderExtension 
{
    public override void Apply()
    {
        base.Apply();
        ShipControls.RefreshAimAngle();
    }

    public override void OnSliderAdjust()
    {
        base.OnSliderAdjust();
        ShipControls.RefreshAimAngle();
    }
}
