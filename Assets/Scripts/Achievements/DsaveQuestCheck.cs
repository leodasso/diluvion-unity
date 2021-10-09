using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quests;

namespace Diluvion.Achievements
{
    

    [CreateAssetMenu(fileName = "quest check", menuName = "Diluvion/Achievement/QuestAchievementCheck")]
    public class DsaveQuestCheck : DSaveAchievement
    {
        [SerializeField] private List<DQuest> quests = new List<DQuest>();
        
        public override int Progress(DiluvionSaveData dsd)
        {
            base.Progress(dsd);

            foreach (DQuestSave s in dsd.questSaves)
            {
                //Debug.Log(" has " + s.key + ",");
                if (!s.complete) continue;

                foreach (DQuest dq in quests)
                {
                    //Debug.Log(" comparing to: " + dq.name + ".");
                    if (s.key == dq.name)
                        progress++;
                }
                  
            }
            return progress;
        }
    }
}
