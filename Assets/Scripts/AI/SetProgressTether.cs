using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Returns the percentage of completion as well as the current progress point between two vectors")]
	public class SetProgressTether : ActionTask<AIMono>
	{

		
		public BBParameter<Route> route;

	
		public BBParameter<float> progress;
		public BBParameter<Vector3> tetherPoint;
		
		
		protected override string OnInit()
		{
			return null;
		}

		protected override void OnExecute()
		{

			progress.value = WaypointProgress(route.value);
			tetherPoint.value = ProgressPoint(route.value);
			
			EndAction(true);
		}
		
		
		float prog = 0;
		float WaypointProgress(Route wp)
		{
			Vector3 a = agent.transform.position - wp.PreviousWP();

			Vector3 b = wp.CurrentWP() - wp.PreviousWP();

			if (b == Vector3.zero || a == Vector3.zero)
				return 1;

			//Debug.Log("Index: " + wp.wpInd + " A: " + a +"(" +transform.position  + "-"+wp.PreviousWP()+ ")"+ " B: " + b + "(" + wp.CurrentWP() + "-" + wp.PreviousWP() + ")");
          
			//To know if we have passed the upcoming waypoint we need to find out how much of b is a1
			//a1 = (a.b / |b|^2) * b
			//a1 = progress * b -> progress = a1 / b -> progress = (a.b / |b|^2)
			prog = (a.x * b.x + a.y * b.y + a.z * b.z) / (b.x * b.x + b.y * b.y + b.z * b.z);

			return prog;
		}

		/// <summary>
		/// Records the linear progress on a waypoint from its previous waypoint
		/// </summary>
		/// <param name="wp"></param>
		/// <returns></returns>
		
		Vector3 ProgressPoint(Route wp)
		{
			Vector3 waypointLine = route.value.CurrentWP() - route.value.PreviousWP();
			
			if (waypointLine == Vector3.zero)
			{
				return wp.CurrentWP();
			}

		//	float offsetDistance = waypointLine.magnitude * progress.value + tetherOffset.value;
			Vector3 offsetVector = waypointLine.normalized;

			return route.value.PreviousWP() + offsetVector;
		}

	}
}