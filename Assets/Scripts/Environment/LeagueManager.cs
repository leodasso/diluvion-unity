using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using SpiderWeb;
using Diluvion;

public class LeagueManager : MonoBehaviour {

	public Light directionalLight;
	public Light directionalLight2;
    public List<League> worldLeagues = new List<League>();
    public Atmosphere worldAtmosphere;
	public Atmosphere overrideAtmospere;
    
	public List<LocalAtmosphere> localAtmospheres = new List<LocalAtmosphere>();

    OrbitCam orbitCam;
	Atmosphere currentAtmosphere;
    //Fog camFog;
	bool atmosphereOverride = false;
	float currentDepth;
	float maxHeight = -1000;
	float minHeight = 1000;
	bool init = false;
	float camDefaultEndClip;

	/// <summary>
	/// The transition time of the most recently exited local atmosphere
	/// </summary>
	float mostRecentTransitionTime = 2;

    public static LeagueManager leagueManager;


	//Draw boxes to represent leagues when world object is selected
	void OnDrawGizmosSelected() {

		for (int i = 0; i < worldLeagues.Count; i++) {

			Gizmos.color = worldLeagues[i].localAtmosphere.fogColor;
			Vector3 leagueCenter = new Vector3(0, (worldLeagues[i].upperHeight+ worldLeagues[i].lowerHeight)/2, 0);
			Gizmos.DrawWireCube (leagueCenter, new Vector3 (10000, worldLeagues[i].upperHeight- worldLeagues[i].lowerHeight, 10000));
		}
	}

    public static LeagueManager Get()
    {
        //if already stored, return stored reference
        if (leagueManager) return leagueManager;
        //if not, find a reference
        leagueManager = FindObjectOfType<LeagueManager>();
        return leagueManager;
    }

	IEnumerator Start()
	{
		while (OrbitCam.Exists() == false) yield return null;
		InitLeagueManager();
	}

	void InitLeagueManager() 
    { 
	    orbitCam = OrbitCam.Get();

		camDefaultEndClip = orbitCam.theCam.farClipPlane;

		//Get min and max height
		for(int i = 0; i< worldLeagues.Count; i++)
		{
			League league = worldLeagues[i];
			if (league.lowerHeight < minHeight) minHeight = league.lowerHeight;
			if (league.upperHeight > maxHeight) maxHeight = league.upperHeight;
			
			//Set above Color to the color of the league above, unless its the highest one, in which case it is the same as the fog color
			//league.localAtmosphere.aboveColor = i == 0 ? league.localAtmosphere.fogColor : worldLeagues[i - 1].localAtmosphere.fogColor;
			
			//set Below color to the fogcolor of the league below, unless its the last in which case the below color is the same as the fog color
			//league.localAtmosphere.belowColor = i == worldLeagues.Count - 1 ? league.localAtmosphere.fogColor : worldLeagues[i + 1].localAtmosphere.fogColor;
			
		}

		init = true;
	}

	void Update() {

		if (orbitCam == null) return;

		//Get current depth, clamp it to min and max leagues
		currentDepth = orbitCam.transform.position.y;
		currentDepth = Mathf.Clamp(currentDepth, minHeight, maxHeight);

		FindWorldAtmosphere();

		if (!atmosphereOverride) SetAtmosphere(worldAtmosphere);
	}


	/// <summary>
	/// Compares the two given peagues, returns 1 if league b is higher,
    /// returns -1 if league b is lower.
	/// </summary>
	int CompareLeagues(League a, League b){
		if (a.upperHeight> b.upperHeight) return -1;
		else return 1;	
	}

    League l;
	/// <summary>
	/// Applies the world atmosphere based on camera's position, not taking into account any local
	/// atmospheres.
	/// </summary>
	void FindWorldAtmosphere() {

        // Check if the camera is in one of the league's regions
        //foreach (League l in worldLeagues) 
        for ( int i = 0; i < worldLeagues.Count; i++ ) {

            l = worldLeagues[i];
			if (l.InThisLeague(currentDepth)) {

				//Set atmosphere if we aren't local
				worldAtmosphere = l.localAtmosphere;
				return;
			}
        }

		// If we've gotten to this point, it means the camera isn't within any leagues, so it must be between leagues.
		// Check each league to see what it's between.
		League aboveLeague = null;
		League belowLeague = null;

		//foreach (League l in worldLeagues) {
        for (int i = 0; i < worldLeagues.Count; i++ )
        {
            l = worldLeagues[i];
			if ( l.HigherThan(currentDepth)) aboveLeague = l;

			// If this league isn't higher than current depth, it means the
			// camera is between the last league and this
			else {
				belowLeague = l;
				break;
			}
		}

		if (aboveLeague == null || belowLeague == null) return;

		// Need to get the upper and lower bounds of the space BETWEEN the two leagues
		float upperBounds = aboveLeague.lowerHeight;
		float lowerBounds = belowLeague.upperHeight;
		float distance = Mathf.Abs(upperBounds - lowerBounds);
		float progress = Mathf.Abs(currentDepth - upperBounds);

		// Assuming the bottom of the upper league is 0, and the top of the lower league is 1,
		// get the normalized distance between the two.
		float lerpValue = progress / distance;

		worldAtmosphere = Calc.LerpAtmosphere(aboveLeague.localAtmosphere, belowLeague.localAtmosphere, lerpValue);
	}


	private Material skyBoxMat;

	private Material SkyBoxMat
	{
		get
		{
			if (skyBoxMat != null) return skyBoxMat;
			skyBoxMat = RenderSettings.skybox;
			if (skyBoxMat == null)
				RenderSettings.skybox = skyBoxMat = Resources.Load<Material>("skyBoxGradient");
			return skyBoxMat;
		}
	}
	
	/// <summary>
	/// This applies all the stats of the given atmosphere to the world.
	/// </summary>
	public void SetAtmosphere(Atmosphere myAtmosphere) 
    {
        //if (camFog == null) return;
        if (myAtmosphere == null) return;
	    if (!Camera.main) return;

        //Set current atmosphere (for debug purposes)
		currentAtmosphere = myAtmosphere;

		//Set Ambient light
		RenderSettings.ambientLight = myAtmosphere.ambientLightColor;
		RenderSettings.ambientIntensity = 1;

		//Set directional light
		if (directionalLight != null) {
			directionalLight.intensity = myAtmosphere.directionalLightIntensity;
			directionalLight.color = myAtmosphere.directionalLightColor;
		}

		if (directionalLight2 != null) {
			directionalLight2.intensity = myAtmosphere.directionalLightIntensity;
			directionalLight2.color = myAtmosphere.directionalLightColor;
		}

        //camFog.fogColor = myAtmosphere.fogColor;
	    Camera.main.backgroundColor = myAtmosphere.fogColor;
	  //  SkyBoxMat.SetColor("_Color1",  myAtmosphere.aboveColor);
	    //SkyBoxMat.SetColor("_Color2",  myAtmosphere.fogColor);
	    //SkyBoxMat.SetColor("_Color2",  myAtmosphere.belowColor);
	    //SkyBoxMat.SetFloat("_Intensity", myAtmosphere.fogExposure);

        if (orbitCam.cameraMode == CameraMode.Normal)
        {
			//camFog.startDistance = 	Mathf.Lerp(camFog.startDistance, myAtmosphere.fogDist.x, Time.deltaTime * 10);
			//camFog.endDistance = 	Mathf.Lerp(camFog.endDistance, myAtmosphere.fogDist.y, Time.deltaTime * 10);

			RenderSettings.fogColor =  myAtmosphere.fogColor;
			RenderSettings.fogStartDistance = Mathf.Lerp(RenderSettings.fogStartDistance, myAtmosphere.fogDist.x, Time.deltaTime * 10);
			RenderSettings.fogEndDistance 	= Mathf.Lerp(RenderSettings.fogEndDistance, myAtmosphere.fogDist.y, Time.deltaTime * 10);

			// Allow the camera to see as far as the fog end distance - min dist is whatever the camera's 
			// far clip plane is at start.
			orbitCam.defaultFarClip = Mathf.Clamp(camDefaultEndClip, myAtmosphere.fogDist.y, 1000);
        }


		// Apply bloom settings
	    if (!orbitCam) return;
		SENaturalBloomAndDirtyLens myBloom = orbitCam.GetComponentInChildren<SENaturalBloomAndDirtyLens>();
		if (myBloom) myBloom.bloomIntensity = myAtmosphere.bloomIntensity;
	}

	#region local atmospheres

    public void EnterLocalAtmosphere(LocalAtmosphere newAtmo)
    {
		StartCoroutine(SetLocalAtmo(newAtmo));
    }

	IEnumerator SetLocalAtmo(LocalAtmosphere newAtmo) {

		// Wait for initiailization to complete
		while (!init) 						yield return null;
		while (currentAtmosphere == null) 	yield return null;

		if (!localAtmospheres.Contains(newAtmo))
            localAtmospheres.Add (newAtmo);

		SortAtmospheres ();
		CheckLocalAtmospheres ();

		yield break;
	}

    public void ExitLocalAtmosphere(LocalAtmosphere newAtmo)
    {
		if (localAtmospheres.Contains (newAtmo))
			localAtmospheres.Remove (newAtmo);

		SortAtmospheres ();
		CheckLocalAtmospheres ();
    }

	/// <summary>
	/// Check the list to see which atmosphere is the priority.
	/// </summary>
	void CheckLocalAtmospheres() {

		//If there's no local atmospheres
		if (localAtmospheres.Count < 1) {
			StartCoroutine(TimedTransitionAtmosphere(currentAtmosphere, worldAtmosphere, mostRecentTransitionTime, false));
			return;
		}

		//If there are local atmosphere(s), transition to priority 0
		StartCoroutine(TimedTransitionAtmosphere(currentAtmosphere, localAtmospheres[0].myAtmosphere, localAtmospheres[0].transitionTime, true));
	}

	/// <summary>
	/// Sorts local atmospheres based on priority
	/// </summary>
	void SortAtmospheres() {
		if (localAtmospheres.Count < 1) return;
		localAtmospheres = localAtmospheres.OrderBy (x => x.priority).ToList ();
	}

	#endregion


    /// <summary>
    /// Creates a timed transition between these two atmospheres
    /// </summary>
    /// <param name="atmo1">The starting atmosphere</param>
    /// <param name="atmo2">The finishing atmosphere</param>
    /// <param name="time">Transition time</param>
    IEnumerator TimedTransitionAtmosphere(Atmosphere atmo1, Atmosphere atmo2, float time, bool localAtmo)
    {
        //set atmosphere override to true if entering a local atmosphere
		if (localAtmo) {
			atmosphereOverride = true;
			mostRecentTransitionTime = time;
		}

        float timer = 0;
        float lerp = 0;
        //the output atmosphere
        Atmosphere atmo;


        while (timer < time)
        {
            //if returning to world atmosphere, update it every frame b/c it can 
            //change if the player is moving
            if (!localAtmo) atmo2 = worldAtmosphere;

            //get the lerp atmosphere and set it
            lerp = timer / time;
            atmo = Calc.LerpAtmosphere(atmo1, atmo2, lerp);
            SetAtmosphere(atmo);

            timer += Time.deltaTime;
			yield return null;
        }

        //if exiting a local atmosphere, set atmosphere override to false
        if (!localAtmo) atmosphereOverride = false;

        yield break;
    }
}