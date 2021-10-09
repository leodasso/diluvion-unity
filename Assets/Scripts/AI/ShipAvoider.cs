using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Returns true if course is clear.")]
	public class ShipAvoider : ConditionTask<AIMono>{

        [BlackboardOnly]
        public BBParameter<Vector3> checkDirection;
        [BlackboardOnly]
        public BBParameter<float> checkDistance;

        public BBParameter<Vector3> avoidVector;

        public BBParameter<float> crashDistance;

        public BBParameter<Avoiding> currentAvoidStatus;

		protected override string OnInit(){
			return null;
		}

        Vector3 aVector;
        float distance;
		protected override bool OnCheck()
        {
            currentAvoidStatus.value = agent.MyAvoider.NormalizedAvoidVector(checkDirection.value, checkDistance.value, out aVector, out distance);
            avoidVector.value = aVector;
            crashDistance.value = distance;
            return currentAvoidStatus.value == Avoiding.Nothing; 
		}
	}
}