﻿using UnityEngine;

namespace I2.Loc
{
	public class dfSetLanguage : MonoBehaviour 
	{
		public string Language;

		void OnClick()
		{
			if( LocalizationManager.HasLanguage(Language))
			{
				LocalizationManager.CurrentLanguage = Language;
			}
		}
	}
}