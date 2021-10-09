using System.Collections;
using System.Collections.Generic;
using Diluvion;
using UnityEngine;
using Sirenix.OdinInspector;

public class Teleporter : MonoBehaviour
{

	[ToggleLeft]
	public bool setDepth;

	[ShowIf("setDepth"), Tooltip("Adds this much depth to my position")]
	public float additionalDepth = 100;

	// Use this for initialization
	void Start () {
		
	}

	[Button]
	void TeleportPlayer()
	{
		if (!Application.isPlaying)
		{
			Debug.Log("Can't teleport when not playing.");
			return;
		}

		if (PlayerManager.PlayerShip() == null)
		{
			Debug.LogError("No player ship could be found.");
			return;
		}

		PlayerManager.PlayerShip().transform.position = transform.position;

		if (setDepth)
		{
			float totalDepth = transform.position.y - additionalDepth;
			PlayerManager.PlayerShip().GetComponent<Hull>().testDepth = totalDepth;
		}
	}
}
