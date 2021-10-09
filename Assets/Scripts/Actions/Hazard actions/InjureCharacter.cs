using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DUI;

namespace Diluvion
{

	[CreateAssetMenu(fileName = "injure character", menuName = "Diluvion/actions/injure char")]
	public class InjureCharacter : HazardAction
	{
		[MultiLineProperty()]
		public string injuredText = "[char] is injured and must retreat to the ship!";

		public override bool DoAction(Object o)
		{
			AttackSailor(Hazard.sailorToAttack);
			return true;
		}

		void AttackSailor(Sailor sailor)
		{
			if (!sailor)
			{
				Debug.LogError("Can't attack a null sailor!", this);
				return;
			}
			
			sailor.Injure();
		}

		public override void DoAttack(Hazard hazard)
		{
			// Log this attack
			string crewName = Hazard.sailorToAttack.GetLocalizedName();
			BattleLog attackLog = new BattleLog(hazard.LocAttack(), crewName, hazard.LocName(), 5);
			DoAction(null);
			BattlePanel.Log(attackLog);
			BattlePanel.Shake(2, 1);
			
			// log what happens to injured characters // TODO loc
			BattleLog injury = new BattleLog(injuredText, crewName, t: 5);
			injury.onEnd += BattlePanel.Iterate;
			BattlePanel.Log(injury);
		}

		public override string ToString()
		{
			return "Attacks the selected member of the boarding party.";
		}
	}
}