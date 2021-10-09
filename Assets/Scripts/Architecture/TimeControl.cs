using UnityEngine;
using System.Collections;
using Diluvion;
using Diluvion.SaveLoad;

/*
public class TimeControl : MonoBehaviour {

	public static TimeControl instance;

    public static float timeScale
    {
        get { return _timeScale; }
    }

    /// <summary>
    /// This combined with captain's time gives the true time scale.
    /// </summary>
    static float _timeScale = 1;

	public static bool hasPopup = false;

	/// <summary>
	/// The captain time usage - acts as a lerp between 0 and 1. For example, if player has 4 seconds of captain time
	/// and usage is at .5, then there's 2 seconds remaining.
	/// </summary>
	//[Range(0,1) ]public float captainTimeUsage = 1;

	//public bool usingCaptainTime;

	/// <summary>
	/// What will the timescale be when in captain time?
	/// </summary>
	//public float captainTimeSlowdown = .3f;

	/// <summary>
	/// How many seconds does it take for captain time to fully restore?
	/// </summary>
	//public float captainTimeRestore = 5;

	// Is captain time available? If it's restoring, it's not
	//public bool captainTimeAvailable = true;

	float counter;
	const float holdTime = .1f;

    bool returnedState = true;

    public static TimeControl Get() 
	{
		if (instance != null) return instance;
		instance = FindObjectOfType<TimeControl>();
		return instance;
	}

	/*
    public static float TotalCaptainTime()
    {
        float value = GameManager.Mode().defaultCaptTime;
        if (DSave.current == null) return value;

        value += (DSave.current.captainTimeUpgrades * GameManager.Mode().timePerCaptTimeUpgrade);
        return value;
    }
    

    
	// Update is called once per frame
	void Update () 
	{

        // If there's a popup, override any other time operations to pause the game.
        if ( hasPopup )
        {
            Time.timeScale = 0;
            if ( Application.isEditor ) Cursor.lockState = CursorLockMode.None;
            else Cursor.lockState = CursorLockMode.Confined;
            return;
        }

        if ( GameManager.State() != GameState.Running ) {
            usingCaptainTime = false;
            return;
        }

		// reset counter of how long button has been held
		if (!usingCaptainTime) counter = 0;
		else counter += Time.unscaledDeltaTime;

		// If guage is fully depleted, enter cooldown
		if (captainTimeUsage < .05f) captainTimeAvailable = false;

		if (!CanSlowTime() && captainTimeUsage < 1)
			
			// Restore captain time magic
			captainTimeUsage += Time.unscaledDeltaTime / captainTimeRestore;



        if (CanSlowTime())
        {
            // Deplete captains time magic
            if (returnedState)
            {
                AKMusic.Get().SetSideViewState(Combat_State.Captain_Time);
                returnedState = false;
            }

            captainTimeUsage -= Time.unscaledDeltaTime / TotalCaptainTime();
            // slows down time
            Time.timeScale = Mathf.Lerp(Time.timeScale, captainTimeSlowdown, Time.unscaledDeltaTime * 5);
        }
        else
        {
            Time.timeScale = Mathf.Lerp(Time.timeScale, timeScale, Time.unscaledDeltaTime * 3);
            if (!returnedState)
            {
                AKMusic.Get().ReturnCombatState(false);
                returnedState = true;
            }
        }

		if ( captainTimeUsage >= 1) captainTimeAvailable = true;
	}

    /// <summary>
    /// Safe way of requesting time scale be modified. Won't allow modification if in a popup, pause, etc.
    /// </summary>
    /// <param name="newTimeScale"></param>
    /// <param name="overrideChecks">Directly sets the time scale without checking for pause menu, popups, etc.</param>
    public static void SetTimeScale(float newTimeScale, bool overrideChecks = false)
    {
        if (!overrideChecks)
        {
            if ( hasPopup ) return;
            if ( GameManager.State() != GameState.Running ) return;
        }

        _timeScale = newTimeScale;
    }

    bool CanShowVisualsI()
    {
        if (usingCaptainTime && captainTimeAvailable && captainTimeUsage > 0 &&
            OrbitCam.CamMode() != CameraMode.Normal && counter >= holdTime)
            return true;

        return false;
    }

	/// <summary>
	/// Determines whether this instance can display visuals indicating that captain's mode is activated.
	/// The difference between this and Can Slow Time is this being true won't actually slow time or deplete
	/// the guage. It's for when the player's holding down the button but transition is still taking place.
	/// </summary>
	public static bool CanShowVisuals()
	{
        if (!Get()) return false;
        return Get().CanShowVisualsI();
	}

    bool CanSlowTimeI()
    {
        if (usingCaptainTime && captainTimeAvailable && captainTimeUsage > 0 &&
            OrbitCam.CamMode() == CameraMode.Interior && counter >= holdTime)
            return true;

        return false;
    }

	/// <summary>
	/// Determines whether this instance can slow time.
	/// </summary>
	public static bool CanSlowTime() 
	{
        if (!Get()) return false;
        return Get().CanSlowTimeI();
	}
}
*/
