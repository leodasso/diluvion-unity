using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Diluvion.Ships
{
	[CreateAssetMenu(fileName = "reload mod", menuName = "Diluvion/subs/mods/calibration speed")]
	public class CalibrationMod : ShipModifier
	{
		public List<WeaponModule> affectedModules = new List<WeaponModule>();
		
		public override void Modify(Bridge bridge, float value)
		{
			foreach (WeaponSystem ws in bridge.GetComponents<WeaponSystem>())
			{
				if (affectedModules.Contains(ws.module))
					ws.SetCalibrationSpeed(value);
			}
		}

		protected override string Test()
		{
			string s = base.Test();
			s += "This would set calibration speed for ";
			foreach (WeaponModule m in affectedModules) s += m.name + " ";
			s += "to " + TestingValue();
			return s;
		}
	}
}