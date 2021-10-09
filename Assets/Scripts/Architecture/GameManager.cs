using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEngine;
using PathologicalGames;
using Diluvion.SaveLoad;
using DUI;
using Rewired;
using UnityEngine.SceneManagement;

namespace Diluvion
{

    /// <summary>
    /// The overarching game manager for Diluvion. Can load scenes, game modes, save files, etc and does it all
    /// in the correct order.
    /// </summary>
    [CreateAssetMenu(fileName = "game manager", menuName = "Diluvion/Game/game manager", order = 0)]
    public class GameManager : ScriptableObject
    {
        static GameManager gameManager;
        static SpawnPool pool;
        static Player player;
        

        public GameState currentState = GameState.Starting;
        public GameMode gameMode;
        public BuildSettings buildSettings;
        public GameZone currentZone;
        public GameObject rewiredInputManager;
        public MusicSettingsObject musicGlobals;
        public BankHolder wwiseBanks;
        public GameObject poolPrefab;
        public GameObject travelCompassPrefab;

        public Shader stippleShader;

        public BankHolder bankInstance;

        public bool saveAs;
        public string newSaveName;

        [Tooltip("Editor only. Builds always load zones.")]
        public bool loadZone;
        
        [Tooltip("Controls if Cosmetic scenes loaded in editor. They're are always loaded in builds. ")]
        public bool loadCosmeticScenes;

        public bool debug;

        /// <summary>
        /// List of all objects calling for the game to freeze.
        /// </summary>
        public List<Object> freezers = new List<Object>();

        [Space, Tooltip("This save file will only be used while testing from scene. Loading from in-game menu ignores it.")]
        public string saveFile;
        public DiluvionSaveData saveData;

        // The zone travelled from. Null unless recently travelled.
        GameZone previousZone;

        GameObject _inputManagerInstance;
        bool createdInputManager;

        public static bool Exists()
        {
            return gameManager != null;
        }

        public static GameManager Get()
        {
            if (gameManager) return gameManager;
            gameManager = Resources.Load("managers/game manager") as GameManager;
            return gameManager;
        }

  
        /// <summary>
        /// Returns the Rewired input player 0 (i.e. the only player)
        /// </summary>
        public static Player Player()
        {
            if (ReInput.players == null)
                PrepInput();

            if (player != null) return player;
            player = ReInput.players.GetPlayer(0);
            return player;
        }


        #region queries 
        public static GameState State()
        {
            return Get().currentState;
        }

        /// <summary>
        /// Returns the current game mode.
        /// </summary>
        public static GameMode Mode()
        {
            return Get().gameMode;
        }

        public static BuildSettings BuildSettings()
        {
            return Get().buildSettings;
        }

        public static GameObject TravelCompassPrefab()
        {
            return Get().travelCompassPrefab;
        }

        public static bool DebugMode()
        {
            return Get().debug;
        }

        public static SpawnPool Pool()
        {
            if (pool) return pool;
            pool = Instantiate(Get().poolPrefab).GetComponent<SpawnPool>();
            return pool;
        }
        #endregion

        #region game file

        /// <summary>
        /// Loads the data of the current save file name into saveData slot.
        /// </summary>
        public static void LoadSaveData()
        {
            LoadSaveData(Get().saveFile);
        }

        /// <summary>
        /// Loads data from a file with the given name into the saveData slot. Doesn't affect DSave.current,
        /// and doesn't begin game. If 'new game' option is true on game manager, loads the starting save file from
        /// the current mode.
        /// </summary>
        public static void LoadSaveData(string dataName)
        {
            Get().saveData = DSave.Load(dataName, true);
        }

        /// <summary>
        /// Creates and loads a new save file with the given name.
        /// </summary>

        public static void LoadNewGame(string newName = "new guy")
        {
            Get().saveData = DSave.NewGame(newName);
        }

        public static void LoadShipSelection()
        {
            SceneManager.LoadScene("ship selection");
        }

        /// <summary>
        /// If the given object is a member of the spawn pool, despawns it. Otherwise destroys it.
        /// </summary>
        public static void Despawn(GameObject GO)
        {
            if (Pool().IsSpawned(GO.transform))
            {
                Pool().Despawn(GO.transform);
                return;
            }
            Destroy(GO);
        }

        #endregion

        public static void LoadSoundBanks()
        {
            if (Get().bankInstance != null) return;
            if (Get().wwiseBanks == null) Get().wwiseBanks = Resources.Load<BankHolder>("wwiseBanks");
            
            Get().bankInstance = Instantiate(Get().wwiseBanks);
            
        }

        static bool ShouldLoadZone(bool force)
        {
            bool willLoad = true;
#if UNITY_EDITOR
            willLoad = Get().loadZone;
#endif
            return willLoad || force;
        }
        
        /// <summary>
        /// Begins the game with the given save data. Uses the game mode currently in the manager.
        /// </summary>
        public static void BeginGame( bool forceLoad = false)
        {
            // destroy interior cam
            //InteriorView.Clear();
            
            // destroy world object
            WorldControl.Clear();
            
            PrepInput();
            Time.timeScale = 1;
            
            // Clear list of 'freezers'
            Get().freezers.Clear();

            OrbitCam.transitionLock = false;

            if (ShouldLoadZone(forceLoad))
            {
                UIManager.Clear();
                InteriorView.Clear();
                Convo.ResetConvoTimes();
            }

            #if UNITY_EDITOR
            // Save a different file than the one loaded (optional)
            if (Get().saveAs)
            {
                DSave.currentSaveName = Get().newSaveName;
                DSave.current.saveFileName = Get().newSaveName;
            }
            #endif

            if (!Mode())
            {
                Debug.LogError("Game manager needs a game mode before game can begin!", Get());
                return;
            }

            LoadSoundBanks();
            
            // find the correct zone to load
            GameZone zoneToLoad = DSave.current.LastZone();
            if (!zoneToLoad) zoneToLoad = Mode().defaultZone;
            Get().currentZone = zoneToLoad;
            
            
            // Load the zone from the save data (unless the current mode doesn't allow zone loading)
            if (ShouldLoadZone(forceLoad))
                // instantiate the player ship when loading is done.
                Loader.Load(zoneToLoad.ScenesToLoad(Get().loadCosmeticScenes), CreatePlayerShip);
            
            // If skipping loading, just go ahead and create the player ship.
            else CreatePlayerShip();
        }

        /// <summary>
        /// Begins the game with the given save data. Uses the game mode currently in the manager.
        /// </summary>
        public static void BeginGame(DiluvionSaveData saveData, bool forceLoad = false)
        {
            // Set up the save data
            DSave.current = saveData;
            BeginGame(forceLoad);
        }

        
        static void CreatePlayerShip()
        {
            QuestManager.GameLoaded();
            
            CheckPoint.lastTimeSaved = Time.unscaledTime;
            Debug.Log("Game manager creating player ship at " + Time.unscaledTime);
            PlayerManager.InstantiatePlayerSub(DSave.current.SavedPlayerShip());
            Get().currentState = GameState.Running;
        }


        /// <summary>
        /// Gets the game ready to take ReWired inputs
        /// </summary>
        public static void PrepInput()
        {
            if (Get()._inputManagerInstance != null) return;
            Get()._inputManagerInstance = Instantiate(Get().rewiredInputManager);
        }

        public static InputManager RewiredInputManager()
        {
            PrepInput();
            return Get()._inputManagerInstance.GetComponent<InputManager>();
        }

        #region zones
        
        /// <summary>
        /// Begins travelling to the next zone.
        /// </summary>
        /// <param name="nextZone">The next zone to load</param>
        public static void TravelTo(GameZone nextZone)
        {
            Debug.Log("Recieved request to travel to " + nextZone.name);
            Get().previousZone = CurrentZone();
            nextZone.Load();
        }

        public static GameZone PreviousZone()
        {
            return Get().previousZone;
        }

        public static GameZone CurrentZone()
        {
            return Get().currentZone;
        }

        public static void ClearPreviousZone()
        {
            Get().previousZone = null;
        }
        #endregion

        /// <summary>
        /// Brings up the 'game over' screen
        /// </summary>
        public static void GameOver()
        {
            UIManager.Create(UIManager.Get().gameOver as EndMenu);
        }

        
        /// <summary>
        /// Reloads the last saved save file. This isn't DSave.current; it's the file that was last saved to disc.
        /// Use this for loading from any file, reloading last checkpoint.
        /// </summary>
        public static void ReloadFromFile(bool forceLoad = false)
        {
            BeginGame(DSave.Load(), forceLoad);
        }

        /// <summary>
        /// Reload's the game scenes using dsave.current. Use this for switching scenes.
        /// </summary>
        public static void Reload(bool forceLoad = false)
        {
            BeginGame(forceLoad);
        }
        

        /// <summary>
        /// Quits to main menu.
        /// </summary>
        public static void MainMenu()
        {
            Get().currentState = GameState.Starting;

            //Restarts the save for this session
            DSave.current = null;
            
            UIManager.ClearAll();
            
            // destroy interior cam
            InteriorView.Clear();
            
            // destroy world object
            WorldControl.Clear();

            FullUnFreeze();
            Loader.Load("diluvion_startMenu");
        }

        #region pause and resume

        /// <summary>
        /// Freezes the game and brings up the pause menu.
        /// </summary>
        public static void Pause()
        {
            // If already paused or in popup, don't pause.
            if (Get().currentState == GameState.Paused) return;
            if (Get().currentState == GameState.popup) return;

            // Show the pause menu
            UIManager.Create(UIManager.Get().pauseMenu);

            Freeze(Get());
        }

        /// <summary>
        /// Unfreezes the game and removes the pause menu.
        /// </summary>
        public static void Resume()
        {
            // Can't resume from travel menu
            if (Get().currentState == GameState.TravelMenu) return;

            //cant pause during popup
            if (Get().currentState == GameState.popup) return;

            UIManager.Clear<PauseMenu>();

            UnFreeze(Get());
        }


        public static void Freeze(Object caller)
        {
            CleanFreezeList();
            
            Debug.Log(caller.name + " is calling for a freeze!");
            Get().currentState = GameState.Paused;

            // freeze time
            Time.timeScale = 0;

            // Set cursor locking
            if (Application.isEditor) Cursor.lockState = CursorLockMode.None;
            else Cursor.lockState = CursorLockMode.Confined;

            if (!Get().freezers.Contains(caller)) Get().freezers.Add(caller);
        }

        public static void UnFreeze(Object caller)
        {
            Get().freezers.Remove(caller);

            CleanFreezeList();
            
            Debug.Log(caller.name + " called for unfreeze. current freezers: ");
            foreach (var VARIABLE in  Get().freezers)
            {
                Debug.Log(VARIABLE.name);
            }

            // If there's other objects calling for a freeze, don't un-freeze yet
            if (Get().freezers.Count > 0) return;

            Debug.Log("un-freezing!");
            Time.timeScale = 1;
            //TimeControl.SetTimeScale(1, true);
            Get().currentState = GameState.Running;
        }

        /// <summary>
        /// Overrides any freezers and completely un-freezes the game
        /// </summary>
        public static void FullUnFreeze()
        {
            Get().freezers.Clear();
            UnFreeze(Get());
        }

        static void CleanFreezeList()
        {
            Get().freezers = Get().freezers.Where(x => x != null).ToList();
        }

        #endregion
    }


    public enum GameState
    {
        Starting,
        Loading,
        Saving,
        CaptainsTools,
        Running,
        TravelMenu,
        Paused,
        Ending,
        Dying,
        popup
    }
}