using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Hard Moves the agent towards its target")]
	public class UnstuckMove : ActionTask<Rigidbody>
	{

		public BBParameter<Vector3> targetPosition;
		public BBParameter<float> moveSpeed = new BBParameter<float>(2);
		public BBParameter<float> moveAttemptTime = new BBParameter<float>(3);

		private float attemptTime = 0;
		protected override void OnExecute()
		{
			attemptTime = 0;
		}

		protected override void OnUpdate()
		{
			Vector3 targetRelation = targetPosition.value - agent.transform.position;
			
			agent.AddForce(targetRelation.normalized*moveSpeed.value, ForceMode.Acceleration);

			if (attemptTime < moveAttemptTime.value)
				attemptTime = Time.deltaTime;
			else
			{
				agent.transform.position = targetPosition.value;
				attemptTime = 0;
			}
		}
	}
}