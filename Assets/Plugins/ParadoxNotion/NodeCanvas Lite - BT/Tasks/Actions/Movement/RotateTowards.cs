using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions{

	[Category("Movement/Direct")]
	[Description("Rotate the agent towards the target per frame")]
	public class RotateTowards : ActionTask<Transform> {

		[RequiredField]
		public BBParameter<GameObject> target;
		public BBParameter<float> speed = 2;
		[SliderField(1, 180)]
		public BBParameter<float> angleDifference = 5;
		public bool waitActionFinish;

		protected override void OnUpdate(){
			if (Vector3.Angle(target.value.transform.position - agent.position, agent.forward) <= angleDifference.value){
				EndAction();
				return;
			}

			var dir = target.value.transform.position - agent.position;
			agent.rotation = Quaternion.LookRotation(Vector3.RotateTowards(agent.forward, dir, speed.value * Time.deltaTime, 0));
			if (!waitActionFinish){
				EndAction();
			}
		}
	}
}