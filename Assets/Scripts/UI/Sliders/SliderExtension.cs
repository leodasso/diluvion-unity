using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using Diluvion;

[RequireComponent(typeof(Slider))]
public class SliderExtension : MonoBehaviour
{
    public bool loadPrefsValue = false;

    [ShowIf("loadPrefsValue")]
    public string prefsKey = "player prefs key";

    [ShowIf("loadPrefsValue")]
    public float defaultValue = 1;

    public TextMeshProUGUI settingText;

    protected Slider slider;

    // Use this for initialization
    protected virtual void Start ()
    {
        slider = GetComponent<Slider>();
        if (settingText) settingText.text = "";

        if (loadPrefsValue)
            slider.value = PlayerPrefs.GetFloat(prefsKey, defaultValue);
    }

    public void Bump (float amt)
    {
        if (slider)
            slider.value += amt;

        OnSliderAdjust();
    }

    public virtual void OnSliderAdjust ()
    {
        if (loadPrefsValue)
            PlayerPrefs.SetFloat(prefsKey, slider.value);

        OrbitCam.RefreshPlayerPrefsValues();
    }

    public void ShowSliderValue ()
    {
        if (settingText) settingText.text = SpiderWeb.Calc.NiceRounding(slider.value).ToString();
    }

    /// <summary>
    /// Applies the settings of this slider.
    /// </summary>
    public virtual void Apply()
    {    }
}
