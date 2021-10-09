using UnityEngine;
using System.Collections;

namespace DUI
{
    public class DebugQuestMenu : DUIDebug
    {
        public LoadMenu loadMenu;
        LoadMenu loadMenuInstance;

        public void WarpToNextObjective()
        {
            CheatMan().QuestWarp();
        }

        public void LoadMenu()
        {
            loadMenuInstance = Instantiate<LoadMenu>(loadMenu);
            loadMenuInstance.transform.SetParent(transform, false);
            loadMenuInstance.GetSavesList();
        }
    }
}