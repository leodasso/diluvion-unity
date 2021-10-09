using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Diluvion.Achievements
{
	
	/// <summary>
	/// Checks the save file for the int field name fieldname
	/// </summary>
	[CreateAssetMenu(fileName = "intfieldCheck", menuName = "Diluvion/Achievement/AchievementIntFieldCheck")]
	public class DSaveIntFieldCheck : DSaveAchievement 
	{
		public string fieldName;
	
		public override int Progress(DiluvionSaveData dsd)
		{
			base.Progress(dsd);
			progress = (int)dsd.GetType().GetField(fieldName).GetValue(dsd);
			
			return progress;
		}
	}
}

