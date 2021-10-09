using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Endless corridor controller. There should only ever be one instance in a scene.
/// </summary>
public class EndlessCorridor : MonoBehaviour {

	public bool corridorActive;
	public List<Animator> affectedAnimators;
    public List<GameObject> objectsToTurnOff = new List<GameObject>();

	public static EndlessCorridor endlessCorridor;

	public static EndlessCorridor Get() {
		if (endlessCorridor != null) return endlessCorridor;
		endlessCorridor = GameObject.FindObjectOfType<EndlessCorridor>();
		return endlessCorridor;
	}

	// Use this for initialization
	void Start () {
	
	}
	
    /// <summary>
    /// Called by the animation when the Corridor is active
    /// </summary>
    public void CorridorActive()
    {
        OffObjects(false);

    }
    /// <summary>
    /// Called by  the animation when the corridor is inactive and default
    /// </summary>
    public void CorridorInactive()
    {
        OffObjects(true);
    }

    void OffObjects(bool turnedOn)
    {
        foreach (GameObject go in objectsToTurnOff)
        {
            if (go == null) continue;
            go.SetActive(turnedOn);
        }
    }

	/// <summary>
	/// Sets the corridor active, tells each affected animator to play the corresponding 
	/// animation.
	/// </summary>
	/// <param name="nowActive">If set to <c>true</c> now active.</param>
	public void SetCorridorActive(bool nowActive) {

		if (corridorActive == nowActive) return;
		corridorActive = nowActive;
     
       

		foreach (Animator anim in affectedAnimators) {
			
			anim.SetBool("active", nowActive);
			anim.SetTrigger("change");
		}
	}
}
