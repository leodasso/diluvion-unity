using System.Collections;
using System.Collections.Generic;
using Diluvion;
using UnityEngine;
using Sirenix.OdinInspector;
using Quests;
using Sirenix.Serialization;
using UnityEngine.Events;

/// <summary>
/// Triggers quest progress when all or some of the damageable children are killed
/// </summary>
[AddComponentMenu("DQuest/progress quest on group kill")]
public class QuestGroupKill : SerializedMonoBehaviour {
	
	
	
	[Tooltip("This component can trigger progress on an objective / quest combo based on how many damageable children are alive. Quest AND" +
	         " objective must be entered for it to work. Valid damageable children are spawners and anything that implements interface iDamageable.")]
	[AssetsOnly, PropertyOrder(-999)]
	public DQuest quest;
        
	[ShowIf("HasQuest"), Indent, AssetList(CustomFilterMethod = "QuestHasObjective", AutoPopulate = false), AssetsOnly, PropertyOrder(-998)]
	public Objective objectiveToProgress;

	bool HasQuest()
	{
		return quest != null;
	}
	
	bool QuestHasObjective(Objective o)
	{
		if (!quest) return false;
		return quest.HasObjective(o);
	}

	[DrawWithUnity, Tooltip("Event that gets invoked when the goal kill count is reached.")]
	public UnityEvent onGoalReached;
	
	[ReadOnly, BoxGroup("children")]
	public List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

	[BoxGroup("children"), ReadOnly, OdinSerialize]
	public List<IDamageable> damageableChildren = new List<IDamageable>();

	[ReadOnly, BoxGroup("children")] 
	public int destructibleCount;
	
	[BoxGroup("children"), ReadOnly]
	public int currentKillCount;
	[BoxGroup("children"), OnValueChanged("ClampObjective"), 
	 Tooltip("When the number of kills is greater than or equal to this, quest progress will be triggered.")]
	public int goalKillCount;
	
	[Button, BoxGroup("children")]
	void GetChildren()
	{
		spawnPoints.Clear();
		spawnPoints.AddRange(GetComponentsInChildren<SpawnPoint>());
		
		damageableChildren.Clear();
		damageableChildren.AddRange(GetComponentsInChildren<IDamageable>());
		GetDestructibleCount();
	}

	bool _calledProgress;

	void Start()
	{
		// Add delegate to damageable so we get a callback when it's killed
		foreach (var child in damageableChildren)
			child.onKilled += ChildKilled;
		
		// add delegate to spawnpoints so we get a callback when its spawn is killed
		foreach (var spawner in spawnPoints)
			spawner.onSpawnKilled += SpawnChildKilled;
	}

	int GetDestructibleCount()
	{
		destructibleCount = damageableChildren.Count + spawnPoints.Count;
		return destructibleCount;
	}

	void ClampObjective()
	{
		goalKillCount = Mathf.Clamp(goalKillCount, 0, destructibleCount);
	}

	void SpawnChildKilled()
	{
		ChildKilled(null);
	}

	void ChildKilled(GameObject child)
	{
		currentKillCount++;
		Debug.Log("One of my children has been killed! Current kills: " + currentKillCount + "/" + goalKillCount);
		if (currentKillCount >= goalKillCount && !_calledProgress) GoalReached();
	}

	void GoalReached()
	{
		Debug.Log("Wow we progressed the objective good job guys " + name);
		_calledProgress = true;
		
		onGoalReached.Invoke();

		if (!objectiveToProgress || !quest) return;
		if (objectiveToProgress.IsComplete(quest)) return;
		objectiveToProgress.ProgressObjective(quest);
	}
}
