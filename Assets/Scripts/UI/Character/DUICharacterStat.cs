using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using Diluvion;
using SpiderWeb;

namespace DUI
{

    public class DUICharacterStat : MonoBehaviour
    {
        [ReadOnly]
        public Character myChar;

        public HazardContainer hazardC;
        Hazard _hazard;

        public TextMeshProUGUI hitChance;
        public TextMeshProUGUI statName;
        public TextMeshProUGUI statValue;
        public TextMeshProUGUI plusText;
        public TextMeshProUGUI damageText;
        public TextMeshProUGUI actionText;
        public CanvasGroup statVisuals;

        [ReadOnly]
        public CrewStatValue crewStatValue;

        bool _displayStationUsage;

        // Use this for initialization
        public void Init (CrewStatValue newCrewStat, Character ch = null, bool usesThisStat = true, bool displayStationUsage = false)
        {
            hazardC = BattlePanel.GetHazardContainer();
            if (hazardC)
                _hazard = hazardC.hazard;

            myChar = ch;

            if (statVisuals != null)
                if (!usesThisStat)
                {
                    statVisuals.alpha = 0;
                    return;
                }
                else
                    statVisuals.alpha = 1;

            crewStatValue = new CrewStatValue(newCrewStat);
            statName.text = crewStatValue.statBase.LocalizedStatName();
            float displayValue = crewStatValue.value;
            statValue.text = displayValue.ToString();

            if (plusText)
            {
                if (crewStatValue.value < 0) plusText.color = Color.clear;
                else plusText.color = ColorOfValue(crewStatValue.value);
            }

            if (actionText) actionText.text = crewStatValue.statBase.LocalizedUsageString();

            statName.color = statValue.color = ColorOfValue(crewStatValue.value);

            if (displayStationUsage)
            {
                Color usageColor = new Color(.5f, .5f, .5f, .6f);
                if (usesThisStat) usageColor = ColorOfValue(crewStatValue.value);
                if (plusText) plusText.color = usageColor;
                if (statValue) statValue.color = usageColor;
                if (statName) statName.color = usageColor;
            }

            if (_hazard ) HazardSetup();
        }

        Color ColorOfValue(float value)
        {
            if (value < 0) return Color.red;
            return Color.yellow;
        }

        void HazardSetup()
        {
            if (_hazard.Reaction(crewStatValue.statBase) == null)
            {
                EnableButton(false);
                return;
            }
            
            EnableButton();

            if (hitChance)
            {
                float chance = _hazard.ChanceOfSuccess(crewStatValue, hazardC.instanceLevel) * 100;
                chance = Mathf.Round(chance);
                hitChance.text = chance + "%";
            }

            if (damageText)
            {
                string dmg = _hazard.DamageFromStat(crewStatValue).ToString("0");
                string fullDmg = damageText.text.Replace("#", dmg);
                damageText.text = fullDmg;
            }
        }

        void EnableButton(bool enable = true)
        {
            Button b = GetComponent<Button>();
            if (b) b.interactable = enable;
            if (hitChance) hitChance.gameObject.SetActive(enable);
            if (damageText) damageText.gameObject.SetActive(enable);
        }

        public void StatAction ()
        {
            if (crewStatValue == null) return;
            if (!myChar) return;

            BattlePanel.NewAttack(crewStatValue, myChar);
        }

        public void PlaySelectSound()
        {
            SpiderSound.MakeSound("Crew_Attack_Mouse_Over", gameObject);
        }
    }
}