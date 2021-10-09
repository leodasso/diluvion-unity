using NodeCanvas.Framework;
using ParadoxNotion.Design;
using Diluvion.Sonar;
using System.Collections.Generic;

namespace Diluvion.AI{

	[Category("Diluvion/Sensor")]
	[Description("Gets sensor information from the AI")]
	public class AISensor : ActionTask<AIMono>{

        public BBParameter<List<SonarStats>> currentTargets;

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