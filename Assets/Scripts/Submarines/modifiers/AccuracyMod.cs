using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Diluvion.Ships
{
	[CreateAssetMenu(fileName = "accuracy mod", menuName = "Diluvion/subs/mods/accuracy")]
	public class AccuracyMod : ShipModifier
	{
		public List<WeaponModule> affectedModules = new List<WeaponModule>();
		
		public override void Modify(Bridge bridge, float value)
		{
			foreach (WeaponSystem ws in bridge.GetComponents<WeaponSystem>())
			{
				if (affectedModules.Contains(ws.module))
					ws.SetAccuracy(value);
			}
		}

		protected override string Test()
		{
			string s = base.Test();
			s += "This would set accuracy for ";
			foreach (WeaponModule m in affectedModules) s += m.name + " ";
			s += "to " + TestingValue();
			return s;
		}
	}
}