using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Determines if the input transform is valid for this AI to target")]
	public class ValidTarget : ConditionTask<AIMono>
    {

        public BBParameter<Transform> targetTrans;

        public BBParameter<ContextTarget> currentTopTarget; // if this is no longer my top priority, its no longer valid




		protected override string OnInit(){
			return null;
		}

		protected override bool OnCheck()
        {
            if (targetTrans.isNull) return false;
            if (targetTrans.value == null) return false;
            if (!targetTrans.value.gameObject.activeInHierarchy) return false;
            if (!currentTopTarget.isNull)
                if (currentTopTarget.value.target.transform != targetTrans.value) return false;

			return true;
		}
	}
}