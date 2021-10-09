using Diluvion;
using UnityEngine;
using Diluvion.Ships;
using Sirenix.OdinInspector;

namespace Quests
{

    /// <summary>
    /// A type of trigger that can trigger progress on an quest objective.
    /// </summary>
    [AddComponentMenu("DQuest/trigger quest progress")]
    public class QuestTrigger : Trigger
    {
        [InfoBox("This component can trigger progress on an objective / quest combo. Both fields must be filled for it to work.")]
        [AssetsOnly, PropertyOrder(-999)]
        public DQuest quest;
        
        [AssetList(CustomFilterMethod = "QuestHasObjective", AutoPopulate = false), AssetsOnly, PropertyOrder(-998)]
        public Objective objectiveToProgress;

        bool QuestHasObjective(Objective o)
        {
            if (!quest) return false;
            return quest.HasObjective(o);
        }

        protected override void Start()
        {
            base.Start();
            QuestManager.Tick();
        }

        public override void TriggerAction(Bridge otherBridge)
        {
            base.TriggerAction(otherBridge);
            objectiveToProgress.ProgressObjective(quest);
        }
    }
}
