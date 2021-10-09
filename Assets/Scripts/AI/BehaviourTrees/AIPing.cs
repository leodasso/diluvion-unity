using NodeCanvas.Framework;
using ParadoxNotion.Design;


namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Creates a sonarPing")]
	public class AIPing : ActionTask<AIMono>{

        public BBParameter<float> charge = 0.5f;

		protected override string OnInit(){
			return null;
		}

		protected override void OnExecute()
        {
            if (agent.MyPinger == null) { EndAction(false); return; }
            agent.MyPinger.Ping(charge.value);
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