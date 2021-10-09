using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Compares the input route end position and the input vector3 to see if they are within range")]
	public class CompareRouteEnd : ConditionTask{

        public BBParameter<Route> routeToCheck;
        public BBParameter<Vector3> comparePosition;
        public BBParameter<Vector3> offsetVector;
        public BBParameter<float> distance;
		
		#if UNITY_EDITOR
        [ReadOnly]
        #endif
        public float currentDistance;

		protected override string OnInit(){
			return null;
		}

		protected override bool OnCheck()
        {
            if (routeToCheck.isNull) return false;
            if (comparePosition.isNull) return false;
            if (routeToCheck.value.Destination() == Vector3.zero) return false;
            currentDistance = Vector3.Distance(comparePosition.value + offsetVector.value, routeToCheck.value.Destination());

            return currentDistance > distance.value;
        }
	}
}