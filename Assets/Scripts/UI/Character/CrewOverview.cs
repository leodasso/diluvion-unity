using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Diluvion;
using Sirenix.OdinInspector;
using TMPro;
using Diluvion.Ships;

namespace DUI
{
    /// <summary>
    /// A simple panel that appears when dragging a member of your crew. Shows the crews stats, 
    /// and if dragging over a station, shows the change that will happen to the station.
    /// </summary>
    public class CrewOverview : DUIPanel
    {
        public CharacterBox charBoxPrefab;
        public StationStatUI statPrefab;

        [Space]
        public Transform charBoxParent;
        [ReadOnly]
        public CharacterBox characterBox;

        public Transform stationStatParent;

        public GameObject textPanel;
        public GameObject statsPanel;
        public TextMeshProUGUI infoText;

        [ReadOnly]
        public Character character;

        Sailor _sailor;

        Station _displayedStation;

        Vector3 _charPos;

        public static CrewOverview CreateInstance (Character character)
        {
            CrewOverview newInstance = UIManager.Create(UIManager.Get().crewOverview as CrewOverview);
            newInstance.Init(character);
            return newInstance;
        }

        void Init (Character ch)
        {
            character = ch;
            _sailor = character as Sailor;

            characterBox = Instantiate(charBoxPrefab, charBoxParent, false);
            characterBox.Init(ch);

            _charPos = ch.TopCenter();

        }

        void LateUpdate ()
        {
            if (!character) return;

            infoText.text = character.swapInfo;
            
            // Turn the text panel on or off depending on if there's a description
            textPanel.SetActive(!string.IsNullOrEmpty(character.swapInfo));

            /*
            if (!string.IsNullOrEmpty(character.swapInfo))
            {
                infoParent.alpha = 1;
                infoText.text = character.swapInfo;
            }
            */

            _charPos = Vector3.Lerp(_charPos, character.TopCenter(), Time.unscaledDeltaTime * 20);
            transform.position = FollowTransform(_charPos, 20, InteriorView.InteriorCam());

            ShowStats();
        }

        void ShowStats()
        {
            if (!_sailor)
            {
                Clear();
                return;
            }

            if (!_sailor.hoveredStation)
            {
                Clear();
                return;
            }

            if (_sailor.hoveredStation == _displayedStation) return;

            // Check if the sailor is able to join that station
            if (_sailor.CanJoinHoveredStation())
                ShowStatsForStation(_sailor.hoveredStation);           
        }

        void Clear(bool activeState = false)
        {
            _displayedStation = null;
            SpiderWeb.GO.DestroyChildren(stationStatParent);
            statsPanel.SetActive(activeState);
        }

        /// <summary>
        /// Displays a UI element for each modifier of the given station, to show how this sailor will affect those stats.
        /// </summary>
        void ShowStatsForStation(Station s)
        {
            Clear(true);
            _displayedStation = s;

            if (!s) return;
            if (!s.linkedModule) return;

            // Display how adding this sailor to the station will affect the stat modifiers
            // Show the results of the crew on the station's ship modifiers
            foreach (ShipModifier mod in s.linkedModule.shipMods)
            {   // For each ship modifier in the station's linked module...

                // Instantiate a UI element for the mod
                StationStatUI newStationStat = Instantiate(statPrefab, stationStatParent, false);

                // Initialize the UI element with the mod and the station's total crew stats
                newStationStat.Init(mod, _sailor.GetSailorStats().stats);
            }
        }
    }
}