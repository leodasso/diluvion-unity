using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Sets a direciton point to move towards in relation to a target")]
	public class SetRelativeDirectionTo : ActionTask<AIMono>
	{
		public BBParameter<Vector3> target;
		public BBParameter<Vector3> relativeDirection;
		public BBParameter<float> distancetoMove;
		public BBParameter<Vector3> worldPoint;
		public BBParameter<bool> getClosestWP = false;
		
		
		protected override string OnInit(){
			return null;
		}

		protected override void OnExecute()
		{
			Vector3 directionToTarget = target.value - agent.transform.position;

			Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

			Vector3 relDirDistance = relativeDirection.value * distancetoMove.value;

			Vector3 positionToGoTo = targetRotation * relDirDistance;
			
			Debug.DrawLine(agent.transform.position, worldPoint.value, Color.magenta, 5);
			
			
		
			//Debug.Log("Ref for point: BBParameter: " + worldPoint + " varRef: " + worldPoint.varRef + " varType: " + worldPoint.varType.ToString() + " varRef value: " +  worldPoint.varRef.value.ToString());
			
			blackboard.RemoveVariable(worldPoint.name);
			blackboard.AddVariable(worldPoint.name, typeof(Vector3));

			//worldPoint.value = positionToGoTo;
			Vector3 worldPointToGoTo;
			if (getClosestWP.value)
            {
                PathMono pm = NavigationManager.Get().ClosestLOSPathMonoToPosition(positionToGoTo, positionToGoTo);
                if (pm != null)
                    worldPointToGoTo = pm.transform.position;
                else
                    worldPointToGoTo = positionToGoTo;
            }
		    else
				worldPointToGoTo = positionToGoTo;
			
			blackboard.SetValue(worldPoint.name, worldPointToGoTo);
			worldPoint.PromoteToVariable(blackboard);
			
			EndAction(true);
		}

	}
}