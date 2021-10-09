using System.Collections;
using NodeCanvas.Framework;
using ParadoxNotion.Design;


namespace NodeCanvas.Tasks.Conditions{

	[Category("✫ Blackboard/Lists")]
	public class ListIsEmpty : ConditionTask {

		[BlackboardOnly]
		public BBParameter<IList> targetList;

		protected override string info{
			get
			{
				if (targetList.isNull) return "nullOrEmpty";
				return string.Format("{0} Is Empty", targetList);
			}
		}

		protected override bool OnCheck()
		{
			if (targetList.isNull) return true;
			if (targetList.isNone) return true;
			return targetList.value.Count == 0;
		}
	}
}