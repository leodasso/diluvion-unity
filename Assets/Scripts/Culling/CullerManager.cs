using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

public class CullerManager : MonoBehaviour
{
	[ReadOnly]
	public List<CullerBase> cullersInScene = new List<CullerBase>();

	[ReadOnly, Tooltip("The time in seconds of the last cycle updating all the cullers in the scene")]
	public float lastCycleTime;

	[ReadOnly]
	public int currentIndex;
	const float InstancesPerUpdate = 5;

	float _cycleStartTime;

	void Start()
	{
		DontDestroyOnLoad(gameObject);
		_cycleStartTime = Time.unscaledTime;
	}

	// Update is called once per frame
	void Update () {

		for (int j = 0; j < InstancesPerUpdate; j++)
		{
			UpdateNextCuller();
			IterateIndex();
		}
	}

	CullerBase _currentCuller;
	void UpdateNextCuller()
	{
		if (cullersInScene.Count < 1) return;
		
		currentIndex = Mathf.Clamp(currentIndex, 0, cullersInScene.Count - 1);
		_currentCuller = cullersInScene[currentIndex];
		if (_currentCuller == null) return;
		
		_currentCuller.LocalUpdate();
		
	}

	void IterateIndex()
	{
		currentIndex++;
		
		// When cycled through the full list, give it a cleaning as well (to remove null elements)
		if (currentIndex >= cullersInScene.Count)
		{
			currentIndex = 0;
			CleanList();
			lastCycleTime = Time.unscaledTime - _cycleStartTime;
			_cycleStartTime = Time.unscaledTime;
		}
	}

	/// <summary>
	/// Removes null elements from the list
	/// </summary>
	void CleanList()
	{
		cullersInScene = cullersInScene.Where(x => x != null).ToList();
	}
}
