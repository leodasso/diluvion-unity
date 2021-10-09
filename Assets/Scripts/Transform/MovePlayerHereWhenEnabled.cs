using UnityEngine;
using System.Collections;
using Diluvion;

public class MovePlayerHereWhenEnabled : MonoBehaviour {


    Transform t;
    Transform PlayerTransform()
    {

        if (t != null) return t;
        t = PlayerManager.PlayerTransform();
        return t;
    }

	// Use this for initialization
	void OnEnable ()
    {
        StartCoroutine(SetPosition());
	}
	
    IEnumerator SetPosition()
    {
        yield return new WaitForSeconds(0.1f);
        while (PlayerManager.pBridge == null) yield return null;

        PlayerTransform().position = transform.position;
        PlayerTransform().rotation = transform.rotation;
    }
}
