using UnityEngine;
using System.Collections;
using Diluvion;
using Diluvion.Ships;

public class TriggerLookAt : Trigger {

	public float lookTime = 5;
	public Transform lookTarget;
    public bool landmark;


	public override void TriggerAction (Bridge otherBridge)
	{
		base.TriggerAction (otherBridge);
       // Debug.Log("PLEASE RAPE MY FACE IM A TRIGGER TO LOOK AT");
		StartCoroutine(SetBiasTarget(otherBridge.transform));
	}

	IEnumerator SetBiasTarget(Transform playerShip) {

		Transform camLookTarget = transform;
		if (lookTarget) camLookTarget = lookTarget;

		OrbitCam.Get().biasTarget = camLookTarget;
       
        yield return new WaitForSeconds(lookTime);

		OrbitCam.Get().biasTarget = null;
		yield break;
	}

	void OnDestroy() {
		if (OrbitCam.Exists())
			OrbitCam.Get().biasTarget = null;
	}
}
