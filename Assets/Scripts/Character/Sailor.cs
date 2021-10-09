using UnityEngine;
using System.Collections.Generic;
using SpiderWeb;
using Diluvion.Ships;

namespace Diluvion
{

    /// <summary>
    /// Sailors are characters that are randomly generated. 
    /// They support random name, dialogue, appearance, and stats.
    /// </summary>
    public class Sailor : Character
    {
        public List<string> generatedName = new List<string>();
        public Dialogue dialogue;
        public Appearance appearance;
        public Gender gender = Gender.Male;
        public VoiceType voiceType = VoiceType.Somber;

        [Range(0,1), Tooltip("0 is healthy. When injured, gets set to 1, and then there's a cooldown.")]
        public float injured;

        [Space]
        CrewStats crewStats;
        CrewStats tempStats;
        
        public int costToHire;
        
        const float  PointFactor = 0.3f;
        const int    MinimumWage = 250;

        protected override void Start()
        {
            base.Start();
            CheckForStats();
            if (appearance) animator.runtimeAnimatorController = appearance.animController;
        }
        
        /// <summary>
        /// Instantiates a blank sailor template
        /// </summary>
        public static Sailor InstantiateSailor()
        {
            Sailor sailorTemplate = CharactersGlobal.SailorTemplate();
            Sailor instance = Instantiate(sailorTemplate);
            return instance;
        }
        
        /// <summary>
        /// Checks my crew stats and if there's null or valid stats, creates default stats.
        /// </summary>
        public void CheckForStats()
        {
            // If my stats are null, create a new one and set defaults
            if (crewStats == null) crewStats = new CrewStats();

            // If I have empty stats, create a new one and sets defaults.
            if (crewStats.stats.Count < 1)
                crewStats.SetDefaults();
        }

        public override Appearance GetAppearance()
        {
            if (characterInfo) return characterInfo.appearance;
            return appearance;
        }

        protected override void MakeDialog()
        {
            if (dialogue) PrepDialogue(dialogue);
            else base.MakeDialog();
        }

        protected override void Update()
        {
            base.Update();
            if (injured > 0)
            {
                injured -= Time.deltaTime / GameManager.Mode().crewInjuryCooldown;
            }
        }

        public bool IsInjured()
        {
            return injured > 0;
        }

        /// <summary>
        /// Returns the save data (as a SailorSave) for this sailor.
        /// </summary>
        public override CharSave CreateSaveData()
        {
            
            //Debug.Log("Saving " + NonLocalizedName() + " as a sailor.");
            SailorSave sailorSave = new SailorSave();

            if (characterInfo)
                sailorSave.charObjName = characterInfo.name;

            sailorSave.generatedName = generatedName;
            sailorSave.stats = crewStats.GetStatValues();
            sailorSave.gender = gender;
            sailorSave.voiceType = voiceType;
            sailorSave.dialogueName = dialogue.name;
            sailorSave.savedStation = StationName();
            sailorSave.savedLevel = level;
            //Debug.Log("Saved station as " + sailorSave.savedStation);
            
            //sailorSave.stationName = StationName();
            if (appearance) sailorSave.appearanceName = appearance.name;
            sailorSave.animName = animator.runtimeAnimatorController.name;
            return sailorSave;
        }

        /// <summary>
        /// Reverts all temporary stats
        /// </summary>
        public void RevertStats()
        {
            tempStats = new CrewStats();
        }

        /// <summary>
        /// Changes the temporary stats of the crewmember. This is only in effect until the crew joins back on your ship
        /// </summary>
        public void ChangeTemporaryStats(List<CrewStatValue> statsToAdd)
        {
            foreach (var st in statsToAdd)
                tempStats.AddStat(st);
        }

        public void ChangePermanentStats(CrewStatValue statToAdd)
        {
            crewStats.AddStat(statToAdd);
        }

        public CrewStats GetSailorStats(bool includeTemporary = false)
        {
            CrewStats returnStats = new CrewStats(crewStats);
            if (includeTemporary)
            {
                foreach (CrewStatValue v in tempStats.stats)
                    returnStats.AddStat(v);
            }

            return returnStats;
        }

        public void Injure()
        {
            injured = 1;
            PlayerManager.BoardingParty().Remove(this);
        }

        /// <summary>
        /// Sets the permenant stats of the sailor with the given values.
        /// </summary>
        public void SetStatValues(List<float> stats)
        {
            crewStats.ApplyStatValues(stats);
        }

        #region random gen
        /// <summary>
        /// Randomize this instance.
        /// </summary>
        public void Randomize(int totalPoints)
        {
            //choose a gender
            int randInt = Random.Range(0, 2);
            if (randInt == 0) gender = Gender.Male;
            else gender = Gender.Female;

            voiceType = (VoiceType)Random.Range(0, 4);

            // Get a random dialogue
            dialogue = CharactersGlobal.RandomDialogue();
            PrepDialogue(dialogue);

            // random appearance
            SetAppearance(CharactersGlobal.RandomAppearance(gender));

            ApplyRandomName();

            gameObject.name = "sailor " + NonLocalizedName();
            
            // See how much this sailor will cost to hire.
            costToHire = Mathf.RoundToInt(CostOfSailor(totalPoints)); 

            SetRandomCrewStats(totalPoints);
        }

        /// <summary>
        /// Returns the cost of creating the given number of stat points.
        /// </summary>
        public static int CostOfSailor(int numberOfPoints)
        {
            // Calculate the exponent so it only begins compounding after the minimum number of sailor points
            int exponentPoints = Mathf.Clamp(numberOfPoints - GameManager.Mode().minPointsForSailor, 0, 999);
            float exponent = Mathf.Pow(exponentPoints, GameManager.Mode().sailorPointPower);
            
            return Mathf.RoundToInt(
                GameManager.Mode().sailorBaseCost + GameManager.Mode().costPerSailorPoint * exponent);
        }

        void SetAppearance(Appearance newAppearance)
        {
            appearance = newAppearance;
            Animator().runtimeAnimatorController = appearance.animController;
        }

        /// <summary>
        /// Gives the character random stats, using totalPoints as the sum
        /// </summary>
        public void SetRandomCrewStats(int totalPoints)
        {
            CheckForStats();
            crewStats.Randomize(totalPoints, -1);
        }

        public void ApplyRandomName()
        {
            generatedName = CharactersGlobal.RandomName(gender);
        }


        #endregion

        #region naming, localization, interface text
        /// <summary>
        /// Returns the non-localized standard name of this instance.
        /// </summary>
        public override string NonLocalizedName()
        {
            return NonLocalizedSailorName(generatedName);
        }

        /// <summary>
        /// Method for joining a sailor's namelist into a save-able single string.
        /// </summary>
        public static string NonLocalizedSailorName(List<string> names)
        {
            string n = "";
            foreach (string s in names) n += s;
            return n;
        }

        
        public override string GetLocalizedName()
        {
            if (characterInfo) return base.GetLocalizedName();

            if (generatedName.Count < 1) return "Noname";

            // Select a locKey prefix based on gender
            string prefix = (gender == Gender.Male) ? CharactersGlobal.maleNamePrefix : CharactersGlobal.femaleNamePrefix;
            string firstName = Localization.GetFromLocLibrary(prefix + generatedName[0], generatedName[0]);

            // Get localized last name
            string lastName = Localization.GetFromLocLibrary(CharactersGlobal.lastNamePrefix + generatedName[1], generatedName[1]);

            return firstName + " " + lastName;
        }
        
        /// <summary>
        /// The info displayed when hovering this character over the given station.
        /// </summary>
        protected override string JoinStationInfo (Station st)
        {
            if (!st.operational) return CantJoinInfo(st);
            
            // check if the officer can command more sailors
            if (!st.HasRoomForCrew(gameObject)) return OfficerFullInfo(st);

            string s = "{0} will be assigned to work in {1}.";
            return string.Format(s, GetLocalizedName(), st.LocalizedName());
        }

        public string OfficerFullInfo(Station station)
        {
            string s = "{0}, {1}'s officer, can't command any more Sailors.";
            return string.Format(s, station.officer.GetLocalizedName(), station.LocalizedName());
        }

        /// <summary>
        /// The info displayed when pulling this character out of the given station.
        /// </summary>
        protected override string LeaveStationInfo (Station st)
        {
            string s = "{0} will no longer work in {1}.";

            s = string.Format(s, GetLocalizedName(), st.LocalizedName());
            return s;
        }
        #endregion

        #region audio
        public override void LeaveMe()
        {
            ConvoEndSound(gender);
        }

        public override void ClickSound()
        {
            PlayTalkSound(gender);
        }

        protected override void DeathSound(bool violent)
        {
            PlayDeathSound(violent, gender);
        }
        #endregion

        #region stations	

        /// <summary>
        /// Returns the Station component based on the station string name in my data.
        /// </summary>
        public Station MyStation()
        {
            if (!crewManager) return null;
            return crewManager.FindStationOfType(station);
        }

        /// <summary>
        /// Returns to the station I'm supposed to be at. Used to bring guests back.
        /// </summary>
        public void ReturnToStation()
        {
            JoinStation(MyStation());
        }


        /// <summary>
        /// Attempts to join the given station. If there's room for more sailors, joins and updates the station's stats.
        /// </summary>
        public override void JoinStation(Station theStation)
        {
            if (!CanJoinStation(theStation))
            {
                JoinHeartStation();
                return;
            }
            base.JoinStation(theStation);
        }

        /// <summary>
        /// Override that also checks given station's officer's level to determine if it can handle another sailor.
        /// </summary>
        protected override bool CanJoinStation(Station st)
        {
            if (!st) return false;
            if (!st.HasRoomForCrew(gameObject)) return false;
            return base.CanJoinStation(st);
        }


        #endregion
    }


    [System.Serializable]
    public class SailorSave : CharSave
    {
        public List<string> generatedName = new List<string>();
        public List<float> stats;
        public Gender gender = Gender.Male;
        public VoiceType voiceType = VoiceType.Somber;
        public string animName = "";
        public string appearanceName = "";
        public string dialogueName = "";

        public override Character CreateCharacter()
        {
            Sailor newSailor = Sailor.InstantiateSailor();
            newSailor.generatedName = generatedName;
            newSailor.CheckForStats();
            newSailor.SetStatValues(stats); //newSailor.crewStats.ApplyStatValues(stats);
            newSailor.gender = gender;
            newSailor.voiceType = voiceType;
            newSailor.level = savedLevel;
            GO.MakeComponent<Animator>(newSailor.gameObject).runtimeAnimatorController = CharactersGlobal.GetAnimController(animName);
            newSailor.appearance = CharactersGlobal.GetAppearance(appearanceName);
            newSailor.dialogue = CharactersGlobal.GetDialogue(dialogueName);
            
            //Debug.Log("Loading " + newSailor.NonLocalizedName() + " station: " + savedStation);
            newSailor.station = savedStation;

            newSailor.gameObject.name = newSailor.GetLocalizedName();

            return newSailor;
        }

        public override string ToString()
        {
            CharacterInfo info = CharactersGlobal.GetCharacter(charObjName);
            if (info) return info.niceName;

            string s = "";
            foreach (string n in generatedName) s += " " + n;
            return s;
        }
    }
}