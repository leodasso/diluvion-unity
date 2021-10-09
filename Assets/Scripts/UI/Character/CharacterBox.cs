using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Diluvion;
using Diluvion.Ships;
using Sirenix.OdinInspector;


namespace DUI
{

    /// <summary>
    /// Displays UI for a character.
    /// </summary>
    public class CharacterBox : DUIPanel
    {
        /// <summary>
        /// If a portrait isn't available for this character, should the placeholder be shown, or just hide the portrait?
        /// </summary>
        [ToggleLeft]
        [TabGroup("charBox", "General")]
        public bool showPlaceholderPortrait;

        [ToggleLeft]
        [TabGroup("charBox", "General")]
        public bool showStats = true;

        [ToggleLeft, ShowIf("showStats"), TabGroup("charBox", "General")] 
        [Tooltip("If false, just shows the crews permenant stats. If true, adds the temporary stats as well")]
        public bool showTempStats;

        [ReadOnly]
        [TabGroup("charBox", "General")]
        public Character myCrew;

        [TabGroup("charBox", "Setup")]
        public CrewStatCollection crewStatCollection;

        [TabGroup("charBox", "General")]
        public PopupObject fireCrew;
        [TabGroup("charBox", "General")]
        public PopupObject crewFiredPopup;

        [TabGroup("charBox", "Setup")]
        public Image portraitIcon;

        [TabGroup("charBox", "Setup")]
        public TextMeshProUGUI buttonText;

        [TabGroup("charBox", "Setup")]
        public TextMeshProUGUI characterName;

        [TabGroup("charBox", "Setup")]
        public TextMeshProUGUI characterStation;

        [TabGroup("charBox", "Setup")]
        public TextMeshProUGUI descriptorText;

        [TabGroup("charBox", "Setup")]
        public Button fireButton;

        [TabGroup("charBox", "Setup")]
        public CanvasGroup statsLayoutGroup;

        [TabGroup("charBox", "Setup")]
        public CanvasGroup officerGroup;
        
        [TabGroup("charBox", "Setup")]
        public CanvasGroup injuredGroup;

        [TabGroup("charBox", "Setup")] 
        public TextMeshProUGUI counterText;

        [TabGroup("charBox", "Setup")]
        public LayoutGroup officerStarLayout;

        Button _myButton;
        
        float _statLayoutAlpha;

        Animator _animator;

        bool _showOfficerStats;

        Sailor _sailor;

        Sailor GetSailor()
        {
            if (_sailor) return _sailor;
            if (!myCrew) return null;
            return myCrew as Sailor;
        }


        public void Init(Character ch, string buttonLabel = "", bool forceShowStats = false)
        {
            _animator = GetComponent<Animator>();

            myCrew = ch;

            // Check for a button component
            _myButton = GetComponent<Button>();
            if (_myButton != null)
            {
                if (buttonText != null)
                    buttonText.text = buttonLabel;

                // If no label is given to the button, de-activate it.
                if (string.IsNullOrEmpty(buttonLabel)) _myButton.enabled = false;
            }

            // Cull the 'fire' button if theyre not on your crew
            if (fireButton != null) fireButton.gameObject.SetActive(CanFireCrew());

            //set the portrait
            Sprite portrait = null;
            if (myCrew.GetAppearance())
                portrait = myCrew.GetAppearance().portrait;
            if (portrait != null)
            {
                portraitIcon.sprite = portrait;
                portraitIcon.color = Color.white;
            }
            // Hiding portrait image, if there's no portrait for this crew
            else if (!showPlaceholderPortrait)
                portraitIcon.color = Color.clear;

            // Displaying the character's description
            characterName.text = myCrew.GetLocalizedName();
            if (descriptorText) descriptorText.text = "";

            // Displaying the characters skills
            RefreshStatsLayout(forceShowStats);

            // For officers, display the officer group & officer type
            Officer o = ch as Officer;
            if (o)
            {
                // display descriptor text (helm officer, weapons officer, etc)
                if (o.characterInfo && descriptorText)
                    descriptorText.text = o.characterInfo.skill.LocalizedName();
                
                // display officer level
                SetupOfficerStars(o);

                _showOfficerStats = true;
            }
        }

        void RefreshStatsLayout(bool forceShowStats)
        {
            crewStatCollection.Clear();
            showStats = false;

            // For sailors, have the animator show the stats
            if (GetSailor() != null || forceShowStats)
            {
                if (_animator != null)
                    _animator.SetBool("full stats", true);

                showStats = true;
                
                if (GetSailor())
                    crewStatCollection.StatCollectionInit(GetSailor().GetSailorStats(showTempStats).stats, GetSailor());

                CombatItemList itemCollection = GetComponentInChildren<CombatItemList>();
                if (itemCollection)
                {
                    itemCollection.Init(myCrew);
                }
            }
            else if (_animator != null) _animator.SetBool("full stats", false);
        }

        static GameObject _starPrefab;
        static GameObject StarPrefab()
        {
            if (_starPrefab) return _starPrefab;

            _starPrefab = Resources.Load<GameObject>("GUI/officer star");
            return _starPrefab;
        }

        void SetupOfficerStars(Officer officer)
        {
            if (!officerStarLayout)
            {
                //Debug.LogError("No layout has been set for officer stars.", gameObject);
                return;
            }
            
            //Debug.Log(name + " finds officer " + officer.name + "'s level to be " + officer.level, gameObject);

            // Loop through game mode's max level to create the stars
            for (int i = 0; i < GameManager.Mode().maxOfficerLevel; i++)
            {
                // instantiate a star inside the level layout
                GameObject starInstance = Instantiate(StarPrefab(), officerStarLayout.transform);
                
                // check if the officer has reached level i yet
                bool atLevel = officer.level > i;

                // set the star's animator so it displays if that level is unlocked yet.
                Animator a = starInstance.GetComponent<Animator>();
                a.SetBool("unlocked", atLevel);
            }
        }

        
        protected override void Update()
        {
            base.Update();

            

            // Show / hide injured overlay
            bool showInjured = false;
            if (injuredGroup && myCrew)
            {
                injuredGroup.alpha = 0;

                if (GetSailor())
                {
                    showInjured = GetSailor().injured > 0;
                    
                    // Display the timer for how long they're injured
                    float injuredTimer = GetSailor().injured * GameManager.Mode().crewInjuryCooldown;
                    counterText.text = injuredTimer.ToString("00 sec");
                }

                injuredGroup.alpha = showInjured ? 1 : 0;
                injuredGroup.blocksRaycasts = showInjured;
                injuredGroup.interactable = showInjured;
                if (_myButton) _myButton.interactable = !showInjured;
            }

            if (statsLayoutGroup)
            {
                bool totalShowStats = showStats;
                if (showInjured) totalShowStats = false;
                
                if (totalShowStats) {
                    _statLayoutAlpha = 1;
                    statsLayoutGroup.interactable = true;
                }
                else {
                    _statLayoutAlpha = 0;
                    statsLayoutGroup.interactable = false;
                }

                statsLayoutGroup.alpha = Mathf.Lerp(statsLayoutGroup.alpha, _statLayoutAlpha, Time.unscaledDeltaTime * 6);
            }

            if (officerGroup)
            {
                float officerGroupAlpha = 0;
                if (_showOfficerStats) officerGroupAlpha = 1;

                officerGroup.alpha = Mathf.Lerp(officerGroup.alpha, officerGroupAlpha, Time.unscaledDeltaTime * 6);
            }

        }

        /// <summary>
        /// Returns true if my character is a sailor, and is on board the player ship.
        /// </summary>
        bool CanFireCrew()
        {

            CrewManager playerCrew = PlayerManager.PlayerCrew();
            if (!playerCrew) return false;

            Sailor s = myCrew as Sailor;
            if (!s) return false;

            return playerCrew.AllSailorsOnBoard().Contains(s);
        }

        public void HideStats()
        {
            crewStatCollection.gameObject.SetActive(false);
        }


        void ActuallyFireCrew()
        {
            myCrew.Die(false);

            // Show popup that theyre fired
            crewFiredPopup.CreateUI();

            // close the dialogue box
            UIManager.Clear<DialogBox>();
        }


        public void FireCrew()
        {
            if (!CanFireCrew()) return;

            List<string> names = new List<string>();
            names.Add(myCrew.GetLocalizedName());

            fireCrew.CreateUI(new DUIPopup.choiceDelegate(ActuallyFireCrew), null, names);
        }


        public void ButtonAction()
        {
            SendMessageUpwards("OnSelectCrew", myCrew, SendMessageOptions.DontRequireReceiver);
        }


        /// <summary>
        /// Returns a string name of the character's skill type based on the given enum.
        /// </summary>
        public string CharacterType(SkillType skill)
        {

            string key = "misc_sailor";
            if (skill == SkillType.Bolt) key = "misc_wepOfficer";
            if (skill == SkillType.Helm) key = "misc_helmOfficer";
            if (skill == SkillType.Navigating) key = "misc_wepOfficer";
            if (skill == SkillType.Sonar) key = "misc_sonarOfficer";
            return SpiderWeb.Localization.GetFromLocLibrary(key, key);
        }
    }
}