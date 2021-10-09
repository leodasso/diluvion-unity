using System.Collections;
using System.Collections.Generic;
using Diluvion;
using UnityEngine;
using UnityEngine.UI;

namespace DUI
{

	public class HazardSlotPanel : DUIPanel
	{
		public Hazard hazard;
		public Image hazardImage;

		public void Init(Hazard h)
		{
			hazard = h;
			hazardImage.sprite = hazard.sprite;
		}
	}
}