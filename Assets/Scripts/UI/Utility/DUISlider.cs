using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class DUISlider : MonoBehaviour {

    public Slider slider;
    public TextMeshProUGUI currentSetting;
    public TextMeshProUGUI min;
    public TextMeshProUGUI max;

	// Use this for initialization
	protected virtual void Start ()
    {
        if (!slider) return;

        // Set the min and max labels
        if (min) min.text = slider.minValue.ToString();
        if (max) max.text = slider.maxValue.ToString();
	}
	
    protected virtual string CurrentSettingText()
    {
        return "current setting";
    }

    public virtual void SliderAdjusted(float newValue)
    {
    }
}
