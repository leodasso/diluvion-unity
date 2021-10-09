using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using Diluvion.AI;

namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Check for obstacles ahead.")]
    ///returns true if no collision, red if there is one
	public class AIAvoidance : ActionTask<AIMono>{


        public BBParameter<Vector3> directionToCheck;
        public BBParameter<Vector3> avoidDirection;
        public BBParameter<float> crashDistance;

        protected override string OnInit(){
			return null;
		}

		protected override void OnExecute()
        {         
            float distance = 0;
            Vector3 avoidVector = Vector3.zero;
         

            avoidDirection.value = avoidVector;
            crashDistance.value = 1-distance;//Invert the normalized crash distance

            EndAction(false);
		}

		protected override void OnUpdate()
        {
			
		}

		protected override void OnStop(){
			
		}

		protected override void OnPause(){
			
		}
	}
}