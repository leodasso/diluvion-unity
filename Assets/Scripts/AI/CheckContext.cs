using NodeCanvas.Framework;
using ParadoxNotion.Design;


namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Queries the input Context for various values.")]
	public class CheckContext : ConditionTask{

        public BBParameter<ContextTarget> target;

		protected override string OnInit(){
			return null;
		}

		protected override bool OnCheck(){
			return true;
		}
	}
}