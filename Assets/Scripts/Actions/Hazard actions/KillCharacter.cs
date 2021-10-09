using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DUI;
using Sirenix.OdinInspector;

namespace Diluvion
{
	[CreateAssetMenu(fileName = "kill character", menuName = "Diluvion/actions/kill char")]
	public class KillCharacter : HazardAction
	{
		public LocTerm deathText;
		public LocTerm warningText;
		
		[Range(1, 9)]
		[OnValueChanged("CalculateDanger")]
		public int deathTime;

		static int _deathTimer;

		public override bool DoAction(Object o)
		{
			if (_deathTimer < 0)
			{
				KillTheCharacter(o);
				return true;
			}
			
			string warning = "........" + _deathTimer + ".........";
			
			if (_deathTimer == 0) warning += warningText.LocalizedText();
			
			BattleLog doomLog = new BattleLog(warning);
			doomLog.onEnd += BattlePanel.Iterate;
			BattlePanel.Log(doomLog);

			_deathTimer--;

			return true;
		}

		bool KillTheCharacter(Object o)
		{
			if (!(o is Sailor))
			{
				Debug.LogError("Attempting to kill " + o.name + " but it's not a type of sailor!");
				return false;
			}
			
			Sailor s = o as Sailor;
			Debug.Log("Killing " + s.name);
			s.Die(true, false, true);

			BattlePanel.ForcedRetreat();
			return true;
		}

		public override void DoAttack(Hazard hazard)
		{
			// Log this attack
			Sailor attackSubject = Hazard.subject;
			if (!Hazard.subject)
				attackSubject = Hazard.sailorToAttack;
			
			string crewName = attackSubject.GetLocalizedName();
			
			BattleLog attackLog = new BattleLog(hazard.LocAttack(), crewName, hazard.LocName(), 5);
			DoAction(attackSubject);
			BattlePanel.Log(attackLog);
			BattlePanel.Shake(2, 1);
			
			// log what happens to injured characters // TODO loc
			BattleLog injury = new BattleLog(deathText.LocalizedText(), crewName, t: 5);
			injury.onEnd += BattlePanel.Iterate;
			
			BattlePanel.Log(injury);
		}

		public override void BattleBeginPrep()
		{
			base.BattleBeginPrep();
			_deathTimer = deathTime;
		}
	}
}