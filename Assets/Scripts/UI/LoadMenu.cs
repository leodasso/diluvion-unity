using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Diluvion.SaveLoad;

namespace DUI
{
    public class LoadMenu : DUIView
    {
        public RectTransform saveUIParent;
        public LoadItem saveSlotPrefab;
        public SaveFilePreview filePreviwer;

        List<FileInfo> saveFileNames;
        List<GameObject> saveFileDisplays = new List<GameObject>();

        protected override void Start()
        {
            base.Start();
            GetSavesList();
            HighlightFirstSave();
        }


        public bool GetSavesList()
        {
            // Destroy any slots currently being displayed
            SpiderWeb.GO.DestroyChildren(saveUIParent);

            saveFileNames = DSave.GetSaveFileInfo();
            if (saveFileNames.Count < 1) return false;

            // Create a new UI panel for each file info
            foreach (FileInfo f in saveFileNames)
            {
                saveFileDisplays.Add(CreateSaveFileslot(f));
            }

            return true;
        }

        /// <summary>
        /// Has the event system set the first save slot as the current selected
        /// </summary>
        public void HighlightFirstSave()
        {
            saveFileDisplays = saveFileDisplays.Where(x => x != null).ToList();
            if (saveFileDisplays.Count < 1) return;
            EventSystem.current.SetSelectedGameObject(saveFileDisplays[0]);
            SelectFile(saveFileDisplays[0].GetComponent<LoadItem>());
        }

        public void SelectFile(LoadItem file)
        {
            filePreviwer.Init(file);
        }

        /// <summary>
        /// Creates the GUI for the save file, and returns the game object of the UI.
        /// </summary>
        GameObject CreateSaveFileslot(FileInfo finfo)
        {
            LoadItem slotInstance = Instantiate(saveSlotPrefab) as LoadItem;
            slotInstance.transform.SetParent(saveUIParent, false);
            slotInstance.transform.SetAsLastSibling();

            slotInstance.Init(finfo);

            return slotInstance.gameObject;
        }

        public override void End()
        {
            PauseMenu.Reset();
            base.End();
        }
    }
}