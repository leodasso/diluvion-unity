using UnityEngine;
using System.Collections.Generic;
using SpiderWeb;
using Sirenix.OdinInspector;

namespace Diluvion.Ships
{

    /// <summary>
    /// A single ship ability. For example, 'dodging', 'fire rate', and 'sonar range' would all be modifiers.
    /// Calling modify() will apply this particular modification to the given bridge. Modifiers are dynamic only, meaning
    /// they can be added and removed & the original amount will be remembered.
    /// </summary>
    public abstract class ShipModifier : ScriptableObject
    {

        public LocTerm displayName;
        public List<CrewStatValue> affectingCrewStats = new List<CrewStatValue>();
        public List<Forging> requiredBonuses = new List<Forging>();
        public float baseValue = 0.5f;
        [Tooltip("How the value is formatted when displayed in GUI. For example '0.0', '00%', etc")]
        public string valueFormat = "0.0";

        [Tooltip("A loc term something along the lines of 'The {0} of crew in this station '")]
        public LocTerm descriptionPrefix;

        [Tooltip("A loc term something like ' increases your sub's chance of finding a mate by {0}")]
        public LocTerm descriptionSuffix;

        [ToggleLeft]
        public bool useCurve;

        [ShowIf("useCurve"), Tooltip("Optional curve to process the value from crew when modifying a stat. X axis is input (crew stats) " +
                                     "and Y axis is output (the final modified value)")]
        public AnimationCurve curve;

        protected const string locKeyPrefix = "GUI/mod_";
        public abstract void Modify(Bridge bridge, float value);

        public void Modify(Bridge bridge, List<CrewStatValue> crewStats)
        {
            Debug.Log("Module " + name + " recieved stats from station.");
            Modify(bridge, TotalValueOfCrew(crewStats) + baseValue);
        }
        protected float ProcessedValue(float inputValue)
        {
            float output = inputValue;
            if (useCurve) output = curve.Evaluate(inputValue);
            return output;
        }

        protected string GuiDisplayCrewStats()
        {
            string s = "";
            for (int i = 0; i < affectingCrewStats.Count; i++)
            {
                s += affectingCrewStats[i].statBase.localizedName;
                if (affectingCrewStats.Count > i + 1) s += ", ";
                else s += " ";
            }

            return s;
        }

        /// <summary>
        /// The string that reads out in the GUI, like 'adds 35% engine power
        /// </summary>
        public string FormattedValue(float value)
        {
            return value.ToString(valueFormat);
        }

        public string GuiDescription(Bridge b, List<CrewStatValue> crewStats)
        {
            return DescriptionCrewPrefix() + String.Format(descriptionSuffix.LocalizedText(), 
                       new List<string>{ FormattedValue(TotalValueOfCrew(crewStats))});
        }

        protected string DescriptionCrewPrefix()
        {
            return String.Format(descriptionPrefix.LocalizedText(), new List<string>{GuiDisplayCrewStats()});
        }

        /// <summary>
        /// Check if this modifier uses the given stat.
        /// </summary>
        public bool UsesStat(CrewStatValue stat)
        {
            return (UsesStat(stat.statBase));
        }

        /// <summary>
        /// Check if this modifier uses the given stat.
        /// </summary>
        public bool UsesStat(CrewStatObject stat)
        {
            foreach (CrewStatValue csv in affectingCrewStats)
                if (csv.statBase == stat) return true;

            return false;
        }

        /// <summary>
        /// Returns the total value of the given crew stats as they apply to this modifier.
        /// </summary>
        public float TotalValueOfCrew(List<CrewStatValue> crewStats)
        {
            float total = 0;
            foreach (CrewStatValue stat in crewStats) total += ValueOfStat(stat);
            
            return ProcessedValue(total);
        }

        /// <summary>
        /// Returns the total value of a single given stat. For example, if the given stat is intelligence but this 
        /// modifier only depends on strength, will return 0.
        /// </summary>
        protected float ValueOfStat(CrewStatValue stat)
        {
            foreach (CrewStatValue myStat in affectingCrewStats)
            {
                if (myStat.statBase == stat.statBase)
                    return myStat.value * stat.value;
            }
            return 0;
        }

        #region testing

        [ButtonGroup]
        void InspectorTest()
        {
            Debug.Log(Test());
        }

        protected virtual string Test()
        {
            return "Assuming input crew stats of 5 across the board, " +
                "this modifier will have a value of " + TestingValue() + ". ";
        }

        protected float TestingValue()
        {
            float value = 0;
            foreach (CrewStatValue stat in affectingCrewStats) value += stat.value * 5;
            return value + baseValue;
        }

        #endregion
    }

    [System.Serializable]
    public class ModifierValue
    {
        public ShipModifier mod;
        public float value = 1;
    }
}