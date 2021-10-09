using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using SpiderWeb;
using Sirenix.OdinInspector;

public class InputDebug : MonoBehaviour
{

	public ControllerType lastUsedController;
	public string actionToCheck;
	[ReadOnly]
	public string actionMapping;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
	{

		lastUsedController = Controls.LastUsedControllerType();

		if (!string.IsNullOrEmpty(actionToCheck))
		{
			actionMapping = Controls.InputMappingName(actionToCheck);
		}
	}
}
