using UnityEngine;
using System.Collections;
using Diluvion.Ships;
using DUI;

public class TutorialTag : Trigger {

	new public string tag;


	public override void TriggerAction (Bridge otherBridge)
	{
		base.TriggerAction (otherBridge);

		//DUITutorial duiTutorial = DUITutorial.Get();
		//if (duiTutorial) duiTutorial.AddTag(tag);
	}
}
