using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ComponentToggler : MonoBehaviour
{
	
	public enum cToggleType
	{
		TurnOn,
		TurnOff,
		Jiggle
	}

	[InfoBox("Can activate, deactivate, or jiggle any component.")] 
	public MonoBehaviour component;
	
	public cToggleType toggleMode = cToggleType.Jiggle;

	public float delay = 5;

	// Use this for initialization
	IEnumerator Start ()
	{

		if (component == null) yield break;
		
		yield return new WaitForSeconds(delay);


		switch (toggleMode)
		{
			case cToggleType.Jiggle:
				component.enabled = false;

				yield return new WaitForSeconds(.1f);
				component.enabled = true;
				break;

			case cToggleType.TurnOff:
				component.enabled = false;
				break;

			case cToggleType.TurnOn:
				component.enabled = true;
				break;
		}
	}
}
