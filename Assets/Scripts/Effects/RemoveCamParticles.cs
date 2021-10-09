using UnityEngine;
using System.Collections;
using Diluvion;
using Diluvion.Ships;

public class RemoveCamParticles : Trigger {

	// Update is called once per frame
	public override void TriggerAction (Bridge otherBridge)
	{
		base.TriggerAction (otherBridge);
		Transform cam = OrbitCam.Get().transform;

		foreach (ParticleSystem p in GameObject.FindObjectsOfType<ParticleSystem>()) {
			p.Stop();
			//Destroy(p);
		}
	}
}
