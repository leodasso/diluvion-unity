using UnityEngine;
using PathologicalGames;

/*

public class Module : MonoBehaviour 
{
	public Bridge bridge;
    public Station linkedStation;
    public StationStatsCollection collection;
	public bool onLine = false;

    void Awake()
    {
        AwakeModules();
    }

    protected virtual void AwakeModules()
    {    }

    protected float ApplyStatBonus(float bonus)
    {
        return 1 + bonus;
    }

    /// <summary>
    /// Set AI stats for modules
    /// </summary>
    /// <param name="person"></param>
    public virtual void SetAIStats(CaptainPersonality person)
    {    } 

	//Virtual for the purpose of overriding
	public virtual void OnSpawned()
	{	}

	//Virtual for the purpose of overriding
	public virtual void DisableModule()
	{
		onLine = false;
	}

	//Virtual for the purpose of overriding
	public virtual void EnableModule()
	{		
		onLine = true;		
	}

    //Virtual for the purpose of overriding, also they need to call base.Activate()
    public virtual void ActivateModule()
    {
        if (bridge == null) GetBridge();
        EnableModule();        
	}


	/// <summary>
	/// Detects if this instance should be activated based on the station status.
	/// For AI, this always returns true
	/// </summary>
	protected bool CanActivateFromStation() 
	{
		if (!bridge.IsPlayer()) return true;
		if (linkedStation == null) return false;
		//if (linkedStation.requireOfficer && linkedStation.officer == null) return false;

		return true;
	}


    /// <summary>
    /// Checks parents for bridge component.
    /// </summary>
    public Bridge GetBridge()
    {    
        if (bridge != null) return bridge;
		bridge = GetComponentInParent<Bridge>();        
		return bridge;
    }

    public bool IsPlayer()
    {
        if (GetBridge()==null) return false;
        return GetBridge().IsPlayer();
    }
}
*/