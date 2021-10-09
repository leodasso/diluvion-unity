using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Returns a world position of a waypoint closest to input position.")]
	public class GetClosestWaypoint : ActionTask<AIMono>
	{
		public BBParameter<Vector3> pointToCheck;
		public BBParameter<Vector3> waypointPos;

		protected override void OnExecute()
		{
			EndAction(true);
		}

	}
}