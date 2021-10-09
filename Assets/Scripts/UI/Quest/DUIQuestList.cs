using UnityEngine;
using System.Collections.Generic;
using Quests;
using Diluvion.SaveLoad;

namespace DUI
{

    public class DUIQuestList : MonoBehaviour
    {

        public Transform questGrid;
        public DUIQuest questGUIPrefab;

        // Use this for initialization
        void Start ()
        {

            if (DSave.current == null) return;

            // Reverse the lists so that the most recent appear on top.
            List<DQuestSave> reversedList = new List<DQuestSave>(DSave.current.questSaves);
            reversedList.Reverse();

            //get list of quest saves to populate with
            foreach (DQuestSave qs in reversedList)
            {

                DQuest quest = QuestsGlobal.GetQuest(qs.key);
                if (quest == null) continue;

                DUIQuest newQuest = Instantiate(questGUIPrefab) as DUIQuest;
                newQuest.transform.SetParent(questGrid.transform, false);
                newQuest.Init(quest);
            }
        }
    }
}