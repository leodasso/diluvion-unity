using UnityEngine;
using System.Collections;
using Diluvion.Ships;
using Wilberforce.VAO;

public class SetToSky : Trigger 
{
	public Material skybox;

	public override void TriggerAction (Bridge otherBridge)
	{
		base.TriggerAction (otherBridge);
		RenderSettings.skybox = skybox;
		Camera.main.clearFlags = CameraClearFlags.Skybox;
		Camera.main.gameObject.transform.GetComponent<VAOEffect>().ColorTint = Color.white;
	}
}
