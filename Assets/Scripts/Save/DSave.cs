using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.IO;
using Diluvion.Ships;

namespace Diluvion.SaveLoad
{

    /// <summary>
    /// Contains all static functions for loading, saving, and accessing the current save.
    /// </summary>
    public class DSave
    {
        //Static reference that all scripts can access and push their data to;
        public static DiluvionSaveData current;
        public static string currentSaveName;
        public static string currentFilePath;

        public static int maxNameLength = 30;

        const string OldDirectory = "/SaveFiles/";
        const string NewDirectory = "/SaveFiles_1-2/";

        public static DiluvionSaveData NewGame(string name)
        {
            Save(GameManager.Mode().startingSave.saveD, name);

            DiluvionSaveData newSave = Load(name, true);
            newSave.playerName = name;
            newSave.saveFileName = name; 
            currentSaveName = name;
            return newSave;
        }

        /// <summary>
        /// Saves that the room with given ID has been found / opened. 
        /// </summary>
        public static void SaveRoom(string roomID)
        {
            if (current == null) return;
            if (current.savedRooms.Contains(roomID)) return;
            current.savedRooms.Add(roomID);
        }

        /// <summary>
        /// Returns true if the room with the given ID has been saved as open.
        /// </summary>
        public static bool RoomIsOpen(string roomID)
        {
            if (current == null) return false;
            return current.savedRooms.Contains(roomID);
        }

        /// <summary>
        /// Has the given zone been discovered in the current save file?
        /// </summary>
        public static bool HasDiscoveredZone(GameZone zone)
        {
            if (current == null) return false;

            string n = zone.name;
            return current.discoveredZones.Contains(n);
        }

        public static void AddNewZone(GameZone zone)
        {
            current?.AddZone(zone);
        }

        #region directory and path

        /// <summary>
        /// Returns a full file path to access a save file of the given file name
        /// </summary>
        /// <param name="directory">The local directory of the save files. format the string as '/myFolder/'</param>
        /// <returns></returns>
        static string FilePath(string fileName, string directory)
        {
            string dirPath = SaveDirectory(directory);
            string fullFilename = fileName + ".ds";

            return dirPath + fullFilename;
        }

        
        static string FindFilePath(string fileName)
        {
            Debug.Log("Finding file path for file named " + fileName);
            
            // search the 1.2 folder first
            string newFilePath = FilePath(fileName, NewDirectory);
            if (File.Exists(newFilePath))
            {
                Debug.Log("File " + fileName + " was found in directory " + NewDirectory);
                return newFilePath;
            }

            string oldFilePath = FilePath(fileName, OldDirectory);
            if (File.Exists(oldFilePath))
            {
                Debug.Log("File " + fileName + " was found in directory " + OldDirectory);
                return oldFilePath;
            }
            
            Debug.LogError("File with name " + fileName + " couldn't be found.");
            return "none";
        }
        
        

        /// <summary>
        /// Returns the directory to the given local path. If the directory doesn't exist, creates it.
        /// </summary>
        /// <param name="localPath">Format this string as '/myFolder/'</param>
        static string SaveDirectory(string localPath)
        {
            string returnPath = Application.persistentDataPath + localPath;
            if (!Directory.Exists(returnPath))
                Directory.CreateDirectory(returnPath);
            return returnPath;
        }
        

        /// <summary>
        /// Returns a list of the FileInfo for each item in the save directory
        /// </summary>
        public static List<FileInfo> GetSaveFileInfo()
        {
            List<string> fileEntries = new List<string>();
            
            fileEntries.AddRange(Directory.GetFiles(SaveDirectory(NewDirectory)));
            fileEntries.AddRange(Directory.GetFiles(SaveDirectory(OldDirectory)));

            //Get the file info for each .ds file in the directory
            List<FileInfo> tempList = new List<FileInfo>();
            foreach (string s in fileEntries)
            {
                if (s.EndsWith(".ds")) tempList.Add(new FileInfo(s));
            }

            //orders the load list by last time modified
            List<FileInfo> returnList = tempList.OrderByDescending(f => f.LastWriteTime).ToList();

            return returnList;
        }

        /// <summary>
        /// Returns true if the file exists, false if it doesnt.
        /// </summary>
        public static bool CheckForSaveName(string saveName)
        {
            string fullPath = FindFilePath(saveName); //FilePath(saveName);
            bool returnbool = File.Exists(fullPath);
            return returnbool;
        }

        /// <summary>
        /// Sets the current file path to the given path.
        /// </summary>
        public static void SetCurrentFilePath(string path)
        {
            currentFilePath = path;
        }
        #endregion

        #region loading

        /// <summary>
        /// Reloads the current file from disc
        /// </summary>
        public static DiluvionSaveData Load(bool upgrade = false)
        {
            return Load(currentSaveName, upgrade);
        }

        /// <summary>
        /// Searches for the given fileName in the save file directory,
        /// Returns a diluvionSaveData class from the given filename. 
        /// </summary>
        public static DiluvionSaveData Load(string fileName, bool upgrade = false)
        {
            currentSaveName = fileName;
            //currentFilePath = FilePath(fileName);
            currentFilePath = FindFilePath(fileName);
            
            DiluvionSaveData data = SaveDataFromFile(currentFilePath);
            current = data;
            if (upgrade) data.Upgrade();

            return data;
        }


        public static DiluvionSaveData DebugLoad(string localFilePath)
        {
            return SaveDataFromFile(localFilePath);
        }

        public static DiluvionSaveData DebugLoad(DebugSave localFilePath)
        {
            return localFilePath.saveD;
        }
        #endregion

        #region saving
        /// <summary>
        /// Returns the diluvion savedata from the given file
        /// </summary>
        public static DiluvionSaveData SaveDataFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError("No file exists at path " + filePath);
                return null;
            }

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(filePath, FileMode.Open);
            DiluvionSaveData loadedSaveData = (DiluvionSaveData)bf.Deserialize(file);
            file.Close();

            return loadedSaveData;
        }

        /// <summary>
        /// Updates the dsave.current data with the latest player crew. Doesn't save to disk.
        /// </summary>
        public static void UpdatePlayerCrew()
        {
            if (PlayerManager.pBridge == null)
            {
                Debug.LogError("Attempting to save player's crew, but no player bridge exists!");
                return;
            }

            var crewManager = PlayerManager.pBridge.GetCrewManager();

            if (!crewManager)
            {
                Debug.LogError("Attempting to save player's crew, but no crew manager is attached to the player's bridge!");
            }

            current.SaveCrew(crewManager.AllCharactersOnBoard());
        }

        /// <summary>
        /// Saves a copy of the file with _autosave suffix
        /// </summary>
        public static void AutoSave()
        {
            Save(currentSaveName + "_autosave");
        }

        public static void Save()
        {
            Save(currentSaveName);
        }


        /// <summary>
        /// Saves all latest info to Dsave.current, then writes to the actual file
        /// </summary>
        /// <param name="newFileName">The name of the file created</param>
        public static void Save(string newFileName)
        {
            if (current == null)
            {
                Debug.LogError("No current save file is defined, can't save");
                return;
            }

            if (PlayerManager.PlayerShip() == null)
            {
                Debug.LogError("No player ship is defined, can't save position!");
            }
            else
            {
                current.SaveLocation(PlayerManager.PlayerShip().transform);
            }

            // Save the ship's crew
            UpdatePlayerCrew();

            // Save boarding party
            current.SaveBoardingParty(PlayerManager.BoardingParty());

            // save the current zone
            current.SaveZone();

            Save(current, newFileName);
        }

        /// <summary>
        /// Saves the given save file newSave to the file at the path of saveFileName.
        /// </summary>
        static void Save(DiluvionSaveData newSave, string saveFileName)
        {
            if (newSave == null)
            {
                Debug.LogError("Cancelled attempt to save a null save file!");
                return;
            }
                  
            // Save the file in the new directory for 1.2 files
            SaveToDisk(newSave, FilePath(saveFileName, NewDirectory));

            // show game saved GUI
            if(Application.isPlaying)
                UIManager.Create(UIManager.Get().gameSaved);
        }

        static void SaveToDisk(DiluvionSaveData data, string fullPath)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(fullPath);

            Debug.Log("Saved to disk at " + fullPath + " version: " + data.savedVersion);

            bf.Serialize(file, data);
            file.Close();
        }
        #endregion
    }
}