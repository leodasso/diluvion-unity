using System.Collections;
using System.Collections.Generic;
using Diluvion.Achievements;
using UnityEngine;

namespace Diluvion.Achievements
{
	/// <summary>
	/// Check object for zone progress.
	/// </summary>
	[CreateAssetMenu(fileName =  "discoveredZone", menuName = "Diluvion/Achievement/ZoneAchievementCheck")]
	public class DSaveDiscoveredZones : DSaveAchievement
	{
		public List<GameZone> checkZones;

		public override int Progress(DiluvionSaveData dsd)
		{
			base.Progress(dsd);
			
			foreach (GameZone zone in checkZones)
			{
				foreach (string s in dsd.discoveredZones)
				{
					if (zone.name == s)
						progress++;
				}
			}
			return progress;
		}
	}
}
