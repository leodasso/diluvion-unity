using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiScaleSlider : SliderExtension {
	public override void Apply()
	{
		base.Apply();
		UIManager.SetUiScale(slider.value);
	}
}
