using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Callback for undocking with this AI")]
    [EventReceiver("Undocked")]
    public class OnUndock : ConditionTask<AIMono>
    {
        public BBParameter<ContextTarget> undocker;

        protected override string OnInit()
        {
            return null;
        }

        protected override bool OnCheck()
        {
            return false;
        }

        public void Undocked(ContextTarget ct)
        {
            undocker = ct;
            YieldReturn(true);
        }    
    }
}