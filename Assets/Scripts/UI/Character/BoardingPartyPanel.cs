using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Diluvion;
using Sirenix.OdinInspector;
using UnityEngine.UI;

namespace DUI
{
    /// <summary>
    /// Shows available crew and current boarding party
    /// </summary>
    public class BoardingPartyPanel : DUIPanel
    {
        [BoxGroup("", false)]
        public RectTransform    availableCrewParent;
        [BoxGroup("", false)]
        public RectTransform    boardingPartyParent;

        [BoxGroup("", false)] public Button okBtn;

        [BoxGroup("", false), ReadOnly]
        public CrewSelect       availableCrew;
        [BoxGroup("", false), ReadOnly]
        public CrewSelect       boardingParty;

        [ReadOnly]
        public List<Character>  availableCharacters;

        protected override void Start ()
        {
            base.Start();

            GameManager.Freeze(this);

            Refresh();

            // instantiate crew list for 'available crew'
            availableCrew = CrewSelect.Create(availableCharacters, "Add", "Available Crew", willEndIfEmpty: false);
            availableCrew.transform.SetParent(availableCrewParent, false);
            availableCrew.crewSelect += AddToParty;

            // instantiate crew list for 'boarding party'
            boardingParty = CrewSelect.Create(PlayerManager.BoardingParty(), "Remove", "Boarding Party", willEndIfEmpty: false);
            boardingParty.transform.SetParent(boardingPartyParent, false);
            boardingParty.crewSelect += RemoveFromParty;
        }

        /// <summary>
        /// Add the given crew to the boarding party
        /// </summary>
        void AddToParty (Character crew)
        {
            Debug.Log("Adding sailor " + crew.GetLocalizedName());
            Sailor s = crew as Sailor;
            if (!s) return;
            
            if (PlayerManager.BoardingParty().Contains(s)) return;
            if (s.injured > 0) return;
            PlayerManager.BoardingParty().Add(s);
            Refresh();
        }

        /// <summary>
        /// Remove the given crew form the boarding party
        /// </summary>
        void RemoveFromParty (Character crew)
        {
            Sailor s = crew as Sailor;
            if (s) PlayerManager.BoardingParty().Remove(s);
            Refresh();
        }

        void Refresh ()
        {
            // Get list of available crew members
            availableCharacters.Clear();
            foreach (Sailor s in PlayerManager.PlayerCrew().AllSailorsOnBoard())
            {
                if (PlayerManager.BoardingParty().Contains(s)) continue;
                availableCharacters.Add(s);
            }

            if (availableCrew) availableCrew.Refresh();
            if (boardingParty) boardingParty.Refresh();

            // only allow the player to click 'ok' if they have a boarding party set
            okBtn.interactable = PlayerManager.BoardingParty().Count > 0;
        }

        public override void End ()
        {
            base.End();
            GameManager.UnFreeze(this);
        }
    }
}