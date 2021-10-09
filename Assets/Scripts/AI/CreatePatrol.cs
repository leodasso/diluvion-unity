using NodeCanvas.Framework;
using ParadoxNotion.Design;
using Diluvion.Sonar;
using System.Collections.Generic;

using System.Runtime.InteropServices;
using System.Linq;
using UnityEngine;

namespace Diluvion.AI{

	public enum Direction
	{
		ClockWise,
		CounterClockwise,
		Forward
	}
	
	[Category("Diluvion")]
	[Description("Creates a patrol out of a list of points, including the start point.")]
	public class CreatePatrol : ActionTask<SonarStats>
	{
		public Direction direction;
		
		public BBParameter<List<SonarStats>> targetPoints;
		
		private List<SonarStats> sortedPoints;

		public BBParameter<List<SonarStats>> patrolStats;

		public List<Vector3> patrolPoints;
		public BBParameter<Route> target;
		public BBParameter<Vector3> firstPoint;

		private Direction realDirection;

		protected override string OnInit()
		{
			return null;
		}

        /// <summary>
        /// Get the Circuluar direction in relation to the center of the patrol
        /// </summary>
        /// <param name="centerPoint"></param>
        /// <returns></returns>
		Direction FacingDirection(Vector3 centerPoint)
		{

			Vector3 centerDirection = centerPoint - agent.transform.position;
            Debug.DrawLine(agent.transform.position, agent.transform.position + centerDirection, Color.blue, 5);

			Vector3 relativeRight = Vector3.Cross(Vector3.up, centerDirection.normalized);
            Debug.DrawRay(agent.transform.position+Vector3.up, relativeRight*70, Color.red, 5);

            float rightDot = Vector3.Dot(relativeRight.normalized, agent.transform.forward.normalized);

            Debug.Log("Right Dot is: " + rightDot);
			if (rightDot <= 0)
				return Direction.ClockWise;
			else
				return Direction.CounterClockwise;
		}

        /// <summary>
        /// Organize the points in a circular fashion, beginning with the first point in front and ending with the closest point behind
        /// </summary>
		void OrderPoints()
		{
			Vector3 center = Vector3.zero;

			patrolStats.value = new List<SonarStats>();
			patrolPoints = new List<Vector3>();
			
			foreach (SonarStats ss in targetPoints.value)
			{
				center += ss.transform.position;
			}

			center /= targetPoints.value.Count;

			//Enter left Or Right comparison here

			if (direction == Direction.Forward)
				realDirection = FacingDirection(center);
			else
				realDirection = direction;

            
			patrolStats.value.Add(agent);

			for (int i = 0; i < patrolStats.value.Count; i++)
			{
				SonarStats a = patrolStats.value[i];

				//Debug.DrawLine(a.position, center, Color.yellow, 1);
				//	new List<SonarStats>(targetPoints.value.OrderBy(e => Vector3.Distance(e.transform.position, a.transform.position)).ToList());
				float closestDistance = 99999;
				SonarStats closestCandidate = null;
				
				foreach (SonarStats b in targetPoints.value)
				{
					if (patrolStats.value.Contains(b)) continue;
					Vector3 aPosition = a.transform.position;
					Vector3 bPosition = b.transform.position;
					
					Vector3 centerDirection = center - aPosition;
					Vector3 checkPointDirection = bPosition - aPosition;

					Debug.DrawRay(a.transform.position, checkPointDirection.normalized * 150, Color.white, 1);

					Vector3 relativeRight = Vector3.Cross(Vector3.up, centerDirection.normalized);

					float det = Vector3.Dot(relativeRight.normalized, checkPointDirection.normalized);
				//	Debug.Log("Checking B:" + b.name + " against A:" + a.name);
					if (det > 0 && realDirection == Direction.CounterClockwise)
					{
						
						float distance = Vector3.Distance(aPosition, bPosition);
					    //Debug.Log("Its CCW, " +distance+"/"+closestDistance+ " away!" );
						if(distance>closestDistance) continue;
						closestDistance = distance;
						closestCandidate = b;
						Debug.DrawRay(a.transform.position, relativeRight * distance, Color.green, 1);
					}
					if (det < 0 && realDirection == Direction.ClockWise) 
					{	
						float distance = Vector3.Distance(aPosition, bPosition);
						//Debug.Log("Its CW, " +distance+"/"+closestDistance+ " away!" );
						if(distance>closestDistance) continue;
						closestDistance = distance;
						closestCandidate = b;
						Debug.DrawRay(a.transform.position, -relativeRight * distance, Color.red, 1);
					}
				}
				
				//Debug.Log("Closest is: " +closestCandidate , closestCandidate );
				
				if(closestCandidate==null) continue;
				
				patrolStats.value.Add(closestCandidate);
				patrolPoints.Add(closestCandidate.transform.position);
			}
			
			patrolStats.value.Remove(patrolStats.value.First());
			
			target.value = new Route(patrolPoints);
			
			firstPoint.value = target.value.CurrentWP();}


		protected override void OnExecute()
		{
			if (!target.isNull)
			{
				if (!target.value.Finished())
				{
					
					EndAction(true);
					return;
				}
				
			}
			OrderPoints();
			EndAction(true);
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