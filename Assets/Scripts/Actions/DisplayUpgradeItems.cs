using System.Collections;
using System.Collections.Generic;
using DUI;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Diluvion
{

	/// <summary>
	/// Brings up a UI panel to show the items needed to forge the given upgrade.
	/// </summary>
	[CreateAssetMenu(fileName = "show items action", menuName = "Diluvion/actions/show upgrade items")]
	public class DisplayUpgradeItems : Action
	{

		[AssetsOnly]
		public Forging upgrade;
		
		public override bool DoAction(Object o)
		{
			if (upgrade == null)
			{
				Debug.LogError("No upgrade set!", this);
				return false;
			}
			
			ItemExchangePanel.ShowItemExchange(upgrade.synthesisMaterials, upgrade.LocalizedName(), true);
			return true;
		}

		protected override void Test()
		{
			if (Application.isPlaying)
				DoAction(null);
		}

		public override string ToString()
		{
			if (!upgrade)
			{
				return name;
			}

			return "List of items for " + upgrade.name;
		}
	}
}