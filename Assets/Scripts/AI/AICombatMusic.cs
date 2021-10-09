using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Diluvion.AI{

	[Category("DiluvionAI")]
	[Description("Adds Combatant To Combat Music")]
	public class AICombatMusic : ActionTask<AIMono>
	{
		public BBParameter<ContextTarget> target;
		public BBParameter<bool> add = true;
		
		protected override void OnExecute()
		{	
			if (target.isNull||target.value.target!=null&&target.value.target.transform != PlayerManager.PlayerTransform()){ EndAction(true);
				return;
			}
			
			//Debug.Log("Changing combat state : " + add.value);
			if (add.value) CombatMusic.AddCombatant(agent.gameObject);
				//CombatZone.Get().AddToCombat(agent.gameObject);
			else CombatMusic.RemoveCombatant(agent.gameObject);
			//{
				//if(CombatZone.Exists())
					//CombatZone.Get().RemoveFromCombat(agent.gameObject);
			//}
				
			
			EndAction(true);
		}
	}
}