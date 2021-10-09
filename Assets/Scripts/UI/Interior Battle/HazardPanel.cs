using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Diluvion;
using Sirenix.OdinInspector;
using TMPro;

namespace DUI
{
    /// <summary>
    /// panel for displaying a hazard
    /// </summary>
    public class HazardPanel : DUIPanel
    {
        [BoxGroup(""), OnValueChanged("Refresh")]
        public HazardContainer hazardInstance;

        [BoxGroup("")]
        public TextMeshProUGUI nameText;
        
        [BoxGroup("")]
        public TextMeshProUGUI levelText;
        
        [BoxGroup("")]
        public TextMeshProUGUI hpText;

        [BoxGroup("")] public List<Image> hazardDisplays;

        [BoxGroup("")]
        public FancyProgressBar hpBar;

        string _hpString;

        protected override void Awake()
        {
            base.Awake();
            _hpString = hpText.text;
        }

        public void Init(HazardContainer h)
        {
            hazardInstance = h;
            Refresh();
        }

        void Refresh()
        {
            if (!hazardInstance) return;

            nameText.text = hazardInstance.hazard.LocName();
            
            
            foreach (Image i in hazardDisplays) 
                i.sprite = hazardInstance.hazard.sprite;

            hpBar.value = hazardInstance.NormalizedHP();
        }

        protected override void Update()
        {
            base.Update();
            if (!hazardInstance) return;
                
            hpBar.value = hazardInstance.NormalizedHP();
            levelText.text = hazardInstance.instanceLevel.ToString();

            int clampedHP = Mathf.Clamp(hazardInstance.currentHP, 0, 999);

            hpText.text = string.Format(_hpString, clampedHP, hazardInstance.MaxHP());
        }
    }
}