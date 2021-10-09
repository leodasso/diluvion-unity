using NodeCanvas.Framework;
using ParadoxNotion.Design;

using UnityEngine;


namespace NodeCanvas.Tasks.Conditions{

	[Category("GameObject")]
	public class IsActive : ConditionTask<Transform>
	{


		public BBParameter<Transform> t;
		
		
		protected override bool OnCheck()
		{
			return !t.isNull ? t.value.gameObject.activeInHierarchy : agent.gameObject.activeInHierarchy;
		}
	}
}