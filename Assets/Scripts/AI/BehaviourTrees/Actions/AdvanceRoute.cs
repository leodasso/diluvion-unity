using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Diluvion.AI{

    public enum RouteAdvance
    {
        Forward,
        Backward
    }

	[Category("Diluvion")]
	[Description("Moves the input route ahead.")]
	public class AdvanceRoute : ActionTask<AIMono>{

        public BBParameter<RouteAdvance> routeMode;
        public BBParameter<Route> routeToAdvance;
        public BBParameter<Vector3> newWaypoint;
       
		
		protected override void OnExecute()
        {
            switch(routeMode.value)
            {
                case RouteAdvance.Forward:
                {
                    newWaypoint. value =routeToAdvance.value.MoveNext();
                    break;
                }
                case RouteAdvance.Backward:
                {
                    newWaypoint.value = routeToAdvance.value.MovePrevious();
                    break;
                }
            }

          

            if (routeToAdvance.value.Finished())
            {
                routeToAdvance.value.wpInd = 0;
                EndAction(true);
            }
            else
            {
                EndAction(false);
            }
            //Debug.Log("ADVANCING ROUTE TO:" + routeToAdvance.value.wpInd);
		}
	
	}
}