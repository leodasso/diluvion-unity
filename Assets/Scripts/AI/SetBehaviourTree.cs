using NodeCanvas.Framework;
using ParadoxNotion.Design;
using NodeCanvas.BehaviourTrees;

namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Sets a new mission.")]
	public class SetBehaviourTree : ActionTask<AIMono>{

        public BBParameter<BehaviourTree> tree;
        public BBParameter<BehaviourTree> target;


        protected override string info
        {
            get { return "Set BT "  + tree + " to " + target; }
        }

        protected override string OnInit()
        {
            if (tree.isNull) EndAction(false);
        
            return null;
        }


        protected override void OnExecute()
        {
           // target.value.OnFinish += OnStopped;
            if(target.isNull)
                target.value.Stop();
            target.value = tree.value;
            //agent.ResetMission();
            EndAction(true);
        }
    }
}