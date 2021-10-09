using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Gets a escort object on the target, or creates one if it's missing")]
	public class GetEscortPoint : ActionTask<AIMono>
	{
		public BBParameter<Transform> target;

		public BBParameter<Vector3> escortPoint;

		private Escortee ee;

		private Escortee EE
		{
			get
			{
				if (ee != null) return ee;
				if (target.isNull) return null;
				ee = target.value.GetComponentInChildren<Escortee>();
				return ee;
			}
		}


		protected override void OnExecute()
		{
			if (EE == null)
			{
				EndAction(false);
				return;
			}
			escortPoint.value = EE.WorldEscortPoint(agent.transform);
			EndAction(true);
		}

	}
}