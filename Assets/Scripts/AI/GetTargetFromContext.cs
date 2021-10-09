using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Gets a target from ContextTarget")]
	public class GetTargetFromContext : ActionTask
    {
        public BBParameter<ContextTarget> target;
        public BBParameter<Transform> targetPosition;

	   // public BBParameter<bool> chaseNaglFar;
	    public BBParameter<bool> chasePlayer;

		protected override void OnExecute()
        {
	        if (!chasePlayer.value)
	        {
				if (target.isNull)
				{
					EndAction(false);
					return;
				}
				if (!target.value.target)
				{
					EndAction(false);
					return;
				}
		        targetPosition.value = target.value.target.transform;
	        }
	        else
	        {
		        targetPosition.value = PlayerManager.PlayerTransform();
	        }
          
			EndAction(true);
		}
	}
}