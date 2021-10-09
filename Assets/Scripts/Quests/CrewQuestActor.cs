using Quests;
using DUI;
using UnityEngine;

namespace Diluvion
{

    /// <summary>
    /// Add this component to a crew to have them show a little UI 'hey click me!' type of notification.
    /// Useful for quest sensetive stuff where this person is important for a certain part of a quest.
    /// </summary>
    [AddComponentMenu("DQuest/2D Quest waypoint")]
    public class CrewQuestActor : QuestActor
    {

        DUIInteriorQuestWaypoint waypoint;

        protected override void OnActivate ()
        {
            base.OnActivate();
            waypoint = DUIInteriorQuestWaypoint.Create(gameObject, this);
        }

        protected override void OnDeactivate ()
        {
            base.OnDeactivate();
            if (waypoint) waypoint.End();
        }
    }
}