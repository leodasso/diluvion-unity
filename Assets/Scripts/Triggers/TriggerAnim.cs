using UnityEngine;
using System.Collections;

public enum AnimTriggerStart
{
    None,
    Awake,
    Start,
    OnSpawned,
    OnEnable
}

public enum AnimTriggerEnd
{
    None,
    Destroy,
    OnDisable,
    OnDespawned

}
    [RequireComponent(typeof(Animator))]
public class TriggerAnim : MonoBehaviour {

    public AnimTriggerStart startTrigger = AnimTriggerStart.None;
    public AnimTriggerEnd endTrigger = AnimTriggerEnd.None;

    public string triggerName = "";


    void Awake()
    {
        if (startTrigger == AnimTriggerStart.Awake)
            TriggerTheState();
    }

	// Use this for initialization
	void Start ()
    {
        if (startTrigger == AnimTriggerStart.Start)
            TriggerTheState();
	}

    void OnEnable()
    {
        if (startTrigger == AnimTriggerStart.OnEnable)
            TriggerTheState();
    }
	
    void OnSpawned()
    {
        if (startTrigger == AnimTriggerStart.OnSpawned)
            TriggerTheState();
    }


    void OnDestroy()
    {
        if(endTrigger==AnimTriggerEnd.Destroy)
            TriggerTheState();

    }

    void OnDisable()
    {
        if (endTrigger == AnimTriggerEnd.OnDisable)
            TriggerTheState();
    }


    void OnDespawned()
    {
        if (endTrigger == AnimTriggerEnd.OnDespawned)
            TriggerTheState();
    }

    public void TriggerTheState()
    {
        GetComponent<Animator>().SetTrigger(triggerName);
    }





}
