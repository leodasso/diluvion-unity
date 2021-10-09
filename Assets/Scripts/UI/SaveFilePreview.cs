using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Diluvion;
using Diluvion.SaveLoad;
using SpiderWeb;
using UnityEngine.EventSystems;
using Rewired;


namespace DUI
{

	public class SaveFilePreview : MonoBehaviour
	{

		public TextMeshProUGUI saveFileTextObj;
		public TextMeshProUGUI zoneNameText;
		public TextMeshProUGUI creationTimeTxt;
		public TextMeshProUGUI currentQuestText;
		public TextMeshProUGUI goldAmtText;
		public TextMeshProUGUI crewAmtText;
		public Image shipIconImage;
		public CanvasGroup canvasGroup;
	    [Space] 
	    public PopupObject tryDeletePopup;
		public DiluvionSaveData saveData;
		public string saveFileName;

		public LoadItem saveFile;
		const string scenePrefix = "scene_";
		const string questPrefix = "quest_";
		const string questSuffix = "_title";

		FileInfo _fileInfo;
		LoadMenu _parentLoadMenu;

		void Awake()
		{
			canvasGroup.interactable = false;
			canvasGroup.alpha = 0;
		}

		void Start()
		{
			_parentLoadMenu = GetComponentInParent<LoadMenu>();
		}

		public void Init(LoadItem fileSlot)
		{
			canvasGroup.interactable = true;
			canvasGroup.alpha = 1;
			saveFile = fileSlot;
			
			_fileInfo = saveFile.fileInfo;

            // Get the file info name
            saveFileName = Path.GetFileNameWithoutExtension(_fileInfo.Name);

            //last time this info was written to
            DateTime writeTime = _fileInfo.LastWriteTime;
            string formattedWriteTime = writeTime.ToShortDateString() + " " + writeTime.ToShortTimeString();
            creationTimeTxt.text = formattedWriteTime;

            // Get the diluvionSaveData from the file name
            saveData = DSave.Load(saveFileName);
            if (saveData == null) return;

            // Display name of save file
            saveFileTextObj.text = saveFileName;

            // display name of most recent zone
            var mostRecentZone = saveData.LastZone();
            string mostRecentZoneName = "";
            if (mostRecentZone != null) mostRecentZoneName = mostRecentZone.name;
            
            else // If no most recent zone object exists, this must be an older save file. Search for zone name the old way
            {
                if (saveData.savedZones != null && saveData.savedZones.Count > 0)
                {
                    string zoneName = saveData.savedZones[0].ToString();
                    zoneNameText.text = Localization.GetFromLocLibrary(scenePrefix + zoneName, zoneName);
                }
            }

            zoneNameText.text = mostRecentZoneName;

            // Display name of most recent quest
            if (saveData.savedQuests != null && saveData.savedQuests.Count > 0)
            {
                string questname = questPrefix + saveData.displayedQuestLocKey + questSuffix;
                currentQuestText.text = Localization.GetFromLocLibrary(questname, "");
            }
            else currentQuestText.text = "";

            // display icon of most recent ship
            Sprite shipSprite = ShipSprite(saveData);
            if (shipSprite != null) shipIconImage.sprite = shipSprite;

            // display gold
            int gold = saveData.shipInventory.gold;
            goldAmtText.text = gold.ToString();

            // display how many crew members
            crewAmtText.text = SavedCrewCount().ToString();
        }

        /// <summary>
        /// Opens up the 'are you sure?' delete file panel
        /// </summary>
        public void TryDelete()
        {
            tryDeletePopup.CreateUI(ReallyDelete, null);
        }

        /// <summary>
        /// Actually deletes the save file.
        /// </summary>
        public void ReallyDelete()
        {
            File.Delete(_fileInfo.FullName);
	        Destroy(saveFile.gameObject);
	        _parentLoadMenu.HighlightFirstSave();
        }
        
        
        void SetCurrentSelectable(GameObject go)
        {
            if (EventSystem.current.currentSelectedGameObject == go) return;

            // Check for player's most recent input type
            Player player = ReInput.players.GetPlayer(0);

            Controller lastUsedController = player.controllers.GetLastActiveController();

            if (lastUsedController != null)
                if (lastUsedController.type == ControllerType.Mouse) return;

            EventSystem.current.SetSelectedGameObject(go);
        }


        /// <summary>
        /// Calls for startmenu to load this filename.
        /// </summary>
        public void Load()
        {
            GameManager.BeginGame(saveData);
        }
	    
		int SavedCrewCount()
		{
			if (saveData.savedCharacters != null)
			{
				return saveData.savedCharacters.Count; 
			}

			if (saveData.crewSaveData != null)
			{
				return saveData.crewSaveData.Count;
			}

			return 0;
		}
        
		Sprite ShipSprite(DiluvionSaveData data)
		{
			if (saveData.playerShips == null) return null;
			if (saveData.playerShips.Count < 1) return null;
			return saveData.playerShips[0].ChassisObject().shipIcon;
		}

    }
}
