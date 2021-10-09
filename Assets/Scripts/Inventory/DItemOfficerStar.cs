using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Diluvion;
using Diluvion.SaveLoad;
using DUI;
using SpiderWeb;

namespace Loot
{

    [CreateAssetMenu(fileName = "officer star", menuName = "Diluvion/items/officer star")]
    public class DItemOfficerStar : DItem
    {

        public LocTerm chooseOfficer;
        public LocTerm leveledUpTitle;
        public LocTerm leveledUpDescription;
        
        public override void Use()
        {
            if (!PlayerManager.PlayerCrew()) return;

            base.Use();

            // find all the officers on board
            List<Character> characters = new List<Character>();
            foreach (Officer officer in PlayerManager.PlayerCrew().AllOfficersOnBoard())
            {
                characters.Add(officer);
            }

            // open up character selection to choose an officer 
            CrewSelect.CreateStandalone(characters, " ", chooseOfficer.LocalizedText(), true, true).crewSelect += IncreaseLevel;
        }


        void IncreaseLevel(Character character)
        {
            // remove the crew select panel
            UIManager.Clear<CrewSelect>();
            
            // remove the inventory panel
            UIManager.Clear<TradePanel>();
            
            if (!character)
            {
                Debug.LogError("Null character was given when attempting to increase level.");
                return;
            }

            Officer officer = character as Officer;

            if (!officer)
            {
                Debug.LogError("Can't level up anything but officers!");
                return;
            }
            
            officer.level++;

            DSave.UpdatePlayerCrew();
            
            // big fancy GUI panel
            OfficerLevelUpPanel.ShowOfficerLevelUp(officer, leveledUpTitle.LocalizedText(), leveledUpDescription.LocalizedText());
            
            // Removes the item from this inventory
            RemoveFromPlayerInventory();
        }

        public override bool IsStealable()
        {
            return false;
        }
    }

}
