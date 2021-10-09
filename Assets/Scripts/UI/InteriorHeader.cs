using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpiderWeb;
using TMPro;
using Diluvion.Sonar;
using Diluvion;

namespace DUI
{

    /// <summary>
    /// UI panel that shows in interior view, showing the interior's sonar stats info. (Name, description)
    /// </summary>
    public class InteriorHeader : DUIPanel
    {
        public TextMeshProUGUI shipNameText;
        public TextMeshProUGUI shipClassText;
        public TextMeshProUGUI helpText;
        public string sideViewAction;

        static InteriorHeader interiorHeader;
        InteriorManager interior;

        public static InteriorHeader Get()
        {
            if (interiorHeader) return interiorHeader;

            interiorHeader = GameObject.FindObjectOfType<InteriorHeader>();
            return interiorHeader;
        }

        public void Init(InteriorManager i)
        {
            if (i == null) return;
            interior = i;

            // Display the name of the interior
            if (interior.myName)
            {
                shipNameText.text = interior.myName.LocalizedText();
            }else shipNameText.gameObject.SetActive(false);

            // Display the description 
            if (interior.description)
            {
                shipClassText.text = interior.description.LocalizedText();
            }else shipClassText.gameObject.SetActive(false);

            string inputButtonName = Controls.InputMappingName(sideViewAction);
            helpText.text = helpText.text.Replace("#", inputButtonName);
        }


        protected override void Update()
        {
            base.Update();

            // If the camera is focused on something, override to be transparent
            if (OrbitCam.Get().interiorFocus != null) alpha = 0;
            else alpha = 1;
        }
    }
}