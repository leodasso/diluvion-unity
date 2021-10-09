using Diluvion.Ships;
using Galaxy;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Get's the target's Throttle if it has one")]
	public class GetTargetThrottle : ActionTask<AIMono>
	{
		public BBParameter<Transform> target;
		public BBParameter<ThrottleRequest> gotThrottle;
		
		protected override string OnInit(){
			return null;
		}

		protected override void OnExecute()
		{
			if (target.isNull) return;

			ShipMover sm = target.value.GetComponent<ShipMover>();

			if (sm == null) return;

			gotThrottle.value = new ThrottleRequest(sm.throttle);
			
			EndAction(true);
		}

		protected override void OnUpdate(){
			
		}

		protected override void OnStop(){
			
		}

		protected override void OnPause(){
			
		}
	}
}