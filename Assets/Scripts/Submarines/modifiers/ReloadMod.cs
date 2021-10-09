using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Diluvion.Ships
{
	[CreateAssetMenu(fileName = "reload mod", menuName = "Diluvion/subs/mods/reload speed")]
	public class ReloadMod : ShipModifier
	{
		public List<WeaponModule> affectedModules = new List<WeaponModule>();
		
		public override void Modify(Bridge bridge, float value)
		{
			foreach (WeaponSystem ws in bridge.GetComponents<WeaponSystem>())
			{
				if (affectedModules.Contains(ws.module))
					ws.SetReloadSpeed(value);
			}
		}

		protected override string Test()
		{
			string s = base.Test();
			s += "This would set reload speed for ";
			foreach (WeaponModule m in affectedModules) s += m.name + " ";
			s += "to " + TestingValue();
			return s;
		}
	}
}