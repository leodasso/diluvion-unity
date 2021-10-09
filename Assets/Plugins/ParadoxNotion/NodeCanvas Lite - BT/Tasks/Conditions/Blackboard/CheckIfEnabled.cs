using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions{

	[Category("✫ Blackboard")]
	[Description("It's best to use the respective Condition for a type if existant since they support operations as well")]
	public class IsActiveInHiearchy: ConditionTask {

		[BlackboardOnly]
		public BBParameter<Component> valueA;
	

		protected override string info => valueA + " is enabled?";

		protected override bool OnCheck()
		{
			if (valueA.isNull) return false;
			return valueA.value.gameObject.activeInHierarchy;
		}
	}
}