using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Callback for undocking with this AI")]
    [EventReceiver("Docked")]
    public class OnDock : ConditionTask<AIMono>
    {
        public BBParameter<ContextTarget> docker;

        protected override string OnInit()
        {
            return null;
        }

        protected override bool OnCheck()
        {
            return false;
        }

        public void Docked(ContextTarget ct)
        {
            docker = ct;
            YieldReturn(true);
        }    
    }
}