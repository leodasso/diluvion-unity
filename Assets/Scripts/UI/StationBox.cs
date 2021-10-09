using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Diluvion;
using Diluvion.Ships;
using Sirenix.OdinInspector;
using SpiderWeb;

namespace DUI
{
    /// <summary>
    /// Displays the crew currently in a station, and the stats for that station.
    /// </summary>
    public class StationBox : DUIView
    {
        [TabGroup( "station", "vars")]
        public bool expanded;

        [TabGroup( "station", "vars")]
        public float bufferTime = 3;

        [TabGroup( "station", "vars")]
        public float expandedAnchorY;

        [TabGroup( "station", "vars")]
        public float shortAnchorY;

        [TabGroup( "station", "setup")]
        public TextMeshProUGUI stationName;

        [TabGroup( "station", "setup")]
        public RectTransform crewLayout;

        [TabGroup( "station", "setup")]
        public CanvasGroup crewScrollGroup;

        [TabGroup( "station", "setup")]
        public RectTransform crewPanel;

        [TabGroup( "station", "setup")]
        public CrewStatCollection totalCrewStatsLayout;

        [TabGroup( "station", "setup")]
        public RectTransform stationStatsLayout;

        [TabGroup( "station", "setup")]
        public RectTransform officerSlot;

        [TabGroup( "station", "setup")]
        public RectTransform crewSelectParent;

        [TabGroup( "station", "setup")]
        public Button addCrewButton;

        [TabGroup( "station", "setup")]
        public StationStatUI stationStatPrefab;

        [TabGroup( "station", "setup")]
        public CharacterBox crewDisplayPrefab;

        [TabGroup( "station", "setup")]
        public CharacterBox officerDisplayPrefab;

        [TabGroup( "station", "run time"), ReadOnly]
        public Station myStation;

        [ShowInInspector, TabGroup( "station", "run time"), ReadOnly]
        Vector2 targetPosition = Vector2.zero;

        [ShowInInspector, TabGroup( "station", "run time"), ReadOnly]
        RectTransform rect;

        [ShowInInspector, TabGroup( "station", "run time"), ReadOnly]
        int maxCount;

        [ShowInInspector, TabGroup( "station", "run time"), ReadOnly]
        int currentCount;

        [ShowInInspector, TabGroup( "station", "run time"), ReadOnly]
        CrewSelect crewSelectInstance;

        float anchorY;
        float crewAlpha = 1;

        public static StationBox Show(Station station, bool expanded = false)
        {
            StationBox instance = null;

            // Check for an instance of station box
            if (UIManager.GetPanel<StationBox>())
            {
                instance = UIManager.GetPanel<StationBox>();
                if (instance.expanded && !expanded) return instance;
            }
            else instance = UIManager.Create(UIManager.Get().stationDetails as StationBox);

            if (instance == null) return null;

            instance.Init(station);
            instance.expanded = expanded;

            return instance;
        }

        protected override void Start ()
        {
            base.Start();
            group.interactable = group.blocksRaycasts = false;
        }

        public void Init (Station newStation, bool willAutoHide = true)
        {
            alpha = 1;

            //get my rect transform
            rect = GetComponent<RectTransform>();
            myStation = newStation;

            anchorY = crewPanel.anchorMin.y;
            //myStats = myStation.stationStats;

            myStation.UpdateStation();
            RefreshAll();
            SetDefaultSelectable();
        }

        /// <summary>
        /// Set the default selectable to the add crew button
        /// </summary>
        protected override void SetDefaultSelectable ()
        {
            // If there's still an add crew window up, ignore the request
            if (crewSelectInstance != null)
                return;

            if (addCrewButton)
                SetCurrentSelectable(addCrewButton.gameObject);
        }


        void RefreshAll ()
        {
            // Display the name of this station
            stationName.text = myStation.LocalizedName();

            if (addCrewButton)
            {
                //Sets the add crew button to not interactable if the crew is hungry, and vice versa
                addCrewButton.interactable = true;

                // don't allow adding crew if the station is full
                if (!myStation.HasRoomForCrew(null))
                    addCrewButton.interactable = false;
            }

            // Populate the crewmembers of this station
            if (crewLayout)
            {
                // populate the officer slot
                if (myStation.officer != null)
                    AddCrewBox(myStation.officer, "", officerSlot, officerDisplayPrefab);

                // Destroy children
                List<GameObject> children = new List<GameObject>();
                foreach (Transform t in crewLayout)
                {
                    if (t == addCrewButton.transform)
                        continue;
                    children.Add(t.gameObject);
                }
                children.ForEach(child => Destroy(child));

                // Add crew stats boxes
                foreach (Sailor crew in myStation.GetComponentsInChildren<Sailor>())
                {
                    string removeString = Localization.GetFromLocLibrary("GUI/remove", "_remove");
                    AddCrewBox(crew, removeString, crewLayout, crewDisplayPrefab);
                }
            }

            // Show the total crew stats of this station
            totalCrewStatsLayout.StatCollectionInit(myStation.stationCrewStats.stats, 
                shipMods: myStation.linkedModule.shipMods, forStationTotals:true);

            // Remove any children elements from station stats layout
            GO.DestroyChildren(stationStatsLayout);

            // Show the results of the crew on the station's ship modifiers
            foreach (ShipModifier mod in myStation.linkedModule.shipMods)
            {   // For each ship modifier in the station's linked module...

                // Instantiate a UI element for the mod
                StationStatUI newStationStat = Instantiate(stationStatPrefab, stationStatsLayout, false) as StationStatUI;

                // Initialize the UI element with the mod and the station's total crew stats
                newStationStat.Init(mod, myStation.stationCrewStats.stats);
            }

            SetDefaultSelectable();
        }


        /// <summary>
        /// Adds a new crew display box for the given crew
        /// </summary>
        void AddCrewBox (Character crew, string label, Transform parent, CharacterBox prefab)
        {
            // Instantiate and init the new UI element
            CharacterBox newBox = Instantiate(prefab) as CharacterBox;
            newBox.transform.SetParent(parent, false);
            newBox.Init(crew, label);

            // Place the button at the bottom
            addCrewButton.transform.SetAsLastSibling();
        }


        /// <summary>
        /// Opens the 'crew select' window to add characters to this station.
        /// </summary>
        public void ShowCrewSelection ()
        {
            if (crewSelectInstance)
                return;

            if (!myStation) return;

            if (myStation.CrewManager().AvailableSailors().Count < 1)
                return;

            string locTitle = Localization.GetFromLocLibrary("selectCrew", "_select");
            string locString = Localization.GetFromLocLibrary("GUI/addToStation", "_add");
            List<Character> charList = new List<Character>();
            foreach (Sailor s in myStation.CrewManager().AvailableSailors())
                charList.Add(s);

            crewSelectInstance = CrewSelect.Create(charList, locString, locTitle);
            crewSelectInstance.transform.SetParent(crewSelectParent, false);
            crewSelectInstance.crewSelect += AddCrew;
        }


        // Update is called once per frame
        protected override void Update ()
        {
            base.Update();

            if (expanded)
            {
                group.blocksRaycasts = group.interactable = true;
                anchorY = expandedAnchorY;
                crewAlpha = 1;
                crewScrollGroup.blocksRaycasts = crewScrollGroup.interactable = true;
            }
            else
            {
                group.blocksRaycasts = group.interactable = false;
                anchorY = shortAnchorY;
                crewAlpha = 0;
                crewScrollGroup.blocksRaycasts = crewScrollGroup.interactable = false;
            }

            crewPanel.anchorMax = new Vector2(crewPanel.anchorMax.x, Mathf.Lerp(crewPanel.anchorMax.y, anchorY, Time.unscaledDeltaTime * 10));
            crewScrollGroup.alpha = Mathf.Lerp(crewScrollGroup.alpha, crewAlpha, Time.unscaledDeltaTime * 10);

            if (currentCount != myStation.GetComponentsInChildren<Sailor>().Length)
                RefreshAll();
            currentCount = myStation.GetComponentsInChildren<Sailor>().Length;
        }

        /// <summary>
        /// When the crewmember is selected, remove that crew member
        /// </summary>
        public void OnSelectCrew (Character crew)
        {
            crew.LeaveStation();
            RefreshAll();
        }


        public void AddCrew (Character crew)
        {
            crew.JoinStation(myStation);
            RefreshAll();

            // If station is full after add, close the crew select panel
            if (!myStation.HasRoomForCrew(null))
                UIManager.Clear<CrewSelect>();
        }

        protected override void FadeoutComplete ()
        {
            Debug.Log("Ended.");
            Destroy(gameObject);
        }
    }
}