using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Diluvion.Ships
{

	[CreateAssetMenu(menuName = "Diluvion/subs/mods/engineSpeed")]
	public class EnginePowerMod : ShipModifier
	{

		public override void Modify(Bridge bridge, float value)
		{
			Debug.Log(name + " is modifying. Value: " + value);
			ShipMover mover = bridge.GetComponent<ShipMover>();
			if (!mover) return;

			mover.SetExtraEnginePower(value);
		}
	}
}