using UnityEngine;
using Diluvion;
using Sirenix.OdinInspector;

namespace Quests
{

    /// <summary>
    /// A component that can go on any 3D world actor. When the selected quest is active, this component will
    /// be added to a list of active quests in quest manager. That way, the currently selected quest knows where and which
    /// waypoints to show in the GUI, and via the guiding fish.
    /// </summary>
    public class QuestWaypoint : QuestActor
    {

        [InfoBox("When the selected quest & objective is active, this is added to a list of active quests. " +
                 "The player can then select from that list a quest to display via GUI & guiding fish.")]
        [ReadOnly]
        public float bert;
        
        protected override void OnActivate()
        {
            base.OnActivate();
            QuestManager.AddWaypoint(this);
            SelectWaypoint();
        }

        public override void SelectWaypoint()
        {
            base.SelectWaypoint();
            QuestManager.SetMainWaypoint(this);
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            QuestManager.RemoveWaypoint(this);
        }

        public override string ToString()
        {
            string s = "Turns on waypoint if and only " + base.ToString();
            return s;
        }
    }
}