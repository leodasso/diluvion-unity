using Cinemachine;
using Diluvion.SaveLoad;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Diluvion{

	[Category("Diluvion")]
	[Description("Gets the position of the input transform")]
	public class GetPosition : ActionTask<Transform>
	{
		[BlackboardOnly]
		public BBParameter<Vector3> position;

		protected override string info
		{
			get
			{
				if (!position.isNull)
					return "Save agent position as : " + position.name;
				else
				{
					return "Saves the agent position";
				}
				
			}
		}

		
		protected override void OnExecute()
		{
			position.value = agent.position;
			EndAction(true);
		}

	}
}