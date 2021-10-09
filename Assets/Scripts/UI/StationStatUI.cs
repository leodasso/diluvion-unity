using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using Diluvion;
using Diluvion.Ships;
using Sirenix.OdinInspector;

namespace DUI
{

    /// <summary>
    /// DUI element that shows the bonuses for a particular modifier
    /// </summary>
    public class StationStatUI : MonoBehaviour
    {
        public DUICharacterStat statsPrefab;
        public TextMeshProUGUI statName;
        public TextMeshProUGUI statValue;
        public TextMeshProUGUI statDescription;

        [ReadOnly]
        public ShipModifier mod;
        public List<CrewStatValue> crewStats = new List<CrewStatValue>();

        /// <summary>
        /// Initialize the UI element to show the effect the given crew stats have on the given ship modifier.
        /// </summary>
        public void Init(ShipModifier shipMod, List<CrewStatValue> inputStats)
        {
            mod = shipMod;
            crewStats = inputStats;

            Refresh();
        }


        void Refresh()
        {
            // Set the name of the station stat
            statName.text = mod.displayName.LocalizedText();

            if (statDescription)
                statDescription.text = mod.GuiDescription(PlayerManager.pBridge, crewStats);

            // Show the total stat
            float totalStat = mod.TotalValueOfCrew(crewStats);

            // Format the text
            Color statColor = Color.red;
            string addSymbol = "";

            if (totalStat >= 0)
            {
                addSymbol = "+";
                statColor = Color.yellow;
            }

            statValue.text = addSymbol + mod.FormattedValue(totalStat); //mod.GuiValue(crewStats);
            statValue.color = statName.color = statColor;
        }
    }
}