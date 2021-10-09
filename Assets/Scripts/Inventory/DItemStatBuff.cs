using UnityEngine;
using SpiderWeb;
using System.Collections.Generic;
using DUI;
using Diluvion;

namespace Loot
{
    [CreateAssetMenu(fileName = "new stat buff item", menuName = "Diluvion/items/stat buff item")]
    public class DItemStatBuff : DItem
    {
        public LocTerm selectSailor;
        public LocTerm statBuffedTitle;
        public LocTerm statBuffedDescription;
        
        public int buffs = 1;
        public CrewStatObject statToBuff;

        /// <summary>
        /// Using this item brings up the crew select, which will then give the buff
        /// to whichever crewmember was selected.
        /// </summary>
        public override void Use()
        {
            if (!PlayerManager.PlayerCrew()) return;

            base.Use();

            List<Character> characters = new List<Character>();
            foreach (Sailor sailor in PlayerManager.PlayerCrew().AllSailorsOnBoard())
                characters.Add(sailor);

            CrewSelect.CreateStandalone(characters, " ", selectSailor.LocalizedText(), true, true).crewSelect += AddBuff;
        }

        
        /// <summary>
        /// Adds the buff to the given crew.
        /// </summary>
        void AddBuff(Character crew)
        {
            // remove the crew select panel
            UIManager.Clear<CrewSelect>();
            
            // remove the inventory panel
            UIManager.Clear<TradePanel>();
            
            // Get the sailor from the given character
            Sailor s = crew as Sailor;
            if (!s)
            {
                Debug.LogError("You're trying to buff a sailor, but it turns out the sailor is null!");
                return;
            }

            // Create the statValue to add
            CrewStatValue newStatValue = new CrewStatValue();
            newStatValue.value = buffs;
            newStatValue.statBase = statToBuff;
            s.ChangePermanentStats(newStatValue);

            SpiderSound.MakeSound("Play_Crew_Upgrade", crew.gameObject);

            // Removes the item from this inventory
            RemoveFromPlayerInventory();
            
            OfficerLevelUpPanel.ShowSailorBuffed(s, statToBuff, statBuffedTitle.LocalizedText(), statBuffedDescription.LocalizedText());
        }

        public override bool IsStealable()
        {
            return false;
        }
    }
}