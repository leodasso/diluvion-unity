using System.Collections.Generic;
using UnityEngine;
using Quests;
using System;
using System.Linq;
using Diluvion;
using Diluvion.Ships;
using Sirenix.OdinInspector;
using CharacterInfo = Diluvion.CharacterInfo;
using Diluvion.Achievements;
// DO NOT CHANGE THESE, THEYRE LOC KEYS
public enum Zones
{
    ForgottenFjords,
    RoyalRuins,
    DwellerDepths,
    Explorables
}

//The data we use for saving and loading, gets pushed to file
//WARNING, RENAMING AND REMOVING VARIABLES FROM THIS CLASS WILL BREAK OLD SAVE FILES
//ADDING NEW ONES HOWEVER WILL NOT

[Serializable]
public class DiluvionSaveData
{
    #region vars

    public string saveFileName;

    public string playerName;
    public string savedCheckPoint;
    public float savedVersion = 0.0f; //allows for checking what version of the save file is in use on load.

    public bool posSaved = false;

    // Since vector3 & Quaternion can't be serialized in .net, just save them as floats.
    public float pos_x;
    public float pos_y;
    public float pos_z;

    public float rot_x;
    public float rot_y;
    public float rot_z;
    public float rot_w;

    public List<string> shownDialogKeys = new List<string>();
    public List<string> shownChatterKeys = new List<string>();
    public List<string> discoveredZones = new List<string>();
    
    public int homeBaseLevel;
    public int creatureKills;
    public int shipKills;
    
    public List<CharSave> savedCharacters = new List<CharSave>();
    
    /// <summary>
    /// Similar to savedCharacters, except characters are never removed from this list.
    /// </summary>
    public List<CharSave> characterHistory = new List<CharSave>();
    
    
    public List<string> boardingParty = new List<string>();
    public List<string> savedPopups = new List<string>();
    public InventorySave shipInventory;
    public List<InventorySave> savedInventories = new List<InventorySave>();
    public List<int> savedInstanceIds = new List<int>();
    public List<SwitchSave> savedSwitches = new List<SwitchSave>();

    /// <summary>
    /// List of any item player has acquired during full playthrough, NOT what's currently in inventory.
    /// </summary>
    public List<string> itemsAcquired = new List<string>();
    public List<DQuestSave> questSaves = new List<DQuestSave>();

    /// <summary>
    /// Player ships. 0 index is the active sub, the rest are in storage.
    /// </summary>
    public List<SubChassisData> playerShips = new List<SubChassisData>();

    /// <summary>
    /// The number of upgrades the player has forged in their playthrough
    /// </summary>
    public int forgedUpgrades;

    public int captainTimeUpgrades;
    public List<string> knownLandmarks = new List<string>();

    /// <summary>
    /// List of the names of tutorials that have been completed.
    /// </summary>
    public List<string> tutorialsCompleted = new List<string>();
    public bool debugSave;
    public float timePlayed = 0;
    public int deathTimes = 0;
    public string saveFileID = "";

   // public List<SpiderAchievement> achievements = new List<SpiderAchievement>();
    public List<string> foundCharts = new List<string>();

    public List<string> savedRooms = new List<string>();

    #region defunct
    [ReadOnly]
    public List<Zones> savedZones = new List<Zones>();
    [ReadOnly]
    public string saveVersion = "0.0"; // NOT USED
    [ReadOnly]
    public int tutorialsDone = 0;
    [ReadOnly]
    public bool tutorialSkipped = false;
    [ReadOnly]
    public List<string> resourceItemNames = new List<string>();
    [ReadOnly]
    List<string> metCrewNames = new List<string>();
    [ReadOnly]
    public bool bakeToFile;
    [ReadOnly]
    public List<ShipSave> savedShips;
    [ReadOnly]
    public string displayedQuestLocKey;
    [ReadOnly]
    public int indexOfShipBeingUsed;
    [ReadOnly]
    public List<QuestSave> savedQuests = new List<QuestSave>();
    [ReadOnly]
    public List<QuestSave> unsavedQuests = new List<QuestSave>();
    [ReadOnly]
    public float fovAdjust = 0;
    [ReadOnly]
    public List<CrewSave> crewSaveData;
    #endregion

    #endregion

    public DiluvionSaveData()
    {
        InitLists();
    }

    /// <summary>
    /// Initializes the saveData with the given player name.
    /// </summary>
    /// <param name="pName">Player name</param>
    public DiluvionSaveData(string pName)
    {
        NewGameSet(pName);
    }

    #region file upgrading
    public void Upgrade()
    {
        UpgradeToVer12();
        UpgradeToVer121();
        UpgradeToVer122();
        UpgradetoVer123();
        UpgradeToVer124();
    }

    /// <summary>
    /// Updates the save file to work with new quest save system
    /// </summary>
    void UpgradeToVer12()
    {
        if (SetVersion(1.2f)) return;

        shownDialogKeys = new List<string>();
        if (questSaves == null) questSaves = new List<DQuestSave>();

        Debug.Log(savedQuests.Count + " quests saved in an older version. Updating now...");
        foreach (QuestSave qs in savedQuests)
        {
            if (qs == null) continue;
            DQuestSave newQuestSave = new DQuestSave(qs);
            questSaves.Add(newQuestSave);
        }
    }

    /// <summary>
    /// Updates the file to work with the new sub chassis system
    /// </summary>
    void UpgradeToVer121()
    {
        if (SetVersion(1.21f)) return;

        if (playerShips == null) playerShips = new List<SubChassisData>();

        Debug.Log(savedShips.Count + " ships saved in an older version. Updating now...");
        foreach (ShipSave ss in savedShips)
        {
            if (ss == null) continue;
            if (!ss.IsValid()) continue;

            SubChassis chassis = SubChassisGlobal.GetChassisFromPrefab(ss.currentShipType);
            if (chassis == null)
            {
                Debug.LogError("Ship " + ss.currentShipType + " couldn't be found!");
                continue;
            }

            SubChassisData newData = new SubChassisData(chassis);

            // Since loading an old file, add the default loadout of upgrades
            if (chassis.defaultBuild.loadout) chassis.defaultBuild.loadout.AddToChassis(newData);

            newData.decalName = ss.appliedEmblemItem;
            playerShips.Add(newData);
        }

        Debug.Log("<color=green>Complete!</color> Save data is now version " + savedVersion);
    }

    void UpgradeToVer122()
    {
        if (SetVersion(1.22f)) return;

        if (savedCharacters == null) savedCharacters = new List<CharSave>();        
        if (boardingParty == null) boardingParty = new List<string>();

        Debug.Log(crewSaveData.Count + " old characters need updated.");
        foreach (CrewSave cs in crewSaveData)
        {
            savedCharacters.Add(ConvertCrewSave(cs));
        }

        UpdateCharacterHistory();
    }

    /// <summary>
    /// Update saved zone data
    /// </summary>
    void UpgradetoVer123()
    {
        if (SetVersion(1.23f)) return;

        discoveredZones = new List<string>();

        foreach (Zones z in savedZones)
        {
            GameZone gz = ZonesGlobal.GetZone(z);
            discoveredZones.Add(gz.name);
        }

        Debug.Log("<color=green>Complete!</color> Save data is now version " + savedVersion);
    }

    /// <summary>
    /// Checks dialog keys
    /// </summary>
    void UpgradeToVer124()
    {
        if (SetVersion(1.24f)) return;

        Debug.Log("Checking for convos that have likely been said...");
        foreach (CharSave c in savedCharacters)
        {
            SailorSave ss = c as SailorSave;
            if (ss != null)
            {
                Dialogue d = CharactersGlobal.GetDialogue(ss.dialogueName);
                if (d)
                {
                    foreach (Convo convo in d.conversations)
                        convo.CheckSavedStatus(Sailor.NonLocalizedSailorName(ss.generatedName));
                }else
                    Debug.Log("<color=red>No dialogue found for " + Sailor.NonLocalizedSailorName(ss.generatedName) + "</color>");

            }else
            {
                CharacterInfo info = CharactersGlobal.GetCharacter(c.charObjName);
                if (!info) continue;

                Dialogue d = info.dialogue;
                if (!d) continue;

                foreach (Convo convo in d.conversations)
                {
                    if (!convo) continue;
                    convo.CheckSavedStatus(info.niceName);
                }
            }
        }
    }

    CharSave ConvertCrewSave(CrewSave cs)
    {
        // Get the name of this crew save
        string n = cs.crewName;
        foreach (string s in cs.generatedName) n += " " + s;
        Debug.Log("Checking crewsave " + n + " ...");

        // Check if the character info can be found for this crew save
        CharacterInfo info = CharactersGlobal.GetCharacter(cs.crewName);

        if (info != null)
        {
            // Check if the character is a sailor or otherwise
            if (info.prefab.GetComponent<Sailor>())
            {
                SailorSave sailorSave = ConvertToSailorSave(cs, info);
                Debug.Log("<color=green>Successfully converted to sailor save:</color> " + sailorSave);
                return sailorSave;
            }

            CharSave save = new CharSave();
            save.charObjName = info.name;
            save.savedLevel = 4;
            Debug.Log("<color=green>Successfully converted to char save:</color> " + save.charObjName, info);
            return save;
        }

        SailorSave genSailor = ConvertToSailorSave(cs);
        Debug.Log("<color=green>Successfully converted to sailor save:</color> " + genSailor);
        return genSailor;
    }

    /// <summary>
    /// Converts a crew save to a sailor save. Optional character info will overwrite certain properties.
    /// </summary>
    SailorSave ConvertToSailorSave(CrewSave cs, CharacterInfo info = null)
    {
        SailorSave s = new SailorSave();
        s.generatedName = new List<string>(cs.generatedName);
        s.stats = new List<float>(cs.stats);
        s.gender = cs.gender;
        s.voiceType = cs.voiceType;
        s.savedStation = cs.stationName;
        s.animName = cs.controllerName;
        s.dialogueName = cs.dialogueName;

        Appearance a = CharactersGlobal.AppearanceForController(cs.controllerName);
        if (a)
        {
            Debug.Log("Appearance found for controller " + cs.controllerName);
            s.appearanceName = a.name;
        }

        if (info)
        {
            s.charObjName = info.name;
            s.gender = info.gender;
            s.voiceType = info.voiceType;
            if (info.appearance)
                if (info.appearance.animController)
                    s.animName = info.appearance.animController.name;
            s.dialogueName = info.dialogue.name;
        }
        return s;
    }

    /// <summary>
    /// Checks if this file is the given version. Returns true if it is. If not, sets the version to new version.
    /// </summary>
    bool SetVersion(float newVersion)
    {
        if (savedVersion >= newVersion) return true;

        savedVersion = newVersion;
        return false;
    }
    #endregion

    /// <summary>
    /// Saves the given list of crew to saved characters list.
    /// </summary>
    public void SaveCrew(List<Character> crew)
    {
        savedCharacters.Clear();
        foreach (Character ch in crew)
        {
            if (!ch) continue;
         //   Debug.Log("Saving character data for " + ch.name, ch);
            savedCharacters.Add(ch.CreateSaveData());
        }

        UpdateCharacterHistory();
    }

    /// <summary>
    /// Updates character history with the latest versions currently in savedCharacters
    /// </summary>
    public void UpdateCharacterHistory()
    {
        if (characterHistory == null) characterHistory = new List<CharSave>();
        
        // Characters should never be removed from characterHistory, only updated. As such, characterHistory isn't cleared.
        foreach (var charSave in savedCharacters)
            TryAddCharacterToHistory(charSave);
    }

    public CharSave DataForCharacterInfo(CharacterInfo character)
    {
        foreach (var charSave in characterHistory)
        {
            if (charSave.charObjName == character.name) return charSave;
        }
        return null;
    }

    /// <summary>
    /// Attempts to add the given character to the characterHistory list. If it already contains the character, skips it.
    /// </summary>
    void TryAddCharacterToHistory(CharSave character)
    {
        // Remove null entries from history.
        characterHistory = characterHistory.Where(x => x != null).ToList();
        
        // If character is already in history, remove the previous iteration
        characterHistory = characterHistory.Where(x => x.charObjName != character.charObjName).ToList();
        
        // add the newest version to character history.
        characterHistory.Add(character);
    }

    /// <summary>
    /// Saves the names of the boarding party
    /// </summary>
    public void SaveBoardingParty(List<Character> crew)
    {
        boardingParty.Clear();
        foreach (Character ch in crew)
            boardingParty.Add(ch.NonLocalizedName());
    }

    #region quests
    /// <summary>
    /// Saves progress for the objective.
    /// </summary>
    public void SaveObjectiveProgress(DQuest forQuest, Objective obj)
    {
        if (!HasQuest(forQuest))
        {
            Debug.Log("Can't save progress for objective " + obj.name + " because quest " + forQuest.name + " isn't in the save file.");
            return;
        }

        GetQuest(forQuest).ProgressObjective(obj);
    }

    /// <summary>
    /// Returns true if the given objective is currently in progress under the given quest.
    /// </summary>
    public bool IsObjectiveInProgress(DQuest quest, Objective o)
    {
        if (!HasQuest(quest)) return false;
        return GetQuest(quest).ObjectiveInProgress(quest, o);
    }

    public bool IsObjectiveComplete(DQuest quest, Objective o)
    {
        if (!HasQuest(quest)) return false;
        return GetQuest(quest).ObjectiveComplete(quest, o);
    }


    /// <summary>
    /// Return the quest save related to the given quest object. Returns null if 
    /// none exist in this save file.
    /// </summary>
    public DQuestSave GetQuest(DQuest q)
    {
        foreach (DQuestSave qs in questSaves)
            if (qs.key == q.name) return qs;

        Debug.LogError(q.name + " doesn't exist in the save file!");
        return null;
    }

    public void CompleteQuest(DQuest quest)
    {
        if (HasQuest(quest)) GetQuest(quest).complete = true;
    }


    /// <summary>
    /// Is the given quest in the save file? Doesn't specify if complete / in progress.
    /// </summary>
    public bool HasQuest(DQuest quest)
    {
        foreach (DQuestSave qs in questSaves)
            if (qs.key == quest.name)
            {
                //Debug.Log("Dsave has quest " + quest.name);
                return true;
            }
        //Debug.Log("Dsave doesnt have quest " + quest.name);
        return false;
    }

    /// <summary>
    /// Returns true if the given quest is currently at the given quest status.
    /// </summary>
    public bool IsQuestStatus(DQuest quest, QuestStatus status)
    {
        switch (status)
        {
            case QuestStatus.NotStarted:
                if (!HasQuest(quest)) return true;
                break;
                
            case QuestStatus.Complete:
                if (!HasQuest(quest)) return false;
                return GetQuest(quest).complete;
                
            case QuestStatus.InProgress:
                if (!HasQuest(quest)) return false;
                return !GetQuest(quest).complete;
                
            case QuestStatus.NotFinished:
                if (HasQuest(quest))
                {
                    if (GetQuest(quest).complete) return false;
                }
                return true;
                
            case QuestStatus.InProgOrComplete:
                return HasQuest(quest);
        }
        return false;
    }

    #endregion


    /// <summary>
    /// Creates a new game with the given player name
    /// </summary>
    /// <param name="pName">The player's name</param>
    void NewGameSet(string pName)
    {
        playerName = pName;
        savedCheckPoint = "NONE";
        saveFileName = playerName;

        homeBaseLevel = 0;

        InitLists();
    }

    #region ships
    /// <summary>
    /// Sets the given data as the active player sub. It gets moved to the 0 index of playerships
    /// </summary>
    public void SwapPlayerActiveSub(SubChassisData newActive)
    {
        if (playerShips.Contains(newActive)) playerShips.Remove(newActive);
        playerShips.Insert(0, newActive);
    }

    /// <summary>
    /// Returns the last played ship
    /// </summary>
    public SubChassisData SavedPlayerShip()
    {
        return playerShips[0];
    }

    /// <summary>
    /// Cleans a list of bad saveShip data.
    /// </summary>
    List<ShipSave> CleanCopySaveShips(List<ShipSave> sSaves)
    {
        List<ShipSave> returnList = new List<ShipSave>();
        foreach (ShipSave ss in sSaves)
        {
            if (ss != null && ss.IsValid())
                returnList.Add(ss);
        }
        return returnList;
    }


    /// <summary>
    /// Adds the given ship to the list of player's subs.
    /// </summary>
    /// <param name="setAsCurrent">If true, sets the given ship data as the current sub.</param>
    public void AddShip(SubChassisData ship, bool setAsCurrent = false)
    {
        if (setAsCurrent) playerShips.Insert(0, ship);
        else playerShips.Add(ship);
    }

    //Checks my achievements for subs
    public void CheckMaxSubs()
    {
        int score = 0;
        bool snapper = false;
        bool saber = false;
        bool glaciemmk2 = false;


        foreach (ShipSave ss in savedShips)
        {
            if (ss == null) continue;

            if (ss.currentShipType == "ship_saber" && !saber)
            {
                score++;
                saber = true;
            }
            if (ss.currentShipType == "ship_snapper" && !snapper)
            {
                score++;
                snapper = true;
            }
            if (ss.currentShipType == "ship_glaciemType2" && !glaciemmk2)
            {
                score++;
                glaciemmk2 = true;
            }
        }
        if (SpiderAchievementHandler.Get() == null) return;
        SpiderAchievementHandler.Get().SetAchievement("ach_masterAndCommander", score);
    }

    #endregion

    #region tutorials

    public void TrySaveTutorial(string tutorialTag)
    {

        if (tutorialsCompleted == null) tutorialsCompleted = new List<string>();
        if (!tutorialsCompleted.Contains(tutorialTag)) tutorialsCompleted.Add(tutorialTag);
    }

    public bool TutorialComplete(string tutorialTag)
    {
        if (tutorialsCompleted == null) return false;
        if (tutorialsCompleted.Contains(tutorialTag)) return true;
        return false;
    }
    #endregion

    public void SetSaveFileID(string id)
    {
        saveFileID = id;
    }

    #region inventory saving
    /// <summary>
    /// Checks if the player has ever gained this item in their whole play-through.
    /// if they haven't, saves it to the list of acquired items.
    /// </summary>
    public bool HasGottenItem(string itemName)
    {

        if (itemsAcquired == null) return false;
        if (itemsAcquired.Contains(itemName)) return true;
        else
        {
            itemsAcquired.Add(itemName);
            return false;
        }
    }

    /// <summary>
    /// Saves the players current inventory to the save file.
    /// </summary>
    public void SavePlayerInventory(Inventory inv)
    {
        //Debug.Log("Saving the player's inventory.");
        shipInventory = new InventorySave(inv);
    }

    public InventorySave LoadShipInventory()
    {
        return shipInventory;
    }

    public InventorySave TryGetInventorySave(string locKey)
    {

        int count = 0;
        InventorySave selectedSave = null;
        foreach (InventorySave save in savedInventories)
            if (save.locKey == locKey)
            {
                count++;
                selectedSave = save;
            }

        if (count == 0)
        {
            //	Debug.LogWarning("Inventory save " + locKey + " doesn't exist in current save file.");
            return null;
        }

        if (count > 1)
        {
            //Debug.LogError("Multiple inventory saves exist for locKey " + locKey);
            return null;
        }

        //	Debug.Log("Successfully found save file for " + locKey);
        return selectedSave;
    }

    public void SaveMiscInventory(Inventory inv, string invName)
    {
        //Debug.Log("Saving misc inventory " + inv.name);
        InventorySave newSave = new InventorySave(inv, invName);
        SaveMiscInventory(newSave);
    }

    public void SaveMiscInventory(InventorySave invSave)
    {

        int count = 0;
        InventorySave selectedSave = null;
        if (savedInventories == null) savedInventories = new List<InventorySave>();

        foreach (InventorySave s in savedInventories)
            if (s.locKey == invSave.locKey)
            {
                count++;
                selectedSave = s;
            }

        if (count > 1) Debug.LogError("Multiple inventory saves exist for key " + invSave.locKey);

        // If there's already a save for this inventory, overwrite it
        if (selectedSave != null)
            savedInventories.Remove(selectedSave);

        InventorySave newSave = new InventorySave(invSave);
        savedInventories.Add(newSave);
    }
    #endregion

    //Saves an instance id
    public void SaveInstance(int idToSave)
    {
        if (HasInstanceId(idToSave)) return;
        //Debug.Log("saving Instance ID" + idToSave + " to safefile");
        if (savedInstanceIds == null) savedInstanceIds = new List<int>();

        savedInstanceIds.Add(idToSave);
    }

    //sees if an instance id is in the save file
    public bool LoadInstance(int idToLoad)
    {
        return HasInstanceId(idToLoad);
    }

    bool HasInstanceId(int id)
    {
        if (savedInstanceIds == null || savedInstanceIds.Count < 1) return false;
        return savedInstanceIds.Contains(id);
    }

    #region popups
    /// <summary>
    /// Call to save that the popup has been shown.
    /// </summary>
    public void SavePopup(string tag)
    {
        if (savedPopups.Contains(tag)) return;
        savedPopups.Add(tag);
    }

    /// <summary>
    /// Returns true if the popup has already been shown in this save file, 
    /// false if otherwise.
    /// </summary>
    public bool ShownPopup(string tag)
    {
        return savedPopups.Contains(tag);
    }
    #endregion

    public Vector3 SavedPosition()
    {
        return new Vector3(pos_x, pos_y, pos_z);
    }

    public Quaternion SavedRotation()
    {
        return new Quaternion(rot_x, rot_y, rot_z, rot_w);
    }

    #region cartographer chart save / load

    /// <summary>
    /// Saves the given chart info to the save data.
    /// </summary>
    public void SaveChart(ChartInfo chartInfo)
    {
        if (!foundCharts.Contains(chartInfo.chartItem.name))
        {
            foundCharts.Add(chartInfo.chartItem.name);
            Debug.Log("Added " + chartInfo.chartItem.name + " to list of cartographer's found charts.");
        }
    }

    /// <summary>
    /// Returns the 'found' status of the given chart.
    /// </summary>
    public bool LoadChartStatus(ChartInfo chart)
    {
        if (foundCharts.Contains(chart.chartItem.name)) return true;
        return false;
    }

    #endregion

    void InitLists()
    {
        savedZones = new List<Zones>() { Zones.ForgottenFjords };
        savedShips = new List<ShipSave>();
        crewSaveData = new List<CrewSave>();
        shipInventory = new InventorySave();
        savedQuests = new List<QuestSave>();
        unsavedQuests = new List<QuestSave>();
        savedRooms = new List<string>();
    }


    /// <summary>
    /// Returns true if the landmark was new, false if it has already been discovered.
    /// </summary>
    public bool AddLandmark(LandMark lm)
    {
        if (knownLandmarks.Contains(lm.landmarkLocKey)) return false;
        knownLandmarks.Add(lm.landmarkLocKey);
        return true;
    }
    
    /// <summary>
    /// Add the current zone if it's not already in the list. Also, place it at the 0
    /// index in the zones list.
    /// </summary>
    public void SaveZone()
    {
        GameZone currentZone = GameManager.Get().currentZone;
        if (currentZone == null) return;
        AddZone(currentZone);
    }

    public void AddZone(GameZone zone)
    {
        discoveredZones.Remove(zone.name);
        discoveredZones.Insert(0, zone.name);
    }

    /// <summary>
    /// Returns the last zone that was saved in this data. If none, returns null.
    /// </summary>
    public GameZone LastZone()
    {
        if (discoveredZones == null) return null;
        if (discoveredZones.Count < 1) return null;
        return ZonesGlobal.GetZone(discoveredZones[0]);
    }

    #region dialog



    /// <summary>
    /// Memorize to the save file that this dialogue has been said by this crewmember.
    /// </summary>
    public void AddDialogKey(Convo c, Character crew)
    {
        AddDialogKey(c, crew.NonLocalizedName());
    }

    /// <summary>
    /// Memorize that this dialogue has been said by the given character.
    /// </summary>
    /// <param name="c">Conversation</param>
    /// <param name="charName">The NON localized name of the character.</param>
    public void AddDialogKey(Convo c, string charName)
    {
        if (shownDialogKeys.Contains(DialogKey(c.name, charName))) return;
        shownDialogKeys.Add(DialogKey(c.name, charName));
    }

    /// <summary>
    /// Save that the given chatter has been shown in the radio.
    /// </summary>
    public void AddChatterKey(Convo c)
    {
        if (shownChatterKeys == null) shownChatterKeys = new List<string>();
        if (shownChatterKeys.Contains(c.name)) return;
        shownChatterKeys.Add(c.name);
    }

    /// <summary>
    /// Has the given convo been previously shown in the chatter?
    /// </summary>
    public bool ShownChatterKey(Convo c)
    {
        if (shownChatterKeys == null) return false;
        return shownChatterKeys.Contains(c.name);
    }

    /// <summary>
    /// Returns true if this key has been shown; false otherwise.
    /// </summary>
    public bool ShownDialogKey(string convoName, Character crew)
    {
        if (!crew) return false;
        return shownDialogKeys.Contains(DialogKey(convoName, crew.NonLocalizedName()));
    }

    public bool ShownDialogKey(string convoName)
    {
        foreach (string s in shownDialogKeys)
            if (DialogTrimmedKey(s) == convoName) return true;

        return false;
    }

    /// <summary>
    /// Returns a key that can be used to get a dialogue from the 'key + crew' string combo.
    /// </summary>
    public static string DialogTrimmedKey(string originalKey)
    {
        string[] s = originalKey.Split('%');
        return s[0];
    }

    string DialogKey(string key, string name)
    {
        return key + "%_" + name;
    }
    #endregion

    /// <summary>
    /// Saves the position and rotation of the given transform T as the spawn point.
    /// </summary>
    public void SaveLocation(Transform t)
    {
        timePlayed += Time.timeSinceLevelLoad;
        posSaved = true;

        // clear saved checkpoint so we can transition away from it
        savedCheckPoint = string.Empty;

        pos_x = t.position.x;
        pos_y = t.position.y;
        pos_z = t.position.z;

        rot_w = t.rotation.w;
        rot_x = t.rotation.x;
        rot_y = t.rotation.y;
        rot_z = t.rotation.z;
    }
}
