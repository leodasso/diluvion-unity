using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Diluvion
{

	[CreateAssetMenu(fileName = "crew change action", menuName = "Diluvion/actions/crew change action")]
	public class CrewChangeAction : Action
	{
		public CharacterInfo character;
				
		public override bool DoAction(Object o)
		{
			if (PlayerManager.PlayerCrew() == null)
			{
				Debug.LogError("No player crew manager found!");
				return false;
			}
			PlayerManager.PlayerCrew().KillCrew(character, false);
			return true;
		}

		public override string ToString()
		{
			return "Removes " + character.name + " from player crew.";
		}

		protected override void Test()
		{
			DoAction(null);
		}
	}
}