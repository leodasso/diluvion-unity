using UnityEngine;
using System.Collections.Generic;
using Queries;
using HeavyDutyInspector;
using Diluvion;


/// <summary>
/// Can lock the player into an interior if all the queries are true.
/// </summary>
public class InteriorLock : MonoBehaviour {

    [Button("Print Behavior", "Test", true)] public bool test1;

    public List<Query> queries = new List<Query>();
    public bool invertQueries;
    [Space]
    public PopupObject tryLeavePopup;
    [Space]
    public bool requireConfig;
    [HideConditional(true, "requireConfig", true)]
    public GameMode requiredMode;

    void Test()
    {
        if (queries.Count < 1) {
            Debug.Log("This interior will never lock the player in.");
            return;
        }
        string n = "";
        if (invertQueries) n = "not ";
        Debug.Log("Interior will " + n + "lock players in if ");
        foreach (Query q in queries) Debug.Log(q.ToString());
    }

	public bool CanLeave() {

        // If requires a config to lock, checks if that's the current active config
        if (requireConfig)
        {
            if ( !GameManager.Mode() == requiredMode) return true;
        }

        foreach (Query q in queries)
        {
            if (q.IsTrue(gameObject) != invertQueries) return false;
        }
		return true;
	}

	public void ShowPopup() 
	{
        tryLeavePopup.CreateUI();
	}
}
