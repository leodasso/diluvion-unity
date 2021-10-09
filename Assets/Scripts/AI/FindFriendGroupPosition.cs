using NodeCanvas.Framework;
using ParadoxNotion.Design;


namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Returns a place in the friend formation")]
	public class FindFriendGroupPosition : ActionTask<AIMono>{

		protected override string OnInit(){
			return null;
		}

		protected override void OnExecute(){
			EndAction(true);
		}

		protected override void OnUpdate(){
			
		}

		protected override void OnStop(){
			
		}

		protected override void OnPause(){
			
		}
	}
}