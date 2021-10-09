using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Check if agent can see point")]
	public class PointInLOS : ConditionTask<AIMono>
	{
		public BBParameter<Vector3> pointToCheck;
		public BBParameter<LayerMask> mask;


		protected override string info => "Can i see (V3) " + pointToCheck.name;

		protected override string OnInit(){
			return null;
		}

		protected override bool OnCheck()
		{
			Vector3 rayToPoint = pointToCheck.value - agent.transform.position;
			float distance = Vector3.Distance(pointToCheck.value, agent.transform.position);
			Ray lOsRay = new Ray(agent.transform.position,rayToPoint);
			if(!Physics.Raycast(lOsRay, distance, mask.value))
			{
				return true;
			}
			return false;
		}
	}
}