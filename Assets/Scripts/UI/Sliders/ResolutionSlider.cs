using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace DUI
{

    [RequireComponent(typeof(Slider))]
    public class ResolutionSlider : SliderExtension
    {
        [ReadOnly]
        public List<Resolution> resolutions = new List<Resolution>();

        bool setup = false;

        // Use this for initialization
        protected override void Start ()
        {
            base.Start();

            resolutions.AddRange(Screen.resolutions);
            Resolution currentRes = Screen.currentResolution;

            int resIndex = 0;
            int i = 0;
            foreach (Resolution res in resolutions)
            {
                if (res.height == Screen.currentResolution.height && res.width == currentRes.width)
                {
                    resIndex = i;
                    break;
                }
                i++;
            }

            // Set resolution slider max & current value
            slider.maxValue = resolutions.Count - 1;
            slider.value = resIndex;

            setup = true;

            OnSliderAdjust();
        }


        public override void OnSliderAdjust ()
        {
            if (!setup) return;

            base.OnSliderAdjust();
            int newValue = (int)slider.value;
            settingText.text = resolutions [newValue].ToString();
        }

        public override void Apply ()
        {
            base.Apply();
            Resolution currentRes = resolutions[(int)slider.value];
            Screen.SetResolution(currentRes.width, currentRes.height, Screen.fullScreen);
        }
    }
}