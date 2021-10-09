using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System.Collections.Generic;

namespace Diluvion.Sonar{

	[Category("Diluvion")]
	[Description("Searches the attached ship for a relevant target, returns true if it found one, false if it did not")]
	public class FindListenerTarget : ActionTask<Listener>
    {
      
        public BBParameter<List<Signature>> targetTags;
        public BBParameter<SonarStats> target;

        Listener listener;
        Listener Listener()
        {
            if (listener != null) return listener;
            listener = agent.GetComponent<Listener>();
            if (listener == null)
                listener = agent.gameObject.AddComponent<Listener>();
            return listener;
        }

		protected override string OnInit(){
			return null;
		}

		protected override void OnExecute()
        {
            target.value = Listener().ClosestSigWithTags(targetTags.value);
            if (!target.isNull)
                EndAction(true);
            else
                EndAction(false);
		}

		protected override void OnUpdate(){
			
		}

		protected override void OnStop(){
			
		}

		protected override void OnPause(){
			
		}
	}
}