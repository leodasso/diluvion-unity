using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using TMPro;
using Diluvion.SaveLoad;

namespace DUI
{
    /// <summary>
    /// UI Element to display the info of a save file.
    /// </summary>
    public class LoadItem : MonoBehaviour
    {
        public Image bgImage;
        public Color defaultColor = Color.white;
        public Color newColor = Color.yellow;
        [Space]
        public TextMeshProUGUI saveFileTextObj;
        public TextMeshProUGUI creationTimeTxt;
        [Space]
        public DiluvionSaveData saveData;
        public string saveFileName;

        LoadMenu _parentLoadMenu;

        public FileInfo fileInfo;

        public void Init(FileInfo finfo)
        {
            _parentLoadMenu = GetComponentInParent<LoadMenu>();
            
            fileInfo = finfo;

            // Get the file info name
            saveFileName = Path.GetFileNameWithoutExtension(fileInfo.Name);

            //last time this info was written to
            DateTime writeTime = finfo.LastWriteTime;
            string formattedWriteTime = writeTime.ToShortDateString() + " " + writeTime.ToShortTimeString();
            creationTimeTxt.text = formattedWriteTime;

            // Get the diluvionSaveData from the file name
            saveData = DSave.Load(saveFileName);
            if (saveData == null) return;

            // Display name of save file
            saveFileTextObj.text = saveFileName;

            // Change the color to reflect which version this save file is from
            bgImage.color = saveData.savedVersion >= 1.2f ? newColor : defaultColor;
        }

        public void SelectFile()
        {
            if (!_parentLoadMenu)
            {
                Debug.LogError("No load menu could be found in ancestry!", gameObject);
                return;
            }
            
            _parentLoadMenu.SelectFile(this);
        }
    }
}