using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SpiderWeb;
using Diluvion.SaveLoad;
using Diluvion.Ships;
using Diluvion.AI;
using Diluvion.Sonar;
using Sirenix.OdinInspector;
using DUI;

namespace Diluvion
{

    public class PlayerManager : MonoBehaviour
    {
        public static Bridge pBridge;
        static PlayerManager pm;

        [ReadOnly]
        public string playerName = "Captain Leo";

        public LocTerm underAttack;

        public PopupObject createBoardingParty;
        public PopupObject changeBoardingParty;

        public GameObject shipWindZone;
        public GameObject playerSpotlight;

        [ReadOnly]
        public CompassRose compassRoseInstance;

        [ReadOnly, ShowInInspector]
        List<Character> boardingParty = new List<Character>();


        public static PlayerManager Get ()
        {
            if (pm)
                return pm;
            pm = WorldControl.Get().GetComponent<PlayerManager>();
            return pm;
        }

        #region queries
        public static GameObject PlayerShip ()
        {
            if (pBridge == null)
                return null;
            return pBridge.gameObject;
        }

        static ShipControls _playerControls;
        public static ShipControls PlayerControls()
        {
            if (_playerControls) return _playerControls;
            if (!PlayerShip()) return null;
            _playerControls = PlayerShip().GetComponent<ShipControls>();
            return _playerControls;
        }

        public static CrewManager PlayerCrew ()
        {
            if (!PlayerShip()) return null;
            if (!pBridge) return null;
            if (!pBridge.Interior()) return null;

            return pBridge.Interior().GetComponent<CrewManager>();
        }

        /// <summary>
        /// Checks to see if there's any hostiles nearby. Returns true if the player is in a safe area. This is a fairly heavy function,
        /// and should only be used to check if it's safe to save, for example when the pause menu pops up.
        /// </summary>
        public static bool PlayerIsInSafeArea()
        {
            if (PlayerShip() == null) return false;

            // Don't allow saving if the player is below their crush depth
            if (PlayerHull())
            {
                float minDepth = _playerHull.testDepth;
                if (PlayerShip().transform.position.y < minDepth) return false;
            }

            foreach (var col in Physics.OverlapSphere(PlayerShip().transform.position, 30))
            {
                IAlignable alignable = col.GetComponent<IAlignable>();
                if (alignable == null) continue;

                var alignment = alignable.getAlignment();
                if (alignment != AlignmentToPlayer.Hostile) continue;
                float dist = Vector3.Distance(PlayerShip().transform.position, col.transform.position);
                if (dist < alignable.SafeDistance())
                {
                    Debug.Log("Hostile " + col.name + " found near player. Player's not in a safe area.");
                    return false;
                }
            }

            return true;
        }

        static SonarStats _sonarStats;
        public static SonarStats PlayerSonarStats()
        {
            if (_sonarStats) return _sonarStats;
            if (!PlayerShip()) return null;
            _sonarStats = PlayerShip().GetComponent<SonarStats>();
            return _sonarStats;
        }

        public static Transform PlayerTransform ()
        {
            if (!pBridge)
                return null;
            
            return pBridge.transform;
        }

        public static InteriorManager GetPlayerInterior ()
        {
            if (!pBridge)
                return null;
            return pBridge.interiorManager;
        }

        /// <summary>
        /// If the player is docked to anything, this will undock them.
        /// </summary>
        public static void UndockPlayer()
        {
            if (!PlayerDocks()) return;

            PlayerDocks().Undock();
        }

        static DockControl _playerDocks;

        public static DockControl PlayerDocks()
        {
            if (!PlayerShip()) return null;
            if (_playerDocks) return _playerDocks;
            _playerDocks = PlayerShip().GetComponent<DockControl>();
            return _playerDocks;
        }

        static Hull _playerHull;
        public static Hull PlayerHull()
        {
            if (_playerHull) return _playerHull;
            if (!PlayerShip()) return null;
            _playerHull = PlayerShip().GetComponent<Hull>();
            return _playerHull;
        }
        #endregion


        static Inventory _playerInv;
        public static Inventory PlayerInventory()
        {
            if (_playerInv) return _playerInv;
            if (PlayerShip() == null) return null;

            _playerInv = PlayerShip().GetComponent<Inventory>();
            return _playerInv;
        }



        /// <summary>
        /// Checks if players are in range
        /// </summary>
        public static bool IsPlayerInRange (Transform t, float range)
        {
            if (PlayerShip() == null)
                return false;
            return Vector3.Distance(PlayerShip().transform.position, t.position) < range;
        }



        #region player ship instance creation

        /// <summary>
        /// Sets up the given bridge to be the player ship.
        /// </summary>
        /// <param name="newShipTransform">New ship transform.</param>
        /// <param name="createCrew">If set to <c>true</c> create crew from the save file / default crew.</param>
        /// <param name="changeTarget">If set to <c>true</c> change the camera target to the new ship.</param>
        public static void SetPlayerShip(Bridge newShipBridge, bool createCrew, bool changeTarget)
        {
            Get();
            Debug.Log("Player manager instance: " + Get().name, Get().gameObject);
            
            //Set this sub as the current player sub instance
            pBridge = newShipBridge;
            
            // make sure UI is on
            UIManager.ShowAllUI(true);

            // Turn on docks (if off)
            DockControl docks = pBridge.GetComponent<DockControl>();
            if (docks)
                docks.dockActive = true;

            // Place wind zone in player ship so it interacts with fishies
            Get().AddWindZone(pBridge.transform);

            // place the spotlight
            GameObject newLight = Instantiate(Get().playerSpotlight, newShipBridge.transform) as GameObject;
            newLight.transform.localPosition = Vector3.zero;

            //Check for ship controls component
            GO.MakeComponent<ShipControls>(newShipBridge.gameObject).enabled = true;

            // Set the player death delegate so we know when player died
            Hull h = newShipBridge.GetHull();
            h.crippledModel = null;
            h.useCrushDepth = true;

            h.myDeath += PlayerDied;

            // Add compass to the ship 
            CompassRose.AddTo(newShipBridge.transform);

            // Set the camera target to the new player ship
            OrbitCam.SetTarget(pBridge.transform);

            // Set up all the UI on the ship
            List<IHUDable> hudElements = new List<IHUDable>();
            hudElements.AddRange(pBridge.GetComponentsInChildren<IHUDable>());

            foreach (IHUDable hudElement in hudElements)
                hudElement.CreateUI();
            
            // Set up transparency
            SeeThrougher seeThrougher = pBridge.gameObject.AddComponent<SeeThrougher>();
            seeThrougher.SetAsMaster();

            BoardingParty().Clear();

            //Add each character from the crew save to the player's ship.
            if (DSave.current != null)
            {
                foreach (CharSave save in DSave.current.savedCharacters)
                {
                    Character ch = save.CreateCharacter();
                    if(ch==null)continue;
                    
                    newShipBridge.crewManager.AddCrewman(ch);

                    // Check if they're on the boarding party
                    if (DSave.current.boardingParty!=null&&DSave.current.boardingParty.Contains(ch.NonLocalizedName()))
                    {
                        Sailor s = ch as Sailor;
                        if (s)
                            BoardingParty().Add(s);
                    }
                }
            }
            else Debug.Log("No save file while trying to create player crew.");
        }

        /// <summary>
        /// Swaps the player from their current ship chassis to the newShip. Carries over crew, inventorie, and whatever slots it can fit.
        /// </summary>
        public static void SwapPlayerShip(SubChassis newShip)
        {
            if (!PlayerShip())
            {
                Debug.Log("Can't swap player ship because no player ship could be found!");
                return;
            }
            
            SubChassisData newChassisData = DSave.current.SavedPlayerShip();
            newChassisData.chassisName = newShip.name;
            
            DSave.current.SaveLocation(PlayerShip().transform);
            
            InstantiatePlayerSub(newChassisData);
        }

        /// <summary>
        /// Instantiate the given sub. If there was already a player sub, removes it.
        /// </summary>
        public static void InstantiatePlayerSub (SubChassisData newChassis)
        {
            Debug.Log("Player manager is instantiating the player ship...");
            
            //Remove the old sub
            Bridge oldSub = pBridge;
            if (oldSub)
            {
                Debug.Log("Destroying old sub " + oldSub.name);
                oldSub.hull.myDeath -= PlayerDied;
                Destroy(oldSub.gameObject);
            }

            //Instantiate the new sub
            GameObject newSubInstance = newChassis.InstantiateSub();
         
            Bridge newBridge = newSubInstance.GetComponent<Bridge>();

            //Set up the new ship as the player's ship
            SetPlayerShip(newBridge, true, false);

            PlaceSub(newSubInstance);
        }

        /// <summary>
        /// Instantiate the given sub. If there was already a player sub, removes it.
        /// </summary>
        public static void DebugInstantiatePlayerSub(SubChassis chassis, Vector3 positionToSpawn , ShipBuildSettings sbs = null)
        {
            //Remove the old sub
            Bridge oldSub = pBridge;
            
           
            if (oldSub)
            {
                Debug.Log("Destroying old sub " + oldSub.name);
                oldSub.hull.myDeath -= PlayerDied;
                Destroy(oldSub.gameObject);
            }

            if (sbs == null)
                sbs = chassis.defaultBuild;
            //Instantiate the new sub
            GameObject newSubInstance = chassis.InstantiateChassis(null,chassis.defaultBuild,true);

            Bridge newBridge = newSubInstance.GetComponent<Bridge>();

            /*
            // Add air tanks
            AirTanks newTank = GO.MakeComponent<AirTanks>(newBridge.gameObject);
            newTank.clampToInventory = true;
            newTank.air = 1;
            */
            
            //Set up the new ship as the player's ship
            SetPlayerShip(newBridge, true, false);

            if (oldSub != null)
                positionToSpawn = oldSub.transform.position;
                    
            newSubInstance.transform.position = positionToSpawn;
            //PlaceSub(newSubInstance);
        }

        /// <summary>
        /// Places the given object at the appropriate spawn position. <para>If currently travelling, chooses travel point.
        /// Otherwise chooses saved position.</para>
        /// </summary>
        static void PlaceSub (GameObject sub)
        {
            PlayerSpawnPoint spawnPt = null;

            # if UNITY_EDITOR
            // Check for override spawn points (debug only)
            if (GameManager.DebugMode() && PlayerSpawnPoint.OverrideSpawn() != null)
            {
                spawnPt = PlayerSpawnPoint.OverrideSpawn();
                SpawnFromPoint(spawnPt, sub.transform);
                return;
            }
            #endif

            // Check if coming in from travelling from a different zone
            if (GameManager.PreviousZone())
            {
                spawnPt = PlayerSpawnPoint.SpawnPoint(GameManager.PreviousZone());
                if (spawnPt)
                {
                    Debug.Log("Travel from previous zone " + GameManager.PreviousZone().name +
                        " complete. found spawn point: " + spawnPt.name, spawnPt);
                    
                    SpawnFromPoint(spawnPt, sub.transform);
                    return;
                }
                
                Debug.Log("Previous zone found, but no spawn points have that zone!");
                GameManager.ClearPreviousZone();
            }

            // Check save file for a saved position
            if (DSave.current != null)
            {
                // saved position 
                if (DSave.current.posSaved)
                {
                    SetTransform(sub.transform, DSave.current.SavedPosition(), DSave.current.SavedRotation());
                    return;
                }

                // saved checkpoint (for old save files)
                spawnPt = PlayerSpawnPoint.SpawnPoint(DSave.current.savedCheckPoint);
                if (spawnPt)
                {
                    SpawnFromPoint(spawnPt, sub.transform);
                    return;
                }
            }

            // Check for a default spawn point
            spawnPt = PlayerSpawnPoint.DefaultSpawn();
            if (spawnPt)
            {
                Debug.Log("Found default spawn point! ", spawnPt);
                SpawnFromPoint(spawnPt, sub.transform);
                return;
            }

            Debug.Log("No spawn points were found! Placing player sub in wherever the fuck.");
        }

        /// <summary>
        /// invokes any events on the spawn point, and places the player sub there.
        /// </summary>
        static void SpawnFromPoint(PlayerSpawnPoint point, Transform playerSub )
        {
            //Debug.Log(point.name + " invoking events...");
            point.onPlayerSpawned.Invoke();
            SetTransform(playerSub.transform, point.transform.position, point.transform.rotation);
        }

        /// <summary>
        /// Adds a wind zone (used for interacting with fish) to the given transform.
        /// </summary>
        void AddWindZone (Transform parent)
        {
            if (!shipWindZone)
                return;

            GameObject windZoneInst = Instantiate(shipWindZone, parent, false);
            windZoneInst.transform.localPosition = Vector3.zero;
            windZoneInst.transform.localEulerAngles = Vector3.zero;
        }

        public List<OceanCurrent> currents = new List<OceanCurrent>();
        OceanCurrent _mostRecentCurrent;

        void Update()
        {
            if (PlayerInStream() && _mostRecentCurrent)
            {
                PlanktonParticles.SetPlanktonForce(_mostRecentCurrent.ForwardForce());
                
                // lerp drag effect force direction
                if (_currentDragInstance)
                {
                    foreach (var p in _currentDragInstance.GetComponentsInChildren<ParticleSystem>())
                        ParticleHelper.SetParticleForce(_mostRecentCurrent.ForwardForce(), p);
                }
            }
        }
        
        public static void EnterOceanCurrent(OceanCurrent current)
        {
            if (Get().currents.Contains(current)) return;
            
            // If the player wasn't already in an ocean current, enter it
            if (!Get().PlayerInStream()) Get().BeginOceanCurrentTravel();
            
            Get().currents.Add(current);
            Get()._mostRecentCurrent = current;
            OceanCurrentParticle.Get().EnterStream(current);
        }
        

        public static void ExitOceanCurrent(OceanCurrent current)
        {
            // Remove the current
            Get().currents.Remove(current);
            
            OceanCurrentParticle.Get().ExitStream(current);
            if (Get().PlayerInStream() == false) Get().EndOceanCurrentTravel();
            if (Get()._mostRecentCurrent == current) Get()._mostRecentCurrent = null;
        }

        void BeginOceanCurrentTravel()
        {
            SpiderSound.MakeSound("Play_Current_Loop", PlayerShip());
            
            // instantiate the effect on player ship
            _currentDragInstance = Instantiate(OceanCurrentDragEffectPrefab(), PlayerShip().transform);
            _currentDragInstance.transform.localPosition = Vector3.zero;
        }

        void EndOceanCurrentTravel()
        {
            SpiderSound.MakeSound("Stop_Current_Loop", PlayerShip());
            
            // remove effect from player ship
            if (_currentDragInstance)
            {
                foreach (var p in _currentDragInstance.GetComponentsInChildren<ParticleSystem>())
                {
                    var e = p.emission;
                    e.enabled = false;
                }
                
                Destroy(_currentDragInstance, 5);
                _currentDragInstance = null;
            }
        }

        GameObject _currentDragInstance;
        
        GameObject _currentDragPrefab;
        GameObject OceanCurrentDragEffectPrefab()
        {
            if (_currentDragPrefab) return _currentDragPrefab;
            _currentDragPrefab = Resources.Load<GameObject>("effects/ocean current drag");
            return _currentDragPrefab;
        }


        bool PlayerInStream()
        {
            return currents.Count > 0;
        }
        
        #endregion

        #region subs
        /// <summary>
        /// Adds the given chassis to the player's list of subs.
        /// </summary>
        public static void AddNewPlayerShip (SubChassis newChassis)
        {

            if (DSave.current == null)
                return;
            DSave.current.playerShips.Add(new SubChassisData(newChassis));
        }

        /// <summary>
        /// Checks if the player's ships list already contains the given chassis.
        /// </summary>
        public static bool PlayerHasShip (SubChassis chassis)
        {
            if (DSave.current == null)
                return false;

            foreach (SubChassisData d in DSave.current.playerShips)
            {
                SubChassis savedChassis = SubChassisGlobal.GetChassis(d.chassisName);
                if (savedChassis.Equals(chassis))
                    return true;
            }
            return false;
        }


        /// <summary>
        /// Upgrades the given data, instantiates the new upgraded sub and switches to it.
        /// </summary>
        public static void UpgradeSub (SubChassisData dataToUpgrade)
        {
            if (dataToUpgrade.Upgrade())
                InstantiatePlayerSub(dataToUpgrade);
        }


        /// <summary>
        /// Purchases the given sub, and switches to it. Doesn't do money exchange.
        /// </summary>
        public static void PurchaseSub (SubChassisData newSub)
        {
            SubChassisData newSubData = new SubChassisData(newSub);

            if (DSave.current != null)
                DSave.current.playerShips.Add(newSubData);

            InstantiatePlayerSub(newSubData);
        }


        static void SetTransform (Transform t, Vector3 position, Quaternion rotation)
        {
            t.position = position;
            t.rotation = rotation;
        }

        /// <summary>
        /// Gets the highest level playership
        /// </summary>
        public static int GetHighestLevelPlayerShip ()
        {
            if (DSave.current == null)
                return 1;
            int highestLevel = 0;
            foreach (SubChassisData data in DSave.current.playerShips)
                highestLevel = Mathf.Max(data.ChassisObject().shipLevel);

            return highestLevel;
        }

        #endregion


        public static bool IsPlayerAlive ()
        {
            if (pBridge == null)
                return false;
            if (pBridge.gameObject.activeInHierarchy)
                return true;

            return false;
        }


        /// <summary>
        /// This function is meant to be added to the player hull death delegate.
        /// When called, does the whole 'game over' sequence.
        /// </summary>
        static void PlayerDied (Hull playerBridge, string byWho)
        {
            // hide the UI
            UIManager.ShowAllUI(false);

            //Saves the death
            if (DSave.current != null)
            {
                DSave.current.deathTimes++;
                //SpiderAnalytics.LogDeath(playerBridge.transform.position, byWho);
            }

            //AKMusic.Get().RemoveCombatSuddenly();
            Get().StartCoroutine(nameof(DeadPlayer));
        }
        
        #region boarding party
        
        public static List<Character> BoardingParty ()
        {
            return Get().boardingParty;
        }

        /// <summary>
        /// If the player has no boarding party, prompts them to create one. If they do, asks
        /// if they want to make any changes to it.
        /// </summary>
        public static void AskForBoardingParty ()
        {
            // If they have a boarding party...
            if (BoardingParty().Count > 0)

                // Create a popup for changing boarding party
                Get().changeBoardingParty.CreateUI(ChangeBoardingParty, null);

            // Create a popup for creating boarding party
            else
                Get().createBoardingParty.CreateUI(ChangeBoardingParty, null);
        }

        /// <summary>
        /// Opens the 'select boarding party' window
        /// </summary>
        static void ChangeBoardingParty ()
        {
            UIManager.Create(UIManager.Get().boardingPartyPanel as BoardingPartyPanel);
        }

        /// <summary>
        /// Returns a random member of the player's boarding party
        /// </summary>
        public static Sailor RandomBoardingPartyMember()
        {
            if (BoardingParty() == null) return null;
            
            // choose a random character from the player's boarding party - memorized as a static so that actions can
            // access it if they need to.
            int sailorIndex = Random.Range(0, BoardingParty().Count);
            return BoardingParty()[sailorIndex] as Sailor;
        }
        
        #endregion

        public static void GivePlayerItems(List<StackedItem> items, bool overflowToStorage)
        {
            // Find the player inventory
            if (PlayerInventory() == null)
            {
                Debug.LogError("Player inventory couldn't be found! Can't give items.");
                return;
            }
			
            // give the items
            foreach (StackedItem itemStack in items)
                PlayerInventory().AddItem(itemStack, overflowToStorage);
        }
        
        public static Inventory PlayerStorage ()
        {
            if (Get() == null) return null;
            return Get().GetComponent<Inventory>();
        }


        IEnumerator DeadPlayer ()
        {
            float deathCount = 0;
            while (deathCount < GameManager.Mode().reSpawnTime)
            {
                deathCount += Time.unscaledDeltaTime;
                yield return null;
                if (GameManager.State() == GameState.Ending || GameManager.State() == GameState.Loading)
                    yield break;
            }

            GameManager.GameOver();
        }
    }
}