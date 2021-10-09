using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SpiderWeb;
using Sirenix.OdinInspector;
using Rewired;
using DUI;
using UnityEngine.Events;

namespace Diluvion.Ships
{

    /// <summary>
    /// A station is anywhere the crew goes to interact with the ship
    /// </summary>
    public class Station : InteriorElement, IClickable
    {

        public string stationName = "Unnamed station";
        public string stationLocKey;
        public bool showHeader = true;

        public ShipModule linkedModule;
        
        [Space]
        public bool requireOfficer = true;
        
        [ShowIf("requireOfficer")]
        public Officer officer;
        
        public CharacterPlacer officerSpot;
        public CrewStats stationCrewStats;
        public List<CharacterPlacer> crewSpots = new List<CharacterPlacer>();
        public int maxCrew = 5;
        public bool operational;

        public delegate void CrewChanged();
        public CrewChanged crewChanged;

        public bool hovered;

        /// <summary>
        /// Tutorial to display when first activated
        /// </summary>
        [Space]
        public TutorialObject tutOnActivate;
        
        /// <summary>
        /// Tutorial to end when first activated
        /// </summary>
        public TutorialObject endTutOnActivate;

        [Space] public UnityEvent onStationActivate;
        public UnityEvent onStationDeactivate;

        protected bool singleInit = false;
        protected int spotIndex;
        protected Animator animator;
        protected InteriorManager interior;

        CrewManager _crewManager;
        StationBox _fullPanel;
        DUIStationInfo _stationHeader;
        Player _player;
        bool _adding = false;
        bool _removing = false;


        protected virtual void Awake()
        {
            animator = GetComponent<Animator>();
            DisableStation();
        }

        protected override void Start()
        {
            interior = GetComponentInParent<InteriorManager>();
            base.Start();
            FindCrewSpots();
            
            //If this station is on a player ship, show the label GUI
            if (CrewManager() == PlayerManager.PlayerCrew() && showHeader)
            {
                _stationHeader = UIManager.Create(UIManager.Get().stationHeader as DUIStationInfo);
                _stationHeader.Init(this);
                _stationHeader.UpdateCrew(GetCrewCount(), MaxCrew());
            }
        }

        void OnDestroy()
        {
            if (_stationHeader) _stationHeader.End(0);
        }

        public void ShowUI()
        {
            if (!InteriorCamRaycaster.Dragging())
                _fullPanel = StationBox.Show(this, true);
        }

        public void PreviewUI()
        {
            if (!InteriorCamRaycaster.Dragging())
                _fullPanel = StationBox.Show(this, false);
        }

        /// <summary>
        /// Can this station use the given officer as its main operator?
        /// </summary>
        public bool CanUseOfficer(Officer o)
        {
            if (!requireOfficer) return false;
            if (officer) return false;
            if (o.characterInfo == null) return false;
            return (o.characterInfo.skill == linkedModule);
        }


        #region player interaction
        public virtual void OnClick() { }

        public virtual void OnRelease()
        {
            // When clicked, bring up the full station stats
            if (!operational) return;

            ShowUI();
            OrbitCam.FocusOn(gameObject);
        }

        public virtual void OnPointerEnter()
        {
            //Debug.Log("On pointer enter.");
            if (!operational) return;
            hovered = true;

            animator.SetBool("hovered", hovered);
            PreviewUI();
        }


        public virtual void OnPointerExit()
        {
            hovered = false;
            animator.SetBool("hovered", hovered);
            if (_fullPanel)
                if (!_fullPanel.expanded) _fullPanel.End();
        }

        public virtual void OnFocus()
        {

        }

        public virtual void OnDefocus()
        {
            // If no longer looking at this station, remove the full stats panel.
            if (_fullPanel) _fullPanel.End();
        }

        #endregion

        [Button]
        void AddToKeyLib()
        {
            Localization.AddToKeyLib(stationLocKey, stationName);
        }

        public string LocalizedName()
        {
            return Localization.GetFromLocLibrary(stationLocKey, stationName);
        }

        public CrewManager CrewManager()
        {
            if (_crewManager) return _crewManager;
            _crewManager = GetComponentInParent<CrewManager>();
            return _crewManager;
        }



        /// <summary>
        /// Finds all the crew spots, excludes the officer spot
        /// </summary>
        protected void FindCrewSpots()
        {
            crewSpots.Clear();
            crewSpots.AddRange(GetComponentsInChildren<CharacterPlacer>());
            if (crewSpots.Contains(officerSpot)) crewSpots.Remove(officerSpot);
        }

        #region enable and disable
        /// <summary>
        /// Sets the given station as not operational. Kicks out any crew that are in it.
        /// </summary>
        public virtual void DisableStation()
        {
            operational = false;
            officer = null;
            animator.SetBool("operational", operational);

            // Remove all sailors - wait a bit so it iterates through full list before any of them are removed.
            List<Character> crewToRemove = new List<Character>();
            crewToRemove.AddRange(GetComponentsInChildren<Character>());
            foreach (Character crew in crewToRemove) crew.JoinHeartStation();

            onStationDeactivate.Invoke();
            UpdateStation();
            ShowStationDeactivatedGui();
        }


        /// <summary>
        /// Enables the station and sets the given officer as the station officer.
        /// </summary>
        public void EnableStation(Officer o)
        {
            officer = o;
            EnableStation();
            ShowStationActivationGui();
        }

        /// <summary>
        /// Enables the station for use by all crew.
        /// </summary>
        public virtual void EnableStation()
        {
            operational = true;
            animator.SetBool("operational", operational);

            //If this station is on a player ship, show the label GUI
            if (CrewManager() == PlayerManager.PlayerCrew())
            {
                if (tutOnActivate) TutorialPanel.ShowTutorial(tutOnActivate, transform);
                if (endTutOnActivate) TutorialPanel.CompleteTutorial(endTutOnActivate);

                if (_stationHeader) 
                    _stationHeader.UpdateCrew(GetCrewCount(), MaxCrew());
            }

            UpdateStation();
            
            onStationActivate.Invoke();
        }
        #endregion
        
        void ShowStationDeactivatedGui()
        {
            if (!CanShowStationChangeGui()) return;
            
            //  TODO loc
            string text = LocalizedName() + " deactivated";
            Notifier.DisplayNotification(text, Color.red);
            
            SpiderSound.MakeSound("Play_Station_Deactivated", gameObject);
        }

        void ShowStationActivationGui()
        {
            if (!CanShowStationChangeGui()) return;
                    
            //  TODO loc
            string text = LocalizedName() + " activated";
            Notifier.DisplayNotification(text, Color.cyan);
            
            SpiderSound.MakeSound("Play_Station_Activated", gameObject);
        }

        bool CanShowStationChangeGui()
        {
            // display the notification for disabling a station. Check if it's in interior mode, because we only
            // want to display this if it's from the player's actions, not from stations de-activating on start.
            if (OrbitCam.CamMode() != CameraMode.Interior) return false;
            
            // We also only want to show this GUI if the player's ship is the camera target.
            return OrbitCam.CurrentTarget() == PlayerManager.PlayerShip().transform;
        }


        /// <summary>
        /// Sets all the sprites of this station to the given color.
        /// </summary>
        void SetColor(Color newColor)
        {
            foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
                if (sr != null) sr.color = newColor;
        }

        int MaxCrew()
        {
            if (officer) return officer.level;
            return maxCrew;
        }


        /// <summary>
        /// Updates the module with all the necessary crew info
        /// </summary>
        public virtual void UpdateStation()
        {
            //Debug.Log("Station crew stats updated.");
            UpdateStationFromCrew();
            if (crewChanged != null) crewChanged();
        }

        /// <summary>
        /// Checks if the station can handle any more crew. If the station's officer is already at capacity, returns false.
        /// sailors.
        /// </summary>
        public virtual bool HasRoomForCrew(GameObject joiningCrew)
        {
            //if (!HasPhysicalRoomForCrew(joiningCrew)) return false;

            if (!requireOfficer) return true;
            if (!officer) return false;
            
            // First get a list of all the sailors currently manning this station
            List<Sailor> sailorsInStation = new List<Sailor>();
            sailorsInStation.AddRange(GetComponentsInChildren<Sailor>());

            if (joiningCrew)
            {
                Sailor joiningSailor = joiningCrew.GetComponent<Sailor>();
                if (joiningSailor) sailorsInStation.Remove(joiningSailor);
                
                // If the joining object isn't a sailor, then it can join.
                else return true;
            }
            
            // returns true if the sailor qty is less than the officer's level
            return sailorsInStation.Count < officer.level;
        }

        public int GetCrewCount() { return GetComponentsInChildren<Sailor>().Length; }


        /// <summary>
        /// Returns the next available spot. Doesn't include officer spot.
        /// </summary>
        public CharacterPlacer NextAvailableSpot()
        {

            FindCrewSpots();
            if (crewSpots.Count < 1) return null;

            if (spotIndex >= crewSpots.Count) spotIndex = 0;

            // Cycle through the spots.  While the spot isnt available, check the next spot.
            int i = 0;
            while (!AvalableSpot(crewSpots[spotIndex]))
            {

                spotIndex++;
                if (spotIndex >= crewSpots.Count) spotIndex = 0;

                i++;
                if (i > 99) break;
            }

            return crewSpots[spotIndex];
        }

        bool AvalableSpot(CharacterPlacer spot)
        {
            if (spot == null) return false;
            if (spot.Used()) return false;
            return true;
        }

        protected void UpdateStationFromCrew()
        {
            // Resets the total crew stats of the station
            stationCrewStats.SetDefaults(0);

            List<Sailor> inputCrew = new List<Sailor>();
            inputCrew.AddRange(GetComponentsInChildren<Sailor>());

            stationCrewStats = new CrewStats(inputCrew);

            if (_stationHeader) _stationHeader.UpdateCrew(GetCrewCount(), MaxCrew());

            // update the linked ship module
            if (!Bridge() || !linkedModule) return;

            if (operational) linkedModule.Enable(Bridge());
            else linkedModule.Disable(Bridge());

            linkedModule.SetStats(Bridge(), stationCrewStats.stats);
        }

        protected InteriorManager Interior()
        {
            if (interior) return interior;
            interior = GetComponentInParent<InteriorManager>();
            return interior;
        }

        protected Bridge Bridge()
        {
            if (!Interior()) return null;
            return Interior().Bridge();
        }
    }
}