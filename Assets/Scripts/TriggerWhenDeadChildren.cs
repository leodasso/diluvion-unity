using UnityEngine;
using System.Collections;
using Diluvion;

/// <summary>
/// Turns on linked dock controls docking ability when all the children are DEADDDDD
/// </summary>
public class TriggerWhenDeadChildren : MonoBehaviour {

	public DockControl linkedDock;
  

	int deadChildren = 0;
	int originalChildren;

	void Start() {

        if ( linkedDock ) linkedDock.SetDockable(false);
      
		originalChildren = transform.childCount;
		foreach (Hull hull in GetComponentsInChildren<Hull>()) 
		{
			hull.myDeath += ChildDied;
		}
	}


	void ChildDied(Hull bridge, string byWho)
	{
		deadChildren++;

		if (deadChildren >= originalChildren) {

            if ( linkedDock ) linkedDock.SetDockable(true);
        }
	}
}
