using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System.Collections.Generic;
using UnityEngine;

namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Finds the closest X explorables")]
	public class GetClosestExplorables : ActionTask<AIMono>
	{
		public BBParameter<Transform> transformToCheckAgainst;
		public BBParameter<int> returnCount = 10;
		public BBParameter<List<ExplorablePlacer>> returnList;

		Transform TargetTransform
		{
			get
			{
				if (!transformToCheckAgainst.isNull) return transformToCheckAgainst.value;
				return transformToCheckAgainst.value = agent.transform;
			}
		}

		protected override void OnExecute()
		{
			returnList.value = ExplorablePlacer.ClosestPlacersTo(TargetTransform, returnCount.value);
			EndAction(true);
		}

	}
}