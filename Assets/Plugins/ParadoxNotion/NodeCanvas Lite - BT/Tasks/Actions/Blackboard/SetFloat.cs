using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;


namespace NodeCanvas.Tasks.Actions{

	[Category("✫ Blackboard")]
	[Description("Set a blackboard float variable")]
	public class SetFloat : ActionTask{

		[BlackboardOnly]
		public BBParameter<float> valueA;
		public OperationMethod Operation = OperationMethod.Set;
		public BBParameter<float> valueB;
		public bool perSecond;

		protected override string info{
			get	{return valueA + OperationTools.GetOperationString(Operation) + valueB;}
		}

		protected override void OnExecute(){
			valueA.value = OperationTools.Operate(valueA.value, valueB.value, Operation, perSecond? UnityEngine.Time.deltaTime : 1f);
			EndAction(true);
		}
	}
}