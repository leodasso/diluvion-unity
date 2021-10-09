using NodeCanvas.Framework;
using ParadoxNotion.Design;
using Diluvion.Ships;
using UnityEngine;

namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Turns the ship towards the input global point.")]
	public class AISteerAction : ActionTask<ShipMover>{


        public BBParameter<Vector3> direction;
        public BBParameter<Vector3> avoidVector;
        public float clampedUpDown = 0.1f; 

        protected override string OnInit(){
			return null;
		}

		protected override void OnExecute(){
            Go();
        }
      

        protected override void OnUpdate()
        {
            Go();
        }

        void Go()
        {          
            Vector3 compositeDir = direction.value.normalized + avoidVector.value.normalized;
            agent.ShipManeuver(compositeDir.normalized, clampedUpDown);

            float upDownDot = compositeDir.y; // Some kiond of up/down value for up/dpwn
            agent.ChangeBallast(upDownDot);        
        }    
	}
}