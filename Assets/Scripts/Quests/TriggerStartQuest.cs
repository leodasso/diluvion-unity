using UnityEngine;
using System.Collections.Generic;
using Diluvion;
using Diluvion.Ships;

namespace Quests
{
    [AddComponentMenu("DQuest/trigger quest start")]
    public class TriggerStartQuest : Trigger
    {
        public DQuest quest;

        protected override void Start()
        {
            base.Start();
            QuestManager.Tick();
        }

        public override void TriggerAction(Bridge otherBridge)
        {
            base.TriggerAction(otherBridge);
            quest.StartQuest();
        }
    }
}
