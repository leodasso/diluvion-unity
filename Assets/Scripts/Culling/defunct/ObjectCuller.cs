using UnityEngine;
using System.Collections;
using Diluvion.Ships;

public class ObjectCuller : MonoBehaviour
{

	public GameObject culledObject;       

	void OnTriggerEnter (Collider other)
    {
        if (!other.GetComponent<Bridge>()) return;
        if (!other.GetComponent<Bridge>().IsPlayer()) return;
        if (culledObject == null) culledObject = transform.GetChild(0).gameObject;
        culledObject.SetActive(true);
    }

	void OnTriggerExit (Collider other)
    {
        if (!other.GetComponent<Bridge>()) return;
        if (!other.GetComponent<Bridge>().IsPlayer()) return;
        if (culledObject == null) culledObject = transform.GetChild(0).gameObject;
        culledObject.SetActive(false);

    }
}
