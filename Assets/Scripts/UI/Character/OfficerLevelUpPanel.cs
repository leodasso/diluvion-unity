using System.Collections;
using System.Collections.Generic;
using Diluvion;
using SpiderWeb;
using UnityEngine;
using TMPro;

namespace DUI
{

    /// <summary>
    /// Class for the panel that shows when an officer levels up to explain what that means!
    /// </summary>
    public class OfficerLevelUpPanel : DUIView
    {
        public CharacterBox characterBoxPrefab;
        public Transform characterBoxParent;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI titleText;
        
        /// <summary>
        /// Shows a panel to describe how the given officer leveled up.
        /// </summary>
        public static void ShowOfficerLevelUp(Character character, string title, string description)
        {
            OfficerLevelUpPanel instance = UIManager.Create(UIManager.Get().officerLevelUpPanel as OfficerLevelUpPanel);
            instance.SetupCharacterBox(character);

            instance.titleText.text = title;
            instance.descriptionText.text = description;
            
            // format the description text
            instance.descriptionText.text = instance.OfficerLevelUpText(character);
            
            SpiderSound.MakeSound("Play_MUS_Officer_Upgrade", instance.gameObject);
        }
        
        /// <summary>
        /// Shows a panel to describe that the given character's stat has increased.
        /// </summary>
        public static void ShowSailorBuffed(Character character, CrewStatObject buffedStat, string title, string description)
        {
            OfficerLevelUpPanel instance = UIManager.Create(UIManager.Get().officerLevelUpPanel as OfficerLevelUpPanel);
            instance.SetupCharacterBox(character);
            
            instance.titleText.text = title;
            instance.descriptionText.text = description;
            
            // format the description text
            instance.descriptionText.text = instance.SailorBuffText(character, buffedStat);
        }

        void SetupCharacterBox(Character character)
        {
            // remove any previous characterbox from the parent
            SpiderWeb.GO.DestroyChildren(characterBoxParent);

            // show the character box
            CharacterBox charBox = Instantiate(characterBoxPrefab, characterBoxParent);
            charBox.Init(character, forceShowStats:true);
        }

        string SailorBuffText(Character character, CrewStatObject buffedStat)
        {
            return string.Format(descriptionText.text, character.GetLocalizedName(), buffedStat.LocalizedStatName());
        }

        string OfficerLevelUpText(Character character)
        {
            Officer o = character as Officer;
            if (o == null)
            {
                Debug.LogError("Attempting to show level up text for character, however they're not an officer.");
                return "error";
            }
            
            string text = descriptionText.text;
            return string.Format(text, o.GetLocalizedName(), o.level);
        }
    }
}