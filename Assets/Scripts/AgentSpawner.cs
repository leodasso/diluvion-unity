using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Diluvion;
using Diluvion.Ships;
using Sirenix.OdinInspector;


public class AgentSpawner : Trigger
{
	/// <summary>
	/// If this spawn doesn't allow repeat spawning, spawn will be consumed once the spawn sequence is complete. 
	///Once this happens, it can't do any more spawning
	/// </summary>
	[ReadOnly, Tooltip("If this spawn doesn't allow repeat spawning, spawn will be consumed once the spawn sequence is complete. " +
	                   "Once this happens, it can't do any more spawning.")]
	public bool spawnConsumed;
	
    [TabGroup("SpawnRules"), Tooltip("Begin the spawn sequence when this gameobject is enabled. If allowRepeat, it will spawn EVERY time it's enabled.")]
    public bool spawnOnEnable;

    [TabGroup("SpawnRules"), Tooltip("Begin spawn sequence when triggered by the player. If allowRepeat, it will spawn EVERY time it's triggered.")]
    public bool spawnOnTrigger;

    [TabGroup("SpawnRules")]
    [Tooltip("On disable kills all spawned instances, and returns to the state I was on init. Will only allow spawning again if " +
             "'allow repeat' is set to true.")]
    public bool resetOnDisable;

    [TabGroup("SpawnRules"), Tooltip("Allow spawning multiple times.")]
    public bool allowRepeat;    

    [TabGroup("SpawnRules"), Tooltip("Don't spawn unless these other spawners have completed their sequence")]
    public List<AgentSpawner> waitForSpawnsKilled = new List<AgentSpawner>();

    [TabGroup("Spawns"), Button]
    void AddNewSpawn()
    {
        GameObject childObj = AddObject();
        SpawnPoint ssPawn = childObj.AddComponent<SpawnPoint>();
        spawnPoints.Add(ssPawn);
    }

    [TabGroup("Spawns", false, 1), Tooltip("Cooldown between each individual spawn"), MinValue(.1f)]
    public float spawnCooldown = 1;

    [TabGroup("Spawns", false, 2), OnValueChanged("ResizeSpawnList")]
    public List<SpawnPoint> spawnPoints = new List<SpawnPoint>();  

    // Delegate that gets called when spawn behavior starts
    //public delegate void BeginSpawnDelegate(AgentSpawner spawner);
    //public BeginSpawnDelegate beginSpawn;

    //public delegate void SpawningShips(Captain shipSpawned);
    //public SpawningShips spawnShips;

    //public delegate void AllSpawnsKilled();
    //public AllSpawnsKilled allSpawnsKilled;

    [TabGroup("Spawns"), ReadOnly]
    public List<GameObject> spawnedObjects = new List<GameObject>();

    int _shipsSpawned;
    int _shipsKilled;


    void OnDisable()
    {
	    if (resetOnDisable) ResetSpawner();
    }
	
	void OnEnable()
	{      
		//if (spawnOnEnable) StartCoroutine(SpawnSequence());
	}
    
    /// <summary>
    /// Resizes the spawn list dynamically
    /// </summary>
    void ResizeSpawnList()
    {
        List<SpawnPoint> childPoints = GetChildSpawners();

        //If we have too many unregistered children
        if (childPoints.Count > spawnPoints.Count)
        {
            foreach (SpawnPoint sp in childPoints)
            {
                if (spawnPoints.Contains(sp)) continue;
                DestroyImmediate(sp.gameObject);// kill them
            }
        }
        else
        {
            foreach (SpawnPoint sp in spawnPoints)
            {
                if(sp==null)
                {
                    GameObject childObj = AddObject();
                    spawnPoints[spawnPoints.IndexOf(sp)] = childObj.AddComponent<SpawnPoint>();
                }
            }
        }
    }



/*
    public void DisableSpawnPoints()
    {
        foreach ( SpawnPoint sp in spawnPoints ) sp.gameObject.SetActive(false);
    }

	/*
    /// <summary>
    /// If any of my spawns are visible to the player, returns true.
    /// </summary>
    public bool SpawnsAreVisible()
    {
        if ( !PlayerManager.IsPlayerAlive() ) return false;
        bool vis = false;

        foreach (GameObject go in spawnedObjects)
        {
            if ( go == null ) continue;
            if ( !go.activeInHierarchy ) continue;

            // visibility check
            if (OrbitCam.VisibleDistance() * 1.2f < Vector3.Distance(PlayerManager.PlayerTransform().position, go.transform.position)) 
                vis = true;
        }
        return vis;
    }
    */
	


    /// <summary>
    /// Gets all child spawners
    /// </summary>
    List<SpawnPoint> GetChildSpawners(List<SpawnPoint> culledList = null) {

        List<SpawnPoint> childPoints = new List<SpawnPoint>();
		foreach (SpawnPoint spawner in transform.GetComponentsInChildren<SpawnPoint>())
        {
			// Don't add members that are already part of the list
			if (culledList!=null&&culledList.Contains(spawner)) continue;

            childPoints.Add(spawner);
		}

        return childPoints;
	}


	/// <summary>
	/// Returns ships to spawn, but culls out any in the list that are inactive
	/// </summary>
	int SpawnCount() {

		int toSpawn = 0;     
        foreach (SpawnPoint sp in spawnPoints){
			if (sp == null) continue;
			if (sp.gameObject.activeInHierarchy) toSpawn++;
		}

		return toSpawn;
	}


	/*
    [Sirenix.OdinInspector.Button]
    void TestSpawn()
    {
	    // Destroy previous spawned objects & clear the list
	    foreach (GameObject go in spawnedObjects)                 
		    DestroyImmediate(go);
		
	    spawnedObjects.Clear();
	    
        foreach (SpawnPoint spawnPoint in spawnPoints)
        {
            if (spawnPoint == null) continue;
            if (!spawnPoint.gameObject.activeInHierarchy) continue;

            spawnedObjects.Add(spawnPoint.Spawn(this));
            _shipsSpawned++;           
        }
    }
*/
	
	#region spawning
	
	
	bool _spawningInProgress;
	
	/// <summary>
	/// Starts the spawn coroutine
	/// </summary>
	public void BeginSpawn() {
		//StartCoroutine(SpawnSequence());
	}
	
	/*
	//Spawn behaviour, there is always a delay for the first spawn, will repeat 
	IEnumerator SpawnSequence()
	{
		if (spawnConsumed || _spawningInProgress) yield break;
		_spawningInProgress = true;

		//beginSpawn?.Invoke(this);

		// A short delay to allow time for culling & quest components to disable this
		yield return new WaitForSeconds(.1f);

		// Wait for any other spawners to have been defeated before beginning spawn
		foreach (AgentSpawner spawner in waitForSpawnsKilled)
		{
			if (spawner == null) continue;
			while (!spawner.SequenceComplete()) yield return new WaitForSeconds(.5f);
		}

		// Clear the 'spawned objects' list
		spawnedObjects = new List<GameObject>();
	    
		//spawns a variable number of ships at a variable number of locations(defaults to transform)
		foreach (SpawnPoint spawnPoint in spawnPoints)
		{
			if (spawnPoint == null) continue;
			if (!spawnPoint.gameObject.activeInHierarchy) continue;
        
			spawnedObjects.Add(spawnPoint.Spawn(this));      
			_shipsSpawned++;
			yield return new WaitForSeconds(spawnCooldown);
		}

		// Check for completion. If the player destroyed the spawns then left the scene, then returned, the spawnpoints would have saved and thus disabled.
		// checking at this point will be a failsafe for getting quests completed.
		CheckForCompletion();

		_spawningInProgress = false;

		if (!allowRepeat) spawnConsumed = true;
	}
	*/

	
	/// <summary>
	/// Cancels any spawning behavior in progress. If spawning is already complete, this won't do anything.
	/// </summary>
	public void CancelSpawning()
	{
		if (spawnConsumed) return;
		StopAllCoroutines();
		_spawningInProgress = false;
		
		//destroy any instances that were created 
		KillInstances();
	}
	
	/// <summary>
	/// Resets the spawner to the point it was at when the scene started, kills all instances
	/// </summary>
	public void ResetSpawner()
	{
		StopAllCoroutines();
		spawnConsumed = false;
		_spawningInProgress = false;
		KillInstances();
	}
	
	/// <summary>
	/// Destroys any instances that have been spawned without going through the hull code path. Won't trigger hull's 
	/// destroyed model to be spawned, or any quest progress.
	/// </summary>
	void KillInstances()
	{
		if (!Application.isPlaying) return;
	    
		foreach (GameObject go in spawnedObjects)
		{
			if (go == null) continue;
			GameManager.Despawn(go);
		}
	    
		spawnedObjects.Clear();
	}
	
	#endregion

	GameObject AddObject() 
	{
		GameObject childObj = new GameObject("spawn point");
		childObj.transform.position = transform.position;
		childObj.transform.SetParent(transform);
		int siblingIndex = childObj.transform.GetSiblingIndex();
		Vector3 offsetVector = Vector3.up * siblingIndex;
		childObj.transform.localPosition += offsetVector;
		childObj.name += " " + siblingIndex;

		return childObj;
	}  

    //For OnTriggerSpawn
    void OnTriggerEnter(Collider other)
    {
		if (!spawnOnTrigger) return;
        if (spawnConsumed) return;
        if (!other.GetComponent<Bridge>()) return;
        if (!other.GetComponent<Bridge>().IsPlayer()) return;

        //StartCoroutine(SpawnSequence());
    }

	/// <summary>
	/// Keeps track of how many ships have been spawned / killed, and broadcasts messages for questmanager. Called by the myDeath delegate
	/// in Hull.
	/// </summary>
	public void ShipDied(Hull deadShip, string byWhat) {

		_shipsKilled ++;
		
        // If all ships have been spawned and killed, broadcast message
        CheckForCompletion();
	}

    /// <summary>
    /// Tells quest manager if all my spawns have been eliminated.
    /// </summary>
    void CheckForCompletion()
    {
        if ( SequenceComplete() )
        {
            /*
            if ( QuestManager.Get() )
                if ( PlayerManager.Get() )
                    QuestManager.Get().RecordEvent(QuestConditionType.DestroySpawns, PlayerManager.Get().playerShipInstance.gameObject, gameObject);
                    */
            //if ( allSpawnsKilled != null ) allSpawnsKilled();
        }
    }

	/// <summary>
	/// Returns true if all my ships have been spawned and destroyed
	/// </summary>
	 bool SequenceComplete() {

		if (_shipsSpawned < SpawnCount()) return false;
		if (_shipsKilled >= _shipsSpawned) return true;
		return false;
	}

	/// <summary>
	/// Returns true if spawning has begun and not all spawned ships are destroyed
	/// </summary>
	bool SequenceInProgress() 
	{
		if (_shipsSpawned > 0 && _shipsKilled < SpawnCount()) return true;
		else return false;
	}
}