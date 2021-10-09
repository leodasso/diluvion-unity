using NodeCanvas.Framework;
using ParadoxNotion.Design;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{

	[Category("✫ Blackboard")]
	[Description("Target pos - Agent pos")]
	public class DirectionTo : ActionTask<Transform>{      
       
        public BBParameter<Vector3> targetPos;
        public BBParameter<Vector3> direction;
      
             
        protected override string OnInit(){
			return null;
		}

		protected override string info => "Direction to " + targetPos.name;

		protected override void OnExecute()
        {  
            direction.value = (targetPos.value - agent.position);
			//Debug.DrawRay(agent.position, direction.value, Color.yellow, 0.1f);
            EndAction(true);
        }		
	}
}