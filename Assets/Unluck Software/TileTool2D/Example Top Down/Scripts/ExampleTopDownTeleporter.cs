using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ExampleTopDownTeleporter : MonoBehaviour {

	[Tooltip("Teleports player to another teleporter.\nUse the search feature to easily find the target teleporter.")]
	public ExampleTopDownTeleporter targetTeleporter;

	[Tooltip("Position to place player after teleport")]
	public Transform teleportToPosition;

	[Tooltip("Delay between changing positions")]
	public float teleportDelay = .25f;

	void Awake() {
		if(targetTeleporter == null) {
			Debug.Log("No target teleporter found, make sure targetTeleporter is set in the teleporter script, script terminated");
			Destroy(this);
		}
	}
	
	void OnCollisionEnter2D(Collision2D coll) {
		StartCoroutine(Teleport(coll.transform));
	}

	IEnumerator Teleport(Transform target) {
		//Make sure that the teleportation doesn't activate again after teleportation
		if (targetTeleporter.teleportToPosition.position != transform.position) {
			target.gameObject.SetActive(false);
			target.position = targetTeleporter.teleportToPosition.position;
			yield return new WaitForSeconds(teleportDelay);
			target.gameObject.SetActive(true);
		}
	}

#if UNITY_EDITOR
	void OnDrawGizmos () {
		if (teleportToPosition == null && Selection.activeTransform == transform) { 
			teleportToPosition = transform.Find("TeleportToPosition");
			return;
		}
		Gizmos.color = Color.yellow;
		if (!targetTeleporter) Gizmos.color = Color.red;
		Gizmos.DrawWireSphere((Vector2)teleportToPosition.position, .15f);
		if(targetTeleporter) Gizmos.DrawLine(teleportToPosition.position, targetTeleporter.teleportToPosition.position);
	}
#endif
}
