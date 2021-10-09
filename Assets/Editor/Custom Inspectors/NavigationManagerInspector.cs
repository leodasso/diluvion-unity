using UnityEngine;
using UnityEditor;
using System.Collections;


public class NavigationManagerInspector : Editor 
{

	NavigationManager navMan;

	public void OnEnable()
	{
		navMan  = (NavigationManager)target;
	}

	public override void OnInspectorGUI()
	{

	
		DrawDefaultInspector();
	}
}
