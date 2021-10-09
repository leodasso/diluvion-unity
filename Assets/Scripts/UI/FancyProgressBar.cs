using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace DUI
{
    public class FancyProgressBar : MonoBehaviour
    {
        /// <summary>
        /// Value of the progress bar. 0 is empty, 1 is full.
        /// </summary>
        [Range(0, 1)]
        public float value;

        [Tooltip("Enter another progress bar here to copy its value in update.")]
        public FancyProgressBar copyCat;

        [Tooltip("The sprite that will be filling.")]
        public Image progressBar;

        [ToggleGroup("adjustColor")]
        public bool adjustColor;

        [ToggleGroup("adjustColor")] 
        public float colorShiftSpeed = 25;
        
        [ToggleGroup("adjustColor")] 
        public float colorReturnSpeed = 5;

        [ToggleGroup("adjustColor")]
        [Tooltip("If left empty, just colors the progress bar image.")]
        public Image separateImageForColor;

        [ToggleGroup("adjustColor")]
        public Color normalColor = Color.white;

        [ToggleGroup("adjustColor")]
        public Color addColor = Color.green;

        [ToggleGroup("adjustColor")]
        public Color removeColor = Color.red;

        [ToggleGroup("animate")]
        public bool animate;

        [ToggleGroup("animate")]
        public float speed = 1;

        [ToggleGroup("animate"), Range(.1f, 2)]
        public float bounceDecay = 1;

        [ToggleGroup("animate")]
        public float stiffness = 1;

        float velocity = 0;
        float value_l;
        Color color;
        float prevValue;

        // Use this for initialization
        void Start()
        {
            if (!progressBar) return;
            if (progressBar.type != Image.Type.Filled)
                progressBar.type = Image.Type.Filled;

            prevValue = value;
            value_l = value;
        }

        // Update is called once per frame
        void Update()
        {

            if (!progressBar) return;

            if (copyCat) value = copyCat.value;

            // Straight showing the progress with no animation
            if (!animate) value_l = value;

            // Fancy animation
            else
            {
                float dist = value - value_l;

                float closeness = bounceDecay - dist;

                velocity += dist * speed * Time.unscaledDeltaTime;
                velocity = Mathf.Lerp(velocity, 0, Time.unscaledDeltaTime * closeness * stiffness);

                value_l += velocity * Time.unscaledDeltaTime;
            }

            value = Mathf.Clamp01(value);
            value_l = Mathf.Clamp01(value_l);

            // Animate color tint
            Color c = Color.white;
            if (adjustColor)
            {
                Image image = progressBar;
                if (separateImageForColor) image = separateImageForColor;
                
                c = normalColor;
                if (prevValue < value) c = addColor;
                else if (prevValue > value) c = removeColor;

                float deltaValue = Mathf.Abs(prevValue - value) * colorShiftSpeed;
                prevValue = value;

                // Color lerping speed is different depending on if it's returning to default color or changing to a delta color.
                float lerpingSpeed = Time.unscaledDeltaTime * colorReturnSpeed;
                if (Math.Abs(deltaValue) > .001f) lerpingSpeed = deltaValue;
                color = Color.Lerp(color, c, lerpingSpeed);
                image.color = color;
            }

            progressBar.fillAmount = value_l;
        }

        public void SnapTo(float newValue)
        {
            value = value_l = newValue;
            velocity = 0;
        }
    }
}