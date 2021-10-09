using System.Collections;
using System.Collections.Generic;
using Diluvion;
using UnityEngine;

namespace DUI
{

	public class ForgeSlotInfo : DUIPanel
	{
		public ForgeItemPanel forgePanel;

		public Forging chunk;
		public GameObject slot;
		

		public static ForgeSlotInfo CreateInstance(Forging newChunk, GameObject newSlot)
		{
			ForgeSlotInfo instance = UIManager.Create(UIManager.Get().forgeSlotInfo as ForgeSlotInfo);

			instance.chunk = newChunk;
			instance.forgePanel.ApplyChunk(newChunk);
			instance.slot = newSlot;
			return instance;
		}

		protected override void Update()
		{
			base.Update();

			if (!slot) return;

			transform.position = FollowTransform(slot.transform.position, 50, InteriorView.InteriorCam());
		}
	}
}