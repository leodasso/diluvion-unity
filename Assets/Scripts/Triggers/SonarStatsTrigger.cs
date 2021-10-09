using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Diluvion.Sonar;
using Diluvion.Ships;

public class SonarStatsTrigger : Trigger {
  
	public List<SonarStats> statsToTurnOn;

    public List<SonarStats> statsToTurnOff;
	[InfoBox("If true, all stats in toTurnOn will be turned off on start, and " +
		"vice versa for toTurnOff")]
	public bool applyInverseAtStart;

	protected override void Start() {

		if (applyInverseAtStart) {

			foreach (SonarStats ss in statsToTurnOn)
			{
				if (ss == null) return;
				ss.enabled = false;
			}


			foreach (SonarStats ss in statsToTurnOff)
			{
				if (ss == null) return;
				ss.enabled = true;
			}
		}
	}

	public override void TriggerAction (Bridge otherBridge)
	{
		base.TriggerAction (otherBridge);

		foreach (SonarStats ss in statsToTurnOn)
		{
			if (ss == null) return;
			ss.enabled = true;
		}

		foreach (SonarStats ss in statsToTurnOff)
		{
			if (ss == null) return;
			ss.enabled = false;
		}
	}
}
