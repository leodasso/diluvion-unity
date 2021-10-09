using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BehaviorCuller : CullerBase
{
	[Space]
	public List<MonoBehaviour> AffectedComponents;

	public List<Renderer> affectedRenderers;

	[OnValueChanged("GetRB")]
	public bool SleepRigidbody ;
	
	[ReadOnly, ShowIf("SleepRigidbody")]
	public Rigidbody rb;

	[Button]
	void GetAllComponents()
	{
		foreach (MonoBehaviour m in GetComponents<MonoBehaviour>())
		{
			if (m == this) continue;
			if (AffectedComponents.Contains(m)) continue;
			AffectedComponents.Add(m);
		}
	}

	void GetRB()
	{
		rb = GetComponent<Rigidbody>();
	}


	protected override bool SetState(bool enabled)
	{
		// If the state doesn't need to change, do nothing.
		if (!base.SetState(enabled)) return false;

		foreach (var behavior in AffectedComponents)
		{
			if (!behavior) continue;
			behavior.enabled = enabled;
		}

		foreach (var r in affectedRenderers)
		{
			if (!r) continue;
			r.enabled = enabled;
		}

		if (SleepRigidbody && !enabled && rb)  rb.Sleep();

		return true;
	}
}