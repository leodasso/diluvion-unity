using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Diluvion.AI{

	[Category("Blackboard")]
	[Description("Get Component of Type")]
	public class GetComponentFromComponent<T> : ActionTask where T : Component{

        public BBParameter<Component> inputComponent;
        public BBParameter<T> output;


		protected override void OnExecute()
        {
            if (inputComponent.isNull||inputComponent.value==null || inputComponent.value.transform == null)
            {
                EndAction(false);
                return;
            }
            output.value = inputComponent.value.transform.GetComponent<T>();         
            EndAction(!output.isNull);
		}

	}
}