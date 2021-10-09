using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Sirenix.OdinInspector;

public class EffectSpawner : MonoBehaviour
{

	public KeyCode spawnCode;
	public GameObject effectToSpawn;
	[MinValue(1)]
	public float lifetime = 15;
	[ReadOnly]
	public TextMeshPro textMesh;

	// Use this for initialization
	void Start ()
	{
		if (GetComponent<Renderer>()) GetComponent<Renderer>().enabled = false;

		textMesh = GetComponentInChildren<TextMeshPro>();
		if (textMesh) textMesh.text = spawnCode.ToString();
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown(spawnCode))
		{
			GameObject newInstance = Instantiate(effectToSpawn, transform.position, transform.rotation, transform);
			Destroy(newInstance, lifetime);
		}
		
	}
}
