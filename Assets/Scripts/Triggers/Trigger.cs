using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using HeavyDutyInspector;
using Queries;
using Diluvion;
using Diluvion.Ships;
using Quests;
using Sirenix.OdinInspector;

public enum TriggerMode {

	FirstTrigger,
	EveryTrigger
}

/// <summary>
/// Calls 'trigger action' every time a player enters it.
/// </summary>

[RequireComponent(typeof(Collider))]
public class Trigger : MonoBehaviour {

	public enum TriggerStatus
	{
		NotTriggered, WaitingForCamMode, WaitingForDelay, Triggered
	}

    enum InteractionType
    {
        triggerEnter,
        triggerExit
    }

	[FoldoutGroup("trigger")] public TriggerStatus triggerStatus;

    [FoldoutGroup("trigger")]
    public List<Query> queries = new List<Query>();

    [FoldoutGroup("trigger")]
    public bool invertQueries;

	[Space, FoldoutGroup("trigger"), Tooltip("Require a certain quest status before allowing this to trigger?")] 
	public bool checkQuest;

	[FoldoutGroup("trigger"), ShowIf("checkQuest"), AssetsOnly, Indent(1), LabelText("Quest")] 
	public DQuest triggerQuest;

	[FoldoutGroup("trigger"), ShowIf("checkQuest"), Indent(), LabelText("Status")] 
	public QuestStatus questStatus = QuestStatus.NotStarted;

	[FoldoutGroup("trigger"), ShowIf("checkQuest"), Indent()] 
	public bool checkObjective;

	[FoldoutGroup("trigger"), ShowIf("ShowObjective"), AssetsOnly, Indent(2), LabelText("Objective")] 
	[AssetList(CustomFilterMethod = "PartOfQuest")]
	public Objective triggerObjective;

	[FoldoutGroup("trigger"), ShowIf("ShowObjective"), Indent(2), LabelText("Status")] 
	public QuestStatus objectiveStatus;
	
	bool PartOfQuest(Objective o)
	{
		if (!triggerQuest) return false;
		return triggerQuest.HasObjective(o);
	}
	
	bool ShowObjective()
	{
		return checkQuest && checkObjective;
	}

    [Space, FoldoutGroup("trigger")]
	public TriggerMode triggerMode;
	
    [FoldoutGroup("trigger"), Tooltip("The delay between the trigger being hit by player, and the trigger action.")]
    public float delayTime;

    [FoldoutGroup("trigger"), Tooltip("Can this only be triggered by the player?")]
    public bool playerOnly = true;

    [FoldoutGroup("trigger")]
    public bool requireCamMode;

    [ShowIf("requireCamMode"), FoldoutGroup("trigger")]
	public CameraMode camMode;

	[FoldoutGroup("trigger"), ReadOnly] public float lastTimeTriggered = -99;
	
	int _numberOfTimes;
	Collider _trigger;

	// Use this for initialization
	protected virtual void Start ()
	{

		_trigger = GetComponent<Collider>();
		gameObject.layer = LayerMask.NameToLayer("Tools");
        SetToTrigger(true);

		// If this checks a quest, add it to the quest manager's tick
		if (checkQuest)
		{
			_trigger.enabled = false;
			QuestManager.Get().questTick += Tick;
		}
	}

    protected void SetToTrigger(bool isTrigger)
    {
        if (GetComponent<Collider>())
            GetComponent<Collider>().isTrigger = isTrigger;
    }

	void Tick()
	{
		if (!_trigger) return;
		_trigger.enabled = QuestStatusOkay() ;
	}

	[Button, FoldoutGroup("trigger")]
    void TestQueries()
    {
        if (queries.Count < 1)
        {
            Debug.Log("This trigger is always active.");
            return; 
        }
        string s = "";
        if (invertQueries) s = "not";
        Debug.Log("This trigger is " + s + " active if:");
        foreach (Query q in queries) Debug.Log(q.ToString());

        Debug.Log("Result is: " + CheckQueries(null));
    }


	void OnTriggerEnter(Collider other) {

		Triggered(other);

        Bridge otherBridge = other.GetComponent<Bridge>();
        if (otherBridge == null) return;
		if (playerOnly && !otherBridge.IsPlayer()) return;
		
		Debug.Log(name + " was triggered by player", gameObject);

		StartCoroutine(CheckIfPlayer(otherBridge, InteractionType.triggerEnter));
	}


    void OnTriggerExit(Collider other)
    {
        Bridge otherBridge = other.GetComponent<Bridge>();
        if ( otherBridge == null ) return;
	    if (playerOnly && !otherBridge.IsPlayer()) return;

        StartCoroutine(CheckIfPlayer(otherBridge, InteractionType.triggerExit));
    }

    /// <summary>
    /// Called whenever trigger is entered, totally ignoring all checks & delays
    /// </summary>
	public virtual void Triggered(Collider other) { }

	IEnumerator CheckIfPlayer(Bridge otherBridge, InteractionType interaction) {

		if (requireCamMode) {
			while (OrbitCam.CamMode() != camMode)
			{
				triggerStatus = TriggerStatus.WaitingForCamMode;
				yield return new WaitForSeconds(.5f);
			}
		}

		// If this trigger only happens for players, check if the bridge is the player's bridge
		//if (playerOnly) 
		//	if (otherBridge != PlayerManager.pBridge) yield break;

		if (delayTime > 0)
		{
			triggerStatus = TriggerStatus.WaitingForDelay;

			// Wait for delay
			yield return new WaitForSeconds(delayTime);
		}

		// Check for trigger mode
		if (_numberOfTimes > 0 && triggerMode == TriggerMode.FirstTrigger)
		{
			Debug.Log("Breaking trigger action, because " + name + " is only supposed to trigger once.");
			yield break;
		}
		_numberOfTimes++;

        if ( requireCamMode )
        {
	        while (OrbitCam.CamMode() != camMode)
	        {
		        triggerStatus = TriggerStatus.WaitingForCamMode;
		        yield return new WaitForSeconds(.5f);
	        }
        }

		// Break if the queries aren't true
		if (!CheckQueries(otherBridge))
		{
			Debug.Log("Queries not met for " + name + " by " +otherBridge.name + " at " + Time.unscaledTime);
			yield break;
		}

		lastTimeTriggered = Time.unscaledTime;

		triggerStatus = TriggerStatus.Triggered;

        if (interaction == InteractionType.triggerEnter) TriggerAction(otherBridge);
        if ( interaction == InteractionType.triggerExit ) TriggerExitAction(otherBridge);
	}

    /// <summary>
    /// If one of the queries fails, returns false.
    /// </summary>
    /// <returns></returns>
    bool CheckQueries(Object o)
    {
        // Check that the queries are all true
        foreach (Query q in queries)
        {
            if (q.IsTrue(o) == invertQueries) return false;
        }
        return true; 
    }

	/// <summary>
	/// Is the quest status of this trigger all good?
	/// </summary>
	bool QuestStatusOkay()
	{
		if (!checkQuest) return true;

		if (!triggerQuest.IsOfStatus(questStatus)) return false;

		if (checkObjective)
		{
			if (!triggerObjective.IsOfStatus(objectiveStatus, triggerQuest)) return false;
		}

		return true;
	}

    /// <summary>
    /// Called when a collider that fits my requirements has exited my trigger.
    /// </summary>
    /// <param name="otherBridge"></param>
    public virtual void TriggerExitAction(Bridge otherBridge)
    {

    }

	/// <summary>
	/// Called when a collider that fits my requirements enters my trigger (after delay time, cam state check, etc).
	/// </summary>
	public virtual void TriggerAction( Bridge otherBridge) 
	{
	//	Debug.Log("Trigger action!.", gameObject);
	}

    public virtual void TriggerAnimCallback()
    {

    }
}