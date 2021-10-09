using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Diluvion.Roll;
using DUI;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;

#endif


namespace Diluvion
{

    [RequireComponent(typeof(InteriorGrid))]
    public class RoomPlacer : MonoBehaviour, IRoller
    {

        [TabGroup("Main"), Tooltip("Has this room placer already populated?")] 
        public bool consumed;

        [TabGroup("Main"), Tooltip("Rotates the rooms to the rotation of the placer")] 
        public bool adjustRotation;
        
        [TabGroup("Main"), ReadOnly]
        public InteriorGrid grid;

        [TabGroup("Main"), PropertyOrder(-100)]
        public Transform startingPoint;

#if UNITY_EDITOR
        [TabGroup("Main"), Button, HideIf("HasStartingPoint"), PropertyOrder(-99)]
        void CreateStartingPoint()
        {
            if (startingPoint != null) return;

            GameObject newPoint = new GameObject();
            newPoint.name = "starting point";
            newPoint.transform.parent = transform;
            newPoint.transform.localPosition = Vector3.zero;

            startingPoint = newPoint.transform;
            Selection.activeGameObject = startingPoint.gameObject;
        }

        bool HasStartingPoint()
        {
            return startingPoint != null;
        }

#endif

        [ToggleLeft]
        [TabGroup("Main"), Tooltip("Populate on start and independently of any explorable placer. This sh" +
                                               "ouldn't be used in prefabs intended to be spawned by explorable place" +
                                               "r.")]
        public bool populateOnStart;

        [TabGroup("Main"), ShowIf("populateOnStart")]
        [Tooltip("If populating on start, this is the amount of gold that will be used for creating rewards.")]
        public PopResources localPopResources;

        [ShowIf("populateOnStart"), TabGroup("Main")]
        public ExplorableCurves localCurves;

        [TabGroup("Population"), AssetsOnly]
        public Table roomsTable;

        [TabGroup("Population"), AssetsOnly]
        public Table hazardsTable;

        [TabGroup("Main"), ToggleLeft]
        public bool presetStartRoom;

        [TabGroup("Main"), ShowIf("presetStartRoom")]
        public Room startRoom;

        [TabGroup("Main"), ToggleLeft]
        public bool presetEndRoom;

        [TabGroup("Main"), ShowIf("presetEndRoom")]
        public Room endRoom;

        [TabGroup("Main"),  ReadOnly]
        public List<GameObject> spawnedRooms = new List<GameObject>();

        [TabGroup("Main"),  ReadOnly]
        public List<HazardContainer> hazards = new List<HazardContainer>();

        /// <summary>
        /// List of rooms that haven't been linked yet
        /// </summary>
        [TabGroup("Main")]
        public List<Room> availableRooms = new List<Room>();

        [TabGroup("Main"), ReadOnly]
        public Room firstRoom;
        
        [TabGroup("Main"), ReadOnly]
        public Room lastRoom;

        [TabGroup("Population"), AssetList]
        public List<Tag> rollingTags = new List<Tag>();

        InteriorManager _interior;
        float _dangerPool;
        
        /// <summary>
        /// The amount of danger immediately avialable to the current room 
        /// </summary>
        float _availableDanger;

        /// <summary>
        /// Amount of danger leftover from previous rooms
        /// </summary>
        float _leftoverDanger;
        float _goldPool;
        /// <summary>
        /// Amount of gold leftover from previous rooms
        /// </summary>
        float _leftoverGold;
        PopResources _cachedPopResources;

        // Use this for initialization
        void Start ()
        {
            // population on start - useful for objects that arent part of the explorable system
            if (populateOnStart) PrepPopulation(localPopResources);//Populate(localPopResources);
        }

        void FindTables()
        {
            if (!roomsTable) roomsTable = TableHolder.FindTableForInterior<RoomEntry>(transform);
            if (!hazardsTable) hazardsTable = TableHolder.FindTableForInterior<Hazard>(transform);
        }

        #region population

        [ButtonGroup("population")]
        void PopulateAll()
        {
            FindTables();
            
            // Clear out previous rooms & lists
            Clear();
            
            Populate(localPopResources);
        }


        /// <summary>
        /// Adds population to the interior's onShow delegate, so that rooms only populate when the interior is shown.
        /// </summary>
        public void PrepPopulation(PopResources popResources)
        {
            // Find the interior
            _interior = SpiderWeb.GO.ComponentInParentOrSelf<InteriorManager>(gameObject);

            if (!_interior)
            {
                Debug.LogError(name + " couldn't find an interior in its hierarchy. This is bad, m'kay");
                return;
            }
            
            Debug.Log("Room placer " +  name + " populating with " + popResources.value + " gold value and " + popResources.hazardDanger + " hazard danger.");

            // Remember the pop resources to use to populate with
            _cachedPopResources = popResources;
            
            // add 'populate' to the OnShow delegate, so that population happens only when the interior is about to show
            _interior.onShow += Populate;

        }

        /// <summary>
        /// Same as Populate(PopResources) but uses the cached popresources
        /// </summary>
        void Populate()
        {

            if (consumed) return;
            consumed = true;
            
            if (_cachedPopResources == null)
            {
                Debug.LogError(name + " attempted to populate with cached resources, but none exist!");
                return;
            }
            
            Populate(_cachedPopResources);
        }

        /// <summary>
        /// Creates a pathway of rooms, then adds hazards & rewards to the rooms.
        /// </summary>
        void Populate (PopResources resources)
        {
            grid = GetComponent<InteriorGrid>();
            _interior = SpiderWeb.GO.ComponentInParentOrSelf<InteriorManager>(gameObject);

            ExplorableCurves curves;
            float dangerMult = 1;
            float valueMult = 1;

            if (populateOnStart) curves = localCurves;

            // Find curves and tables from the hierarchy to use in population
            else
            {
                // get the explorable entry of the object
                Explorable explorable =
                    SpiderWeb.GO.ComponentInParentOrSelf<Explorable>(_interior.GetWorldParent().gameObject);
                if (!explorable)
                {
                    Debug.LogError(
                        name + "can't continue with population because no explorable component could be found.", gameObject);
                    return;
                }

                ExplorableEntry entry = explorable.entry as ExplorableEntry;
                if (!entry)
                {
                    Debug.LogError(
                        name + "can't continue with population because no related explorable entry could be found.", gameObject);
                    return;
                }

                curves = entry.curves;
                if (!curves)
                {
                    Debug.LogError(
                        name + "can't continue with population because no curves for value/danger could be found.", gameObject);
                    return;
                }

                dangerMult = entry.dangerMultiplier;
                valueMult = entry.goldValueMultiplier;
            }

            PopulateRooms();

            // Get a pool of resources. This can be used as a multiplier on the curve to determine how much resources
            // each room should have. 
            _dangerPool = resources.hazardDanger * dangerMult;
            _goldPool = resources.value * valueMult;
            _leftoverDanger = 0;
            _leftoverGold = 0;
            
            Debug.Log(name + " was given a gold pool of " + _goldPool);
        
            // Loop for placing rewards and ordering hierarchy
            for (int i = 0; i < spawnedRooms.Count; i++)
            {
                // order this room in the hierarchy so the hierarchy order matches the room order
                spawnedRooms[i].transform.SetAsLastSibling();
                PopulateRoomRewards(i, curves);
            }
            
            // Loop for placing hazards
            for (int i = 0; i < spawnedRooms.Count; i++)
            {
                PopulateRoomHazards(i, curves);
            }

            string s = "hazards spawned: ";
            foreach (var h in hazards) s += " " + h.hazard.name + ", ";
            Debug.Log(s);

            // 'add a popup to tell player about the hazards when they open door
            if (firstRoom)
            {
                firstRoom.door.onOpen += CheckHazards;
                firstRoom.door.onOpen += BeginAdventureMusic;
                firstRoom.door.requireBoardingParty = hazards.Count > 0;
            }
            else if (_interior)
                _interior.onShow += CheckHazards;
        }

        #region rewards
        
        /// <summary>
        /// Determine how much gold to use in the room at the given index, and places rewards.
        /// </summary>
        void PopulateRoomRewards(int index, ExplorableCurves curves)
        {
            // Select the room at index i
            GameObject r = spawnedRooms[index];
            
            Room roomInstance = r.GetComponent<Room>();
            if (!roomInstance) return;

            // Set the amount of gold (value) to use for this room
            float goldThisRoom = _goldPool * curves.GoldValueOfRoomNormalized(index, spawnedRooms.Count) + _leftoverGold;
            _leftoverGold = 0;
            
            // Set values so we can easily inspect values placed into each room
            roomInstance.goldValue = goldThisRoom;
            
            // Roll to see if rewards can be placed in this room
            float chance = PopulationChance(roomInstance.goldValue, _goldPool, curves.rewardChance);
            if (!Roll(chance))
            {
                CarryOverGold(roomInstance);
                ClearRewards(r);
                return;
            }

            // Create rewards in the room at index i
            PlaceRewards(r, goldThisRoom);
        }
        
        /// <summary>
        /// Places treasure & crew in the room, and returns the remainder
        /// </summary>
        void PlaceRewards (GameObject room, float value)
        {
            // Find all the possible rewards
            List<IRewardable> rewards = room.GetComponentsInChildren<IRewardable>().ToList();

            room.GetComponent<Room>().rewardsFound = rewards.Count;

            // Shuffle the list of rewards
            SpiderWeb.Calc.ShuffleList(rewards);
            
            // sort rewards by priority - mostly because we want character placers to come first
            rewards = rewards.OrderBy(a => a.PopulatePriority()).ToList();

            float chance = 1;
            
            // Cycle through the shuffled list of rewards
            foreach (IRewardable reward in rewards)
            {
                // roll to see if this reward gets populated
                if (Roll(chance))
                {
                    Debug.Log("Populating reward with " + value + " gold in room " + room.name, room.gameObject);
                    
                    // Reduce the chance of the next treasure getting rolled
                    chance /= GameManager.Mode().rewardRatio;
                    
                    // Create reward, return the remainder
                    float remainder = reward.MakeReward(value);
                    value += remainder;
                }

                reward.DisableIfEmpty();
            }
        }

        /// <summary>
        /// Clears any reward placers in the given room
        /// </summary>
        void ClearRewards(GameObject room)
        {
            // Find all the possible rewards
            List<IRewardable> rewards = room.GetComponentsInChildren<IRewardable>().ToList();

            // disable the rewards
            foreach (IRewardable reward in rewards) reward.Disable();
        }
        
        /// <summary>
        /// Gets the gold back from this room and puts it in leftover gold to be used by further rooms.
        /// </summary>
        void CarryOverGold(Room room)
        {
            _leftoverGold = room.goldValue;
            room.goldValue = 0;
        }

        
        #endregion
        
        #region hazards
        void PopulateRoomHazards(int index, ExplorableCurves curves)
        {
            // Select the room at index i
            GameObject r = spawnedRooms[index];

            float dangerThisRoom = _dangerPool * curves.DangerOfRoomNormalized(index, spawnedRooms.Count);
                            
            // Set values so we can easily inspect values placed into each room
            Room roomInstance = r.GetComponent<Room>();
            if (!roomInstance) return;

            roomInstance.danger = dangerThisRoom + _leftoverDanger;
            
            //Debug.Log(roomInstance.name + " was given " + roomInstance.danger.ToString("0") + " danger.");
            
            // Check if conditions are right to put hazards in this room
            if (!ValidForHazards(roomInstance))
            {
                CarryOverDanger(roomInstance);
                return;
            }

            // Get the chance that this room has to place a hazard, and roll to see if it will place.
            float chance = PopulationChance(roomInstance.danger, _dangerPool, curves.hazardChance);
            if (!Roll(chance))
            {
                CarryOverDanger(roomInstance);
                return;
            }
            
            // get danger amt that we can use for the hazard roll. This value is used by the hazard roll query.
            _availableDanger = dangerThisRoom + _leftoverDanger; 
            
            //.Log("Total available danger is now " + _availableDanger.ToString("000"));

            // Roll for a hazaard
            Hazard h = hazardsTable.Roll<Hazard>(HazardRollQuery) as Hazard;
            if (h == null)
            {
                CarryOverDanger(roomInstance);
                return;
            }
            
            // create the hazard instance
            int hazardLevel = h.MaxLevelForDanger(_availableDanger);
            hazards.Add(h.ApplyToObject(r, hazardLevel));
            
            // If we've gotten to this point, we can assume that leftover danger was used.
            _leftoverDanger = 0;
        }

        /// <summary>
        /// Returns the chance that a room has to populate (gold, value, etc)
        /// </summary>
        /// <param name="roomValue">The value of the room</param>
        /// <param name="totalValue">The total value</param>
        /// <param name="curve">The 'chance of placement' curve</param>
        float PopulationChance(float roomValue, float totalValue, AnimationCurve curve)
        {
            float normValue = Mathf.Clamp01(roomValue / totalValue);
            return curve.Evaluate(normValue);
        }

        /// <summary>
        /// Gets the danger back from this room and puts it in leftover danger to be used by further rooms.
        /// </summary>
        void CarryOverDanger(Room room)
        {
            _leftoverDanger = room.danger;
            room.danger = 0;
        }
        
        /// <summary>
        /// Is it okay to place hazards in the given room?
        /// </summary>
        bool ValidForHazards(Room room)
        {
            if (room == firstRoom) return false;
            //if (!Roll(curves.ChanceOfHazard())) return false;
            if (_dangerPool <= 0) return false;
            if (!hazardsTable) return false;
            return true;
        }
        
        /// <summary>
        /// The roll query for hazards checks if the hazard at the lowest level can be purchased with the available danger.
        /// If there's more danger available, it finds the highest level hazard it can.
        /// </summary>
        public bool HazardRollQuery (Entry entry)
        {
            // Check if the entry is a hazard
            if (entry is Hazard)
            {
                // Cast the entry as a hazard
                Hazard h = entry as Hazard;

                // Using a loop, find the highest level hazard we can place with the available danger.
                int selectedLevel = 0;
                int minLevel = Mathf.RoundToInt(h.minMaxLevel.x);
                int maxLevel = Mathf.RoundToInt(h.minMaxLevel.y);
                
                for (int level = minLevel; level <= maxLevel; level++)
                {
                    // get danger of the hazard at level
                    int hazardDanger = h.DangerValue(level);
                    
                    // if the danger is greater than the allotted amount, break the loop
                    if (hazardDanger > _availableDanger) break;
                    
                    // if the danger was fine, iterate.
                    selectedLevel = level;
                }

                // If the level wasnt able to deget past zero, then this hazard isn't valid.
                if (selectedLevel < 1) 
                {
                    Debug.Log("hazard " + entry.name + " failed because value " + _availableDanger.ToString("0") + 
                              " is not enough to spawn it at the lowest level.");
                    return false;
                }
                
                return true;
                //return entry.AllTagsTrue(this);
            }

            Debug.LogError("Cannot attempt to spawn " + entry + " + because it's not a hazard.", gameObject);
            return false;
        }
        
        /// <summary>
        /// Check if there's hazards, and if so, query the player to make / change their boarding party.
        /// </summary>
        void CheckHazards ()
        {
            if (hazards.Count <= 0)
            {
                Debug.Log(name + " has no hazards. No need to check boarding party.");
                return;
            }
            
            // have player manager check with player to see if they have a boarding party
            PlayerManager.AskForBoardingParty();
        }

        #endregion

      
        #region rooms
        /// <summary>
        /// Get the starting room (nearest to the starting point)
        /// </summary>
        void FindStartingRoom ()
        {
            if (presetStartRoom) firstRoom = startRoom;
            

            // Search through rooms to find the starting room
            else
            {
                // Order rooms by their distance to the starting point
                OrderRoomsByDistance(startingPoint.position);
                
                // Set the rooms visibility and door
                foreach (Room r in availableRooms)
                {
                    r.door.gameObject.SetActive(false);
                    r.visible = false;
                }
                
                // Double check to see if any rooms were found at all
                if (availableRooms == null || availableRooms.Count < 1)
                {
                    Debug.Log("NO AVAILABLE ROOMS IN " + gameObject.name, gameObject);
                    return;
                }
                
                // The starting room is the first room in the list; the nearest to the starting point
                firstRoom = availableRooms.First();
                // Remove it from the list of available rooms
                availableRooms.Remove(firstRoom);
            }

            // Set all rooms but the starting room to invisible.
            firstRoom.door.requireBoardingParty = hazards.Count > 0;
            firstRoom.startVisible = true;
            firstRoom.SetVisible();
        }

        /// <summary>
        /// Orders the availableRooms list so that the ones closest to the given point are placed earlier in the list.
        /// </summary>
        void OrderRoomsByDistance(Vector3 point)
        {
            // Order rooms by their distance to the starting point
            availableRooms = availableRooms
                .OrderBy(r => Vector3.Distance(r.RoomCenter(), point)).ToList();
        }

        /// <summary>
        /// Links the starting room's door to the next room in line, and so on until the end of the rooms list
        /// </summary>
        void ConnectRooms ()
        {
            Room selectedRoom = firstRoom;
            
            // clear the spawned rooms list so it can be ordered by the dungeon order
            spawnedRooms.Clear();
            
            spawnedRooms.Add(firstRoom.gameObject);

            while (availableRooms.Count > 0)
            {
                // find the nearest room to the next room
                OrderRoomsByDistance(selectedRoom.RoomCenter());
                
                //availableRooms = availableRooms.OrderBy(r => Vector3.Distance(r.transform.position, selectedRoom.transform.position)).ToList();

                // Get the next room in the chain
                Room nextRoom = availableRooms[0];
                
                spawnedRooms.Add(nextRoom.gameObject);
                
                // tell the room what room will be on the other side of it's door
                selectedRoom.SetNextRoom(nextRoom);
                
                // iterate
                selectedRoom = nextRoom;
                availableRooms.Remove(selectedRoom);

                // If this is the last room...
                if (availableRooms.Count < 1)
                {
                    // If there's a pre-set end room, connect the last room to it.
                    if (presetEndRoom)
                    {
                        selectedRoom.SetNextRoom(endRoom);
                        lastRoom = endRoom;
                    }
                    else
                    {
                        lastRoom = selectedRoom;
                    }

                    // add 'dungeon cleared' to the last room
                    lastRoom.onBecomeVisible += DungeonCleared;
                }
            }
        }

        void DungeonCleared()
        {
            AllClearMusic();
            // TODO loc
            Notifier.DisplayNotification("Salvage cleared!", Color.yellow);
        }

        /// <summary>
        /// Instantiates the rooms into the grid, then finds the starting room and connects the rooms together.
        /// </summary>
        void CreateRoomsChain()
        {
            // Log if there's no room table
            if (roomsTable == null)
            {
                Debug.LogError("No rooms table attached!", gameObject);
                return;
            }
            
            // Roll to populate rooms
            for (int i = 0; i < 35; i++)
            {
                // Get an entry from the table
                RoomEntry newRoom = roomsTable.Roll<RoomEntry>(RollQuery) as RoomEntry;

                if (newRoom == null) break;

                // Create an instance of the selected room
                InteriorGrid newGrid = newRoom.Create(grid.transform.position, grid.transform.rotation).GetComponent<InteriorGrid>();

                newGrid.transform.localScale = newRoom.prefab.transform.localScale;

                // Try to insert the grid into this grid
                newGrid.TryInsertGrid(grid, true);

                spawnedRooms.Add(newGrid.gameObject);
                availableRooms.Add(newGrid.GetComponent<Room>());
                
                if (adjustRotation) newGrid.transform.localRotation = transform.localRotation;
            }

            FindStartingRoom();
            ConnectRooms();
        }

        [ButtonGroup("population")]
        void PopulateRooms()
        {
            FindTables();
            
            // Clear out previous rooms & lists
            Clear();
            
            // Roll to populate rooms
            CreateRoomsChain();
        }
        
        #endregion
        
        #region audio
        public void BeginAdventureMusic()
        {
            AKMusic.Get().SetNeutralExporable();
        }

        /// <summary>
        /// Plays the happy music when an interior is all clear
        /// </summary>
        public void AllClearMusic()
        {
            AKMusic.Get().SetAdventure(AdventureDifficulty.Cleared);
        }
        #endregion
        

        /// <summary>
        /// Roll with the given chance of success
        /// </summary>
        /// <param name="chance">A value between 0 and 1</param>
        bool Roll (float chance)
        {
            if (Random.Range(0f, 1.0f) > chance) return false;
            return true;
        }

        /// <summary>
        /// Removes all rooms, clears the grid, and resets lists
        /// </summary>
        [ButtonGroup("population")]
        public void Clear ()
        {
            foreach (GameObject GO in spawnedRooms)
            {
                // prevent destroying starting or end rooms
                Room r = GO.GetComponent<Room>();
                if (r == startRoom && presetStartRoom) continue;
                if (r == endRoom && presetEndRoom) continue;
                
#if UNITY_EDITOR
                DestroyImmediate(GO);
#else
               Debug.Log("DESPAWNING FROM:" + this.name , this); Destroy(GO);
#endif

            }
            spawnedRooms.Clear();
            hazards.Clear();
            availableRooms.Clear();
            firstRoom = null;

            grid = GetComponent<InteriorGrid>();
            grid.EmptyCells();
        }

        public bool RollQuery (Entry entry)
        {
            // Check if the entry is a room entry
            RoomEntry room = entry as RoomEntry;
            if (!room) return false;

            // Check if the room entry fits in the grid
            InteriorGrid entryGrid = room.prefab.GetComponent<InteriorGrid>();

            if (!entryGrid.TryInsertGrid(grid)) return false;

            return entry.MatchRollerTags(this);
        }


        public List<Tag> RollingTags
        {
            get {
                return rollingTags;
            }
            set {
                rollingTags = value;
            }
        }

        public void CombineTagList (List<Tag> tags){ }

        #endregion

        void OnDestroy ()
        {
            if (_interior) _interior.onShow -= CheckHazards;
        }
    }
}