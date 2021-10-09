using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HeavyDutyInspector;
using Diluvion;
using Diluvion.Ships;

[System.Serializable]
public struct ArkStageObjects
{
    public List<GameObject> objectsToActivate;

    public void ActivateObjects(bool active)
    {
        foreach (GameObject go in objectsToActivate)
            go.SetActive(active);
    }
    

}



[RequireComponent(typeof(ObjectsOnPath))]
public class ArkResentment : MonoBehaviour {

	public enum ArkMode {ChasePlayer, Circle, Still};

	public ArkMode arkMode;

	[Comment("Max speed is in units along spline per second.")]
	public float startMaxSpeed;
	public float maxSpeed = 100;
	[Space]

	public float minChaseSpeed = 20;
	public float acceleration = 1;
	public Transform head;

    

	[Comment("A list of spawners. Each spawner represents a difficulty level; the first element in the list" +
		"appears first, and is the easiest. Once that's defeated, moves to the next, and so on.")]
	public List<AgentSpawner> spawners;

	[Comment("Each atmosphere in the list corresponds to the stage of spawners of the same index.")]
    public List<ArkStageObjects> arkStageObjects;

	[Space]

	[Comment("How quickly it gets to the ideal distance to the player in the follow mode.")]
	public float magnetism = 5;
	public float idealDistToPlayer = 50;

	[Space]
	public float oscIntensity;
	public float damagedIntensity;
	[Space]

	public float waviness = 5;
	[Space]

	public float oscSpeed = 1;
	public float damagedOscSpeed = 1;
	[Space]

	public float detectDist = 100;

	[Space]
	public GameObject endingObject;
    [Space]
    public GameObject AscensionMusicObject;

    List<SplineObject> splineObjects = new List<SplineObject>();

	int spawnLevel = 0;			// represents the index of spawners
	float difficultyLerp;		// 0 is beginning of battle, 1 is end
	float realMaxSpeed;
	float speed;
	ObjectsOnPath pathMover;
	Bridge player;
	Transform playerShip;
	float initOscIntensity;
	float initSpeed;


	void OnDrawGizmos() {

		Gizmos.color = new Color(0, 1, 1, .2f);
        if (SDTMP() == null) return;
        foreach (Transform t in SDTMP().Points())
        {
            if (t == null) continue;
            Gizmos.DrawSphere(t.position, detectDist);
        }

	}

	// Use this for initialization
	void Start () {

		pathMover = GetComponent<ObjectsOnPath>();

		// Get all my spline object children
		splineObjects.AddRange(GetComponentsInChildren<SplineObject>());

		// TODO find another way to check if all spawns are destroyed
		//foreach (AgentSpawner spawner in spawners) spawner.allSpawnsKilled += AllThornsDestroyed;

		initOscIntensity = oscIntensity;
		initSpeed = oscSpeed;

	//	foreach (LocalAtmosphere atmo in atmospheres) atmo.gameObject.SetActive(false);

        foreach (ArkStageObjects atmo in arkStageObjects) atmo.ActivateObjects(false);
    }
	
	// Update is called once per frame
	void Update () {

		if (oscIntensity > .1f) {

			foreach (SplineObject obj in splineObjects) {
				obj.oscIntensity 	= oscIntensity;
				obj.wavesOnBody 	= waviness;
				obj.oscSpeed 		= oscSpeed;
			}
		}

		// Lerp between 0 and 1 depending on the player's progress in the boss fight
		float difficultyLerp = spawnLevel / spawners.Count;
		realMaxSpeed = Mathf.Lerp(startMaxSpeed, maxSpeed, difficultyLerp);
	}

	public void StartIntroRoutine() {

		StopAllCoroutines();
        head.GetComponent<AKTriggerPositive>().TriggerPos();
        arkStageObjects[0].ActivateObjects(true);
        SpawnThorns();
       // SetEmission(1);

        StartCoroutine(MainRoutine());
	}

	/// <summary>
	/// Spawns the thorns of the current spawner's index.
	/// </summary>
	void SpawnThorns() {
		
		AgentSpawner currentSpawn = spawners[spawnLevel];

		Debug.Log("Spawning thorns for level " + spawnLevel, currentSpawn.gameObject);

		currentSpawn.BeginSpawn();
	}
		

	IEnumerator MainRoutine() {

		yield return StartCoroutine(FollowPlayer(detectDist, 120, 3));
		//yield return StartCoroutine(DoLoop(.3f, 5));
		//yield return StartCoroutine(FollowPlayer(detectDist, 45, 2));

		StartCoroutine(MainRoutine());
		yield break;
	}

	/// <summary>
	/// Gets called when all the thorns of the current spawn are destroyed.
	/// </summary>
	void AllThornsDestroyed() {
		StartCoroutine(AllThornsDestroyedSequence());
	}

	IEnumerator AllThornsDestroyedSequence() {

		Debug.Log("All thorns destroyed for index " + spawnLevel);
      
		spawnLevel++;
		if (spawnLevel >= spawners.Count) {
			Debug.Log("Boss battle complete!");
			StartCoroutine(EndingRoutine());
			yield break;
		}

        head.GetComponent<AKTriggerCallback>().Callback();
        StopCoroutine(MainRoutine());

		StartCoroutine(StandStill());

		yield return new WaitForSeconds(3);

		SpawnThorns();
		//LocalAtmosphere atmo = atmospheres[spawnLevel];
       // atmo.gameObject.SetActive(true);
        arkStageObjects[spawnLevel].ActivateObjects(true);
		

		yield return new WaitForSeconds(8);

		yield return StartCoroutine(DoLoop(.5f, 8));

		StartCoroutine(MainRoutine());

		yield break;
	}

	/// <summary>
	/// Moves the on spline the given amount of units per second. Returns the amount moved this frame
	/// </summary>
	float MoveOnSpline(float amount) {

		float moveAmount = amount * Time.deltaTime;
		pathMover.splineOffsetDistance += moveAmount;
		return moveAmount;
	}

	/// <summary>
	/// Interpolates the speed towards the given value at the set acceleration.
	/// </summary>
	void LerpSpeed(float newSpeed){ 
		speed = Mathf.Lerp(speed, newSpeed, Time.deltaTime * acceleration);
	}

	/// <summary>
	/// Travel in full loops around the spline. 
	/// </summary>
	IEnumerator DoLoop(float numberOfLoops, float speedMultiplier) {

		yield return new WaitForSeconds(.2f);

		//Debug.Log("Beginning path run of " + numberOfLoops + " loops along spline " + pathMover.spline, pathMover.spline.gameObject);

		arkMode = ArkMode.Circle;

		float totalDistance = pathMover.spline.Length * numberOfLoops;
		float distanceTraveled = 0;

		while (distanceTraveled < totalDistance) {

			LerpSpeed(realMaxSpeed * speedMultiplier);
			distanceTraveled += MoveOnSpline(speed);

			yield return null;
		}

		yield break;
	}

 


	Vector3 FollowDistanceOrigin() {
		if (head == null) return transform.position;

		return head.position + head.transform.up * idealDistToPlayer;
	}

    ShortestDistanceToMyPoints sdtmp;
    /// <summary>
    /// Safe Get for the measuring component.
    /// </summary>
    /// <returns></returns>
    ShortestDistanceToMyPoints SDTMP()
    {
        if (sdtmp != null) return sdtmp;
        sdtmp = GetComponent<ShortestDistanceToMyPoints>();
        return sdtmp;
    }

	/// <summary>
	/// Follows the player.
	/// </summary>
	/// <returns>The player.</returns>
	/// <param name="detectionDist">Until within this distance to the player, will just keep circling around at max speed.</param>
	/// <param name="howLong">How long to stay by the player.</param>
	IEnumerator FollowPlayer(float detectionDist, float howLong, float speedMult) {

		arkMode = ArkMode.ChasePlayer;

		yield return new WaitForSeconds(.5f);

		playerShip = PlayerManager.PlayerTransform();

		// If the player isn't there, stop.
		if (playerShip == null) {
			StartCoroutine(StandStill());
			yield break;
		}

		float elapsedTime = 0;
		float distToPlayer = 0;
		float delta = 0;

		while (elapsedTime < howLong) {

            // Figure out how far from player
            float newDist = SDTMP().DistanceFromClosestPoint(playerShip.transform);//Vector3.Distance(FollowDistanceOrigin(), playerShip.position);
            delta = newDist - distToPlayer;
            distToPlayer = newDist;

			if ( distToPlayer > detectionDist)
				LerpSpeed(realMaxSpeed * speedMult);

			else {
				
				float speedAdjust = delta * magnetism;
				float newSpeed = speedAdjust * realMaxSpeed;

				newSpeed = Mathf.Clamp(newSpeed, minChaseSpeed, realMaxSpeed);

				LerpSpeed(newSpeed);
				
				//LerpSpeed(minChaseSpeed);
			}
			
			MoveOnSpline(speed);
			elapsedTime += Time.deltaTime;

			yield return null;
		}
		yield break;
	}


	IEnumerator StandStill() {

		Debug.Log("Starting to stand still.");
		arkMode = ArkMode.Still;

		while (speed > .1f) {
			LerpSpeed(0);
			MoveOnSpline(speed);
			yield return null;
		}
		yield break;
	}

	public void NewThorn() {

		Debug.Log("New thorn at " + Time.time + "!");

		StopCoroutine("DamagedRoutine");
		StartCoroutine(DamagedRoutine(.05f, .9f));
	}

	public void StartDamagedRoutine()
    {
        if (!Application.isPlaying) return;
		StartCoroutine(DamagedRoutine(.5f, 3));
	}

	IEnumerator DamagedRoutine(float timeToDamage, float timeToNormal) {

		float lerp 			= 0;
		Debug.Log("Starting damaged routine");
        //SetEmission(3);
        // lerping animations to damaged mode
        while (lerp < 1) {
			oscIntensity 	= Mathf.Lerp(initOscIntensity, 	damagedIntensity, 	lerp);
			oscSpeed 		= Mathf.Lerp(initSpeed, 		damagedOscSpeed, 	lerp);

			lerp += Time.deltaTime / timeToDamage;
			yield return null;
		}

		lerp = 0;

		// lerping animations back to normal 
		while (lerp < 1) {
			oscIntensity 	= Mathf.Lerp(damagedIntensity, 	initOscIntensity, 	lerp);
			oscSpeed 		= Mathf.Lerp(damagedOscSpeed, 	initSpeed, 			lerp);

			lerp += Time.deltaTime / timeToNormal;
			yield return null;
		}

		Debug.Log("Ending damaged routine");
       // SetEmission(1);
        yield break;
	}

	IEnumerator EndingRoutine() {

		StopCoroutine(MainRoutine());
		StopCoroutine("DoLoop");
		StopCoroutine("FollowPlayer");
		StartCoroutine(StandStill());
        // SetEmission(5);
        Vector3 endPos = new Vector3(playerShip.transform.position.x, 0, playerShip.transform.position.z);
        Instantiate(AscensionMusicObject, endPos, Quaternion.identity);
        float totalTime = 7;
		float elapsed = 0;

		float initIntensity = oscIntensity;
		float initSpeed = oscSpeed;

		while (elapsed < totalTime) { 

			float lerp = elapsed / totalTime;

			oscIntensity 	= Mathf.Lerp(initIntensity, 200, lerp);
			oscSpeed 		= Mathf.Lerp(initSpeed, 2, lerp);

			elapsed += Time.deltaTime;
			yield return null;
		}

		foreach (SplineObject so in splineObjects)
			so.GetComponent<ArkSkeletonPart>().Destruct();

		yield return new WaitForSeconds(10);

	    endPos = new Vector3(playerShip.transform.position.x, 0, playerShip.transform.position.z);
        Instantiate(endingObject, endPos, Quaternion.identity);

    }
}
