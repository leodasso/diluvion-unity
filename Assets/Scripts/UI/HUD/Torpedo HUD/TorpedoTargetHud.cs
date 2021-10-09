using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DUI
{
	public class TorpedoTargetHud : DUIPanel
	{
		[Space]
		public float maxCalibrationSize = 600;
		public float minCalibrationSize = 50;

		public Transform target;
		public Vector3 targetPos;

		void LateUpdate()
		{
			//throw new System.NotImplementedException();
		}
	}
}