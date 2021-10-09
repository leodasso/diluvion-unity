using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using System.Collections.Generic;
namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Finds x amount of legal navpoints")]
	public class FindCloseNavPoints : ActionTask<AIMono> 
	{
		public BBParameter<int> amountOfPointsToGet;
		public BBParameter<List<Transform>> foundNavPoints;
	
	
		protected override string OnInit()
		{
			return null;
		}
	
		protected override void OnExecute()
		{
			foundNavPoints.value = ConvertToTransforms(NavigationManager.Get()
				.ClosestLOSPathMonos(agent.transform.position, agent.transform.position, amountOfPointsToGet.value));
			EndAction(true);
		}
	
		List<Transform> ConvertToTransforms(List<PathMono> components) 
		{
			List<Transform> transforms = new List<Transform>();
	
			foreach (PathMono t in components)
			{
				transforms.Add(t.transform);
			}
			return transforms;
		}
	}
}