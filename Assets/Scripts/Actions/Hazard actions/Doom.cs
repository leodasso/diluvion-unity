using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using DUI;

namespace Diluvion
{

	[CreateAssetMenu(fileName = "doom", menuName = "Diluvion/actions/doom")]
	public class Doom : HazardAction
	{		
		[Range(1, 9)]
		[OnValueChanged("CalculateDanger")]
		public int doomTime;

		[MultiLineProperty, LabelWidth(70)]
		public string doom;
		
		[MultiLineProperty, LabelWidth(100)]
		public string mildWarning;
		
		[MultiLineProperty, LabelWidth(100)]
		public string extremeWarning;
		
		static int _doomTimer;
		
		public override string ToString()
		{
			return "Injures the boarding party & destroys interior after " + doomTime + " turns.";
		}

		public override bool DoAction(Object o)
		{
			if (_doomTimer < 0)
			{
				DestroyExplorable();
				return true;
			}

			string warning = "........" + _doomTimer + ".........";
			
			if (_doomTimer == 0) warning += extremeWarning;
			
			else if (_doomTimer == 1) warning += mildWarning;
			
			BattleLog doomLog = new BattleLog(warning);
			doomLog.onEnd += BattlePanel.Iterate;
			BattlePanel.Log(doomLog);

			_doomTimer--;

			return true;
		}

		[Button]
		void DestroyExplorable()
		{
			// close the battle panel
			BattlePanel.Get().Flee();
			
			// Injure the entire boarding party
			List<Character> charactersToInjure = new List<Character>();
			charactersToInjure.AddRange(PlayerManager.BoardingParty());
			
			foreach (Character c in charactersToInjure)
			{
				Sailor s = c as Sailor;
				if (s) s.Injure();
			}
			
			// TODO popup to indicate what happened!
			
			PlayerManager.UndockPlayer();
		}

		public override void DoAttack(Hazard hazard)
		{
			BattleLog newLog = new BattleLog(hazard.LocAttack(), hazard: hazard.LocName(), t: 5);
			BattlePanel.Log(newLog);
			DoAction(null);
			BattlePanel.Shake(2, 1);
		}

		public override void BattleBeginPrep()
		{
			base.BattleBeginPrep();
			_doomTimer = doomTime;
		}

		protected override void Test()
		{
			DestroyExplorable();
		}
	}
}