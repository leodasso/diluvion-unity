using System.IO;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Sets a point relative to the agent")]
	public class AISetRelativePoint : ActionTask<AIMono>
	{
		[Tooltip("DepthChange as a ")]
		public BBParameter<float> depthChange = 0;
		public BBParameter<float> directionAngle = 0;
		public BBParameter<float> distance = 25;
		public BBParameter<bool> flatForward = false;
		public BBParameter<Vector3> targetToMoveTo;

		public BBParameter<Vector3> outPutPoint;
		

		protected override void OnExecute()
		{

			Vector3 forward;
			if (targetToMoveTo.value == Vector3.zero)
			{
				forward = agent.transform.forward;
			}
			else
			{
				forward = (targetToMoveTo.value - agent.transform.position).normalized;
			}
			
			if (flatForward.value)
				forward = new Vector3(forward.x,0,forward.z).normalized;
			
			
			Vector3 angleDir = (Quaternion.AngleAxis(directionAngle.value, Vector3.up)*forward);
			
			Debug.DrawRay(agent.transform.position,Quaternion.AngleAxis(0, Vector3.up)*forward*distance.value, Color.blue, 4);
			Debug.DrawRay(agent.transform.position,Quaternion.AngleAxis(90, Vector3.up)*forward*distance.value, Color.yellow, 4);
			Debug.DrawRay(agent.transform.position,Quaternion.AngleAxis(-90, Vector3.up)*forward*distance.value, Color.magenta, 4);
			
			angleDir.y += depthChange.value;

			float finalDistance = distance.value;
			RaycastHit hit;
			Ray ray = new Ray(agent.transform.position, angleDir);
			
			if (Physics.Raycast(ray, out hit, distance.value, LayerMask.GetMask("Terrain")))
				finalDistance = Mathf.Max(hit.distance/2, hit.distance-10); //Get the larger distance of these two, we want to favor the point closest to the impact as the point to go to
			
			
			angleDir *= finalDistance;

			Vector3 finalPoint = (agent.transform.position +  angleDir);
			
			
			
			//Workaround for setting a BBParamter<Vector3> that was previously a transform, back to a vector3
			////blackboard.RemoveVariable(outPutPoint.name);
		//	blackboard.AddVariable(outPutPoint.name, typeof(Vector3));
		//	/blackboard.SetValue(outPutPoint.name, finalPoint);
			//outPutPoint.PromoteToVariable(blackboard);
			
			outPutPoint.value = finalPoint; //Causes Teleport
		
			Debug.DrawLine(agent.transform.position+Vector3.one,outPutPoint.value+Vector3.one, Color.red, 2);
			Debug.DrawLine(agent.transform.position,finalPoint, Color.green, 2);
			
			EndAction(true);
		}

	}
}