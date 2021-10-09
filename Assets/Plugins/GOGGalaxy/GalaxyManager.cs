using UnityEngine;
using System.Collections;
using Galaxy.Api;
using System.Threading;
using UnityEngine.UI;

/// <summary>
/// Refactor of the galaxymanager c# class from Gog Galaxy team for use in unity.
/// Mainly converted any function that had a wait or sleep to an asynchronous call as to not block the main thread in unity
/// TODO(1/31/2017) Currently signed in once correctly, and has ever since only signed into SIGNED_IN_OFFLINE
/// </summary>
public class GalaxyManager : MonoBehaviour
{ 
    // Project specific data
    // ClientID and ClientSecret can be obtained from devportal.gog.com's game page

    const string ClientID = "49562197988936567";
    const string ClientSecret = "ebfda05fd8768fa093e6822be07fdff8745649263a5d32549463369fba8b1615";

	//Initializing variables for holding listener instances
	AuthenticationListener authListener;
	UserStatsAndAchievementsRetrieveListener statsAndAchievementsRetrieveListener;
	StatsAndAchievementsStoreListener statsAndAchievementsStoreListener;

	bool isSignedIn;

	public bool IsSignedIn{	get { return isSignedIn; } }

	GalaxyID userId;

	public GalaxyID UserId { get { return userId; } }

	string username;

	public string Username { get { return username; } }

    bool g_EverInitialized = false;

	bool achievementsAndStatsDefinitionsDownloaded = false;
    bool achievementWereStored = false;
    bool achievementsReset = false;
    // initializing variables for storing retrieved achievement data. Passed as refs in method params
    bool achUnlocked;
	uint achUnlockTime;

	// SIGNED_IN_OFFLINE is a special state where there is no connection to the Galaxy servers
	// however, if the user managed to successfully connect at least once before
	// you can access some of the Galaxy functions: stats, achievements and leaderboards
	// changes will be stored in local cache and synchronized after next sign in that
	// successfully connects to the Galaxy servers
	private enum GalaxyState
	{
		NOT_INITIALIZED,
		NOT_SIGNED_IN,
		SIGNING_IN,
		SIGNED_IN,
		SIGNED_IN_OFFLINE,
		AUTH_LOST,
		SIGN_IN_FAILED,
		INIT_FAILED,
	}

	GalaxyState state = GalaxyState.NOT_INITIALIZED;
	GalaxyState lastState = GalaxyState.NOT_INITIALIZED;

	void Awake ()
	{
		// Makes the Galaxy GameObject persistent across scenes and accessible through GameObject getters or other methods
		DontDestroyOnLoad (this.gameObject);
	    Init (ClientID, ClientSecret);
		
	}

    float intervalTime = 0;
    float checkInterval = 3;
    int checkTimes = 0;
    int maxCheckTimes = 3;
	void Update ()
	{
		// Call ProcessData() on every frame, it has a couple of ms overhead at best
        if(!g_EverInitialized)return;
		GalaxyInstance.ProcessData ();
        if (state == GalaxyState.SIGNED_IN)
        {
            if (authListener.LostAuth) // Check if we've lost Authentication
            {
                lastState = state;
                if (state != GalaxyState.AUTH_LOST)
                {
                    state = GalaxyState.AUTH_LOST;
                    intervalTime = 0;
                    checkTimes = 0;
                    OnDisable(); //Reset the galaxy 
                    SignIn();
                }
            }
        }
        else if(state == GalaxyState.SIGN_IN_FAILED)
         {
            intervalTime += Time.deltaTime;
            if (intervalTime > checkInterval)
            {
                if (checkTimes < maxCheckTimes)
                {
                    SignIn();
                    checkTimes++;
                }
                intervalTime = 0;
            }
        }
    }

	void OnDisable ()
	{
		// In Unity, it is advised to manually call Dispose() on each listener to ensure that Galaxy
		// instance shutdown goes smoothly
		if (IsSignedIn) {
			DisposeListeners ();
			Debug.Log ("Shutting down Galaxy instance");
			GalaxyInstance.Shutdown ();
       
            isSignedIn = false;
			username = null;
			userId = null;
			lastState = state;
			state = GalaxyState.NOT_INITIALIZED;
		}
	}

    private void OnDestroy()
    {
        g_EverInitialized = false;
        GalaxyInstance.Shutdown();
    }


    void Init (string cid, string cs)
	{
        if (g_EverInitialized) return;
       

		GalaxyInstance.Init (cid, cs);
		
        // initialize with this function for crossplay
        //GalaxyInstance.InitLocal(ClientID, ClientSecret, "Assets/Plugins/GOGGalaxy/Win64"); // for 64-bit games
		/*/// or
		GalaxyInstance.InitLocal(ClientID, ClientSecret, "Assets/Plugins/GOGGalaxy/Win32"); // for 32-bit games
        //*/
        
		// When not using exceptions, use IError.GetError() to check if the last synchronous call was successful
		if (!GetError ()) {
            Debug.Log("Succesful Init");
			InitializeListeners ();
			lastState = state;
			state = GalaxyState.NOT_SIGNED_IN;
         
            SignIn();
            g_EverInitialized = true;
            return;
		}
		lastState = state;
		state = GalaxyState.INIT_FAILED;

        Debug.Log("Init result: " + state);
	}

	void SignIn ()
	{
		if (authListener.Finished) {
			authListener.Reset ();
		}
      
        Debug.Log("Attempting Sign in ");   

		GalaxyInstance.User ().SignIn();
		lastState = state;
		state = GalaxyState.SIGNING_IN;
        StartCoroutine(SigningIn());
	}

    IEnumerator SigningIn() //Changed the sign in function to a coroutine in order to handle its asynchronous calls, and wait functions
    {
        while (!authListener.Finished)
        {
            GalaxyInstance.ProcessData();            
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("Auth Listener finished for sign in");
        if (authListener.Success)
        {
            Debug.Log("Auth Listener Sucess");
            if (GalaxyInstance.User().IsLoggedOn())
            {
                lastState = state;
                state = GalaxyState.SIGNED_IN;              
            }
            else
            {
                state = GalaxyState.SIGNED_IN_OFFLINE;
            }
          
            userId = GalaxyInstance.User().GetGalaxyID();
            username = GalaxyInstance.Friends().GetPersonaName();
            Debug.Log(Username + " ID: " + UserId);
            isSignedIn = true;
            if (!achievementsAndStatsDefinitionsDownloaded)
            {             
                StartCoroutine(RequestUserStatsAndAchievements());
            }

        }
        else
        {
            Debug.Log("Not logged into Galaxy or no connection");
            lastState = state;
            state = GalaxyState.SIGN_IN_FAILED;
            isSignedIn = false;
        }
        Debug.Log("Post Sign-in state: " + state.ToString());
    }

	// This function downloads definitions for statistics and achievements and their current states for current user
	// It stores them in local cache so that they can be accessed in offline mode. This needs to be called once for every Galaxy session
	IEnumerator RequestUserStatsAndAchievements ()
	{
		Debug.Log ("Requesting User stats:");
		GalaxyInstance.Stats ().RequestUserStatsAndAchievements ();
		while (!statsAndAchievementsRetrieveListener.Finished)
        {
			GalaxyInstance.ProcessData ();
            yield return new WaitForEndOfFrame();
		}

        achievementsAndStatsDefinitionsDownloaded = statsAndAchievementsRetrieveListener.Success;         
		
		Debug.LogWarning ("UserStats request result: " + achievementsAndStatsDefinitionsDownloaded);
        yield break;
    }

	// You need to call this to make the changes from the use SetAchievement, ClearAchievement, SetStatInt/Float persistent
	// You can call this function as much as you like, Galaxy libraries have their own internal logic for managing connection
	// to Galaxy backend servers for synchronizing local cache with persistent databases
	public void StoreStatsAndAchievements ()
	{
        achievementWereStored = false;

        if (achievementsAndStatsDefinitionsDownloaded)
        {
			Debug.Log ("StoreStatsAndAchievements");			
            StartCoroutine(StoreStatsLoop());
		}	
	}

    IEnumerator StoreStatsLoop()
    {
        statsAndAchievementsStoreListener.Reset();
        GalaxyInstance.Stats().StoreStatsAndAchievements();
        while (!statsAndAchievementsStoreListener.Finished)
        {
            GalaxyInstance.ProcessData();
            yield return new WaitForEndOfFrame();
        }
        
        achievementWereStored = statsAndAchievementsStoreListener.Success;
        Debug.Log("Stored Stats Succesfully.");
    }

	public bool GetAchievement (string apiKey)
	{
        //Debug.Log ("Getting Achievement" + apiKey);
        GalaxyInstance.Stats ().GetAchievement (apiKey, ref achUnlocked, ref achUnlockTime);
		Debug.Log ("Achievement: " + apiKey + " unlocked: " + achUnlocked + " on: " + FromUnixTime (achUnlockTime));
		return achUnlocked;
	}

	public void SetAchievement (string apiKey)
	{
		Debug.Log ("Setting Achievement: " + apiKey);
		GalaxyInstance.Stats ().SetAchievement (apiKey);
	}

	public void ClearAchievement (string apiKey)
	{
		Debug.Log ("Clearing Achievement: " + apiKey);
		GalaxyInstance.Stats ().ClearAchievement (apiKey);
	}

	public int GetStatInt (string apiKey)
	{
		Debug.Log ("Getting Stat Int: " + apiKey);
		return GalaxyInstance.Stats ().GetStatInt (apiKey);
	}

	public float GetStatFloat (string apiKey)
	{
		Debug.Log ("Getting Stat Float: " + apiKey);
		return GalaxyInstance.Stats ().GetStatFloat (apiKey);
	}

	public void SetStatInt (int value, string apiKey)
	{
		Debug.Log ("Setting Stat Int: " + apiKey + " to: " + value);
		GalaxyInstance.Stats ().SetStatInt (apiKey, value);
	}

	public void SetStatFloat (float value, string apiKey)
	{
		Debug.Log ("Setting Stat Float: " + apiKey + " to: " + value);
		GalaxyInstance.Stats ().SetStatFloat (apiKey, value);
	}

	public void UpdateAvgRateStat (string apiKey, float countThisSession, double sessionLength)
	{
		Debug.Log ("Setting Avg Rate: " + apiKey);
		GalaxyInstance.Stats ().UpdateAvgRateStat (name, countThisSession, sessionLength);
	}

	// You can reset all stats and achievements for the current user. Use this for testing purposes or if you want to
	// erase all profile progress data. This action cannot be reversed.
	public void ResetStatsAndAchievements ()
	{
        StartCoroutine(ResetStatsLoop());		
	}

    //
    IEnumerator ResetStatsLoop()
    {
        achievementsReset = false;
        GalaxyInstance.Stats().ResetStatsAndAchievements();
        while (statsAndAchievementsStoreListener.Finished)
        {
            yield return new WaitForEndOfFrame();
        }
        achievementsReset = statsAndAchievementsStoreListener.Success;
    }


	// Listeners can be initialized after successfull Init() call
	void InitializeListeners ()
	{
		authListener = new AuthenticationListener ();
		statsAndAchievementsRetrieveListener = new UserStatsAndAchievementsRetrieveListener ();
		statsAndAchievementsStoreListener = new StatsAndAchievementsStoreListener ();
	}

	void DisposeListeners ()
	{
		if (authListener != null) {
			authListener.Dispose ();
		}
		if (statsAndAchievementsRetrieveListener != null) {
			statsAndAchievementsRetrieveListener.Dispose ();
		}
		if (statsAndAchievementsStoreListener != null) {
			statsAndAchievementsStoreListener.Dispose ();
		}
	}


	public class AuthenticationListener : GlobalAuthListener
	{
		bool finished = false;
		bool success = false;
		bool lostAuth = false;

		public bool Finished { get { return finished; } }

		public bool Success { get { return success; } }

		public bool LostAuth { get { return lostAuth; } }

		// OnAuthSuccess() will appear also if there is no connection to GOG Galaxy servers but
		// the user managed to sign in successfully at least once before
		// In such offline state achievements, stats and leaderboards will be stored locally
		public override void OnAuthSuccess ()
		{
			Debug.Log ("OnAuthSuccess");
			finished = true;
			success = true;
		}

		public override void OnAuthFailure (FailureReason reason)
		{
			Debug.LogWarning ("OnAuthFailure: " + reason);
			finished = true;
			success = false;
		}

		// OnAuthLost() will be called if Galaxy cannot refresh the autorization of the user
		// This occurs very rarely, but it is advised to disable Galaxy features if it happens
		public override void OnAuthLost ()
		{
			Debug.LogWarning ("OnAuthLost");
			lostAuth = true;
		}

		public void Reset ()
		{
			finished = false;
			success = false;
		}
	}

	private class UserStatsAndAchievementsRetrieveListener : GlobalUserStatsAndAchievementsRetrieveListener
	{
		bool finished = false;
		bool success = false;

		public bool Finished {
			get { return finished; }	
		}

		public bool Success {
			get { return success; }	
		}

		public override void OnUserStatsAndAchievementsRetrieveSuccess (GalaxyID userID)
		{
			Debug.Log ("OnUserStatsAndAchievementsRetrieveSuccess");
			finished = true;
			success = true;
		}

		public override void OnUserStatsAndAchievementsRetrieveFailure (GalaxyID userID, FailureReason reason)
		{
			Debug.LogWarning ("OnUserStatsAndAchievementsRetrieveFailure: " + reason);
			finished = true;
			success = false;
		}
	}

	private class StatsAndAchievementsStoreListener : GlobalStatsAndAchievementsStoreListener
	{
        
		bool finished = false;
		bool success = false;

		public bool Finished {
			get { return finished; }	
		}

		public bool Success {
			get { return success; }	
		}

		public override void OnUserStatsAndAchievementsStoreSuccess ()
		{
			Debug.Log ("OnUserStatsAndAchievementsStoreSuccess");
			finished = true;
			success = true;
		}

		public override void OnUserStatsAndAchievementsStoreFailure (FailureReason reason)
		{
			Debug.LogWarning ("OnUserStatsAndAchievementsStoreFailure" + reason);
			finished = true;
			success = false;
		}

		public void Reset ()
		{
			finished = false;
			success = false;
		}
	}

	public static bool GetError ()
	{ 
		IError e = GalaxyInstance.GetError ();
		if (e != null) {
			Debug.LogError (e.ToString ());
			return true;
		}
		return false;
	}

	public static System.DateTime FromUnixTime (uint unixTime)
	{
		var epoch = new System.DateTime (1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
		return epoch.AddSeconds (unixTime);
	}
}