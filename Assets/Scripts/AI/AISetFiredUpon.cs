using System;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Diluvion.AI{

	[Category("DiluvionAI")]
	[Description("Sets the Combat state to Fired Upon for player")]
	public class AISetFiredUpon : ActionTask<AIMono>
	{


		public BBParameter<Transform> target;
	
		
		
		protected override string OnInit(){
			return null;
		}

		protected override void OnExecute()
		{
			if (PlayerManager.PlayerTransform() == target.value)
				CombatMusic.ShotsFired(agent.gameObject.transform.position);
				/*
			{
				CombatZone.Get().SetPlayerFiredUpon();
			}
			*/
			EndAction(true);
		}
	}
}