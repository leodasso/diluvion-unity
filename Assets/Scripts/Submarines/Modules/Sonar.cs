using UnityEngine;
using System.Collections.Generic;
using PathologicalGames;
using SpiderWeb;
using Diluvion;
using HeavyDutyInspector;

/*
[System.Serializable]
public class DetectionRatio
{
    [DynamicRange("level2Range", 1)]
    public float level1Range;
    [DynamicRange("level3Range", "level1Range")]
    public float level2Range;
    [DynamicRange( 0.1f, "level2Range")]
    public float level3Range;

	[Button("Update Curve", "UpdateCurve", true)]
	public bool hidden;

    //Temporarily left public for EZ inspection
    public AnimationCurve myCurve;
    
    public DetectionRatio ()
    {
        level1Range = 1;
        level2Range = 0.5f;
        level3Range = 0;
        UpdateCurve();
    }


    public DetectionRatio(DetectionRatio dr)
    {
        level1Range = dr.level1Range;
        level2Range = dr.level2Range;
        level3Range = dr.level3Range;
        UpdateCurve();

    }

    public DetectionRatio(DetectionRatio dr, float percentage)
    {
        level1Range = Mathf.Clamp(dr.level1Range * percentage, 0, 4);
        level2Range = Mathf.Clamp(dr.level2Range * percentage, 0, 4);
        level3Range = Mathf.Clamp(dr.level3Range * percentage, 0, 4);
        UpdateCurve();
    }

    public DetectionRatio(Vector3 v3)
    {
        level1Range = v3.x;
        level2Range = v3.y;
        level3Range = v3.z;
        UpdateCurve();
    }

    public DetectionRatio(float one, float two, float three)
    {
        level1Range = one;
        level2Range = two;
        level3Range = three;
        UpdateCurve();
    }

    //Using Vectors because of vector3.Lerp and other V3 functions
    //Converting from Ratio to Vector
    public Vector3 ToVector()
    {
        return new Vector3(level1Range, level2Range, level3Range);
    }

    //Converts from Vector to Ratio
    public void SetRanges(Vector3 v3)
    {
        level1Range = v3.x;
        level2Range = v3.y;
        level3Range = v3.z;
        UpdateCurve();
    }

    /// <summary>
    /// Returns the Detection Value at objectDistance out of maxDistance
    /// </summary>
    public float DetectionValue(float objectDistance, float maxDistance)
    {        
        if (objectDistance > maxDistance) return 0;//If we get a value greater than 1 its out of bounds

        float normDistance = objectDistance / maxDistance;//Get the normalized Distance to max
       // Debug.Log("normalized Distance = " + normDistance);
        float value = myCurve.Evaluate(normDistance);//Evaluate the value at the normal Distance as time

        return value;//return the detection value at the distance
    }

    //Updates the saved Curve
    public void UpdateCurve()
    {
        List<Keyframe> allKeys = new List<Keyframe>();

        allKeys.AddRange(AnimationCurve.Linear(0, 4, level3Range, 3).keys);
        allKeys.AddRange(AnimationCurve.Linear(level3Range, 3, level2Range, 2).keys);
        allKeys.AddRange(AnimationCurve.Linear(level2Range, 2, level1Range, 1).keys);
        allKeys.AddRange(AnimationCurve.Linear(level1Range, 1, 1, 0).keys);

        AnimationCurve curve = new AnimationCurve(allKeys.ToArray());     

       curve.UpdateAllLinearTangents();
       myCurve = curve;
    }
}
	

[RequireComponent(typeof(SphereCollider))]
public class Sonar : Module
{
	public SonarModuleDataObject sonarDataPrefab;
    [SerializeField]
	public SonarModuleData sonarData;
    public SonarStats ourSignature;
	public Dictionary<SonarStats, float> sonarContacts = new Dictionary<SonarStats, float>();
    public SonarPing sonarPingObj;
	public bool showPassiveRanges;
	public bool showPingRanges;
	public bool hasPlayerSignature = false;//Bool for AI that flips when it adds/removes the player from its contact list

    SonarPing npcSonarPref;
    SonarPing playerSonarPref;
	Color longRangeColor = new Color(.5f, 1, .65f);
	Color shortRangeColor = new Color(1, .6f, .6f);
   
    SonarModuleData realModuleData;
    List<SonarStats> allHits;				//All sonar stats within range.  NOT exposed to player
	bool updatedSonar = false;

    float currentSightRange;
    Camera theCam;
    Fog theFog;
    LayerMask sonarMask;
    SphereCollider thisCollider;
    bool pingCharged = false;
	float pingCount;
	float listenCount;
	bool listening;
	bool playerIsMe = false;
    OrbitCam oCam;
    bool sonarDataInit = false;
	float LOSerrorDist = 4;
    string chargeStatName = "Station Stats/SonarCharge";
    string rangeStatName = "Station Stats/SonarRange";
	GameObject pingFX;
    public bool wantRepaint = false;


    /*
    public void OnDrawGizmos()
    {
        if(showPassiveRanges)
            DrawSonarGizmosForMaxrange(sonarData.passiveMaxRange, sonarData.currentRatio);
        if (showPingRanges)
            DrawSonarGizmosForMaxrange(sonarData.maxSonarRange, sonarData.currentRatio);
        
    }

    void DrawSonarGizmosForMaxrange(float range, DetectionRatio ratio)
    {
    
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, range * ratio.level3Range);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range * ratio.level2Range);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, range * ratio.level1Range);
    }


    #region Init
    //Module activation
    public override void ActivateModule()
    {
	
        //Get the layer we want to ignore
        sonarMask = Calc.IncludeLayer(new string[] { "Terrain", "BlockSonar" });

        //set layer to sonar 
        gameObject.layer = LayerMask.NameToLayer("Sonar");

        base.ActivateModule();

        playerIsMe = bridge.IsPlayer();

        //if (playerIsMe && linkedStation.officer == null)
		if ( !CanActivateFromStation() )
            DisableModule();
      
        FindGUI();
		//accuracy = 15;
		listening = true;
		theCam = Camera.main;
        theFog = theCam.GetComponent<Fog>();
        oCam = OrbitCam.Get();
        ResetSonarContacts();

        if (IsPlayer()) 
			pingFX = Resources.Load("ping fx") as GameObject;
	}


    void ResetSonarContacts()
    {
        sonarContacts = new Dictionary<SonarStats, float>();
        allHits = new List<SonarStats>();
    }

    void OnEnable()
    {
        ResetSonarContacts();
    }


    //Safe Sonardata GetSetter
    public SonarModuleData SonarData()
    {        
        if (sonarDataPrefab == null)
        {
            sonarDataPrefab = (SonarModuleDataObject)Resources.Load("default sonar");      
        }

        if (sonarDataPrefab != null)
        {
            if (!sonarDataInit)
            {
			//	Debug.Log("Setting sonar data prefab on" + ourSignature.info.displayName);
                sonarData = new SonarModuleData(sonarDataPrefab.sonarData);
                sonarDataInit = true;
            }
        }

        if (sonarData == null)
            sonarData = new SonarModuleData();


        return sonarData;
    }

	public void RefreshSonarData() 
	{
		sonarData = new SonarModuleData(sonarDataPrefab.sonarData);
		sonarDataInit = true;
	}

    public void SetupSonarSignature(SonarStats stats)
    {
        ourSignature = stats;
        bridge = GetBridge();
        if (bridge == null) return;
        playerIsMe = bridge.IsPlayer();
        ourSignature.playerIsMe = bridge.IsPlayer();
    }

    void SetupListeningSphere(float baseSightRadius)
    {
        //Debug.Log("attempting to set Radius to " + radius);
        if (thisCollider == null)
        {
            if (GetComponent<SphereCollider>())
                thisCollider = GetComponent<SphereCollider>();
            else
                thisCollider = gameObject.AddComponent<SphereCollider>();
        }

       // Debug.Log("sonar collider's radius is : " + thisCollider.radius + ", set to "  + radius + " ? ");
        if (thisCollider.radius == baseSightRadius) return;
        if (thisCollider.isTrigger == false)
            thisCollider.isTrigger = true;

        float  bonusListeningRadius = baseSightRadius * realModuleData.currentEfficiency;
        thisCollider.radius = baseSightRadius;
    }

    #endregion

    #region Crew and Stats

    public SonarModuleData SetRealSonarData(SonarModuleData data)
    {
        SonarModuleData smd = new SonarModuleData(data);
        realModuleData = smd;
        return smd;
    }

    //Updates the stats of the module
    public override void SetStats()
    {  
        base.SetStats();
        SetRealSonarData(SonarData());
        realModuleData.SetChargeTime(SonarData(), ApplyStatBonus( linkedStation.GetStat(chargeStatName)));
        realModuleData.SetRange(SonarData(), ApplyStatBonus(linkedStation.GetStat(rangeStatName)));
        
    }


    public override void SetAIStats(CaptainPersonality person)
    {
        base.SetAIStats(person);

        SetRealSonarData(SonarData());
        realModuleData.SetChargeTime(SonarData(), ApplyStatBonus(person.GetStat(chargeStatName)));
        realModuleData.SetRange(SonarData(), ApplyStatBonus(person.GetStat(rangeStatName)));
    }
    #endregion

    void OnTriggerEnter(Collider other)
	{
        if (allHits == null) allHits = new List<SonarStats>();
        if (!other.GetComponent<SonarStats>()) return;
        if (other.GetComponent<SonarStats>().OurSignature(ourSignature)) return;
        //if we dont already have the other signal, add it
        if(!allHits.Contains(other.GetComponent<SonarStats>()))             
            allHits.Add(other.GetComponent<SonarStats>());      
	}

    /*
    #region ContactList Manipulators
    /// Checks through all targets in current hits. Adds targets to TargetList if they
    /// are detected.
    void ListenCheck()
	{
        List<SonarStats> removeList = new List<SonarStats>();
        List<SonarStats> checkList = new List<SonarStats>();

        checkList = ShipTools.OrderNearest(allHits, transform);
        

        foreach (SonarStats ss in checkList)
        {
			// If the sonar stats no longer exists, mark for remove and continue
			// to the next item in the loop.
			if (ss == null||!ss.gameObject.activeInHierarchy)//Remove if null and void
            {            
				removeList.Add(ss);              
				continue;
			}


			if (Calc.IsLOS(transform.position, ss.transform.position, sonarMask, LOSerrorDist))//Only adds contacts that are in line of sight
            {
                //Debug.Log(ss.name + " is in LOS, Adding to Dict");
                if (!sonarContacts.ContainsKey(ss))
                    AddToSonarContacts(ss, 0);

                //Takes the current value of the contact and sets it to an appropriate value based on distance
                sonarContacts[ss] = Discovered(ss, sonarContacts[ss]);
            }
            else
                sonarContacts[ss] = 0;        
        }

        foreach (SonarStats ss in removeList)
        {
            allHits.Remove(ss);
            RemoveFromSonarContacts(ss);
        }

        updatedSonar = true;
	}


    List<SonarStats> notifiedLongRangeTorps = new List<SonarStats>();
    List<SonarStats> notifiedCloseRangeTorps = new List<SonarStats>();
    /// <summary>
    /// Checks the content of the sonarcontacts to see if we they are still visible
    /// </summary>
    /// 
    /*
    public bool CheckTargets()
    {
        List<SonarStats> targetsToRemove = new List<SonarStats>();
        if (sonarContacts == null) return false;
        if (sonarContacts.Count < 1) return false;

        SonarStats newLongRangeTorpedo = null;
        SonarStats newCloseRangeTorpedo = null;
        //We remove missing objects first, before evauluating if there are any legal targets
        foreach (KeyValuePair<SonarStats, float> sskvp in sonarContacts)
        {
            if (sskvp.Key == null)
                continue;
            
            //Checks for all Destroyed/null/bad values
            if (sskvp.Key.gameObject == null || !sskvp.Key.gameObject.activeInHierarchy|| !sskvp.Key.enabled)
            {    
				//Debug.Log( sskvp.Key.info.displayName + " was removed from " + ourSignature.info.displayName + " because it's null or inactive.");
                targetsToRemove.Add(sskvp.Key);      //Removes from local List   
                continue;
            }
          
            //checks to see if we still ahve LOS
			if (!Calc.IsLOS(transform.position, sskvp.Key.transform.position, sonarMask, LOSerrorDist))
			{            
				//Debug.Log( sskvp.Key.info.displayName + " was removed from " + ourSignature.info.displayName + " because line of sight was lost.");
				targetsToRemove.Add(sskvp.Key);      //Removes from local List   
				continue;
			}

			//Checks for all Destroyed/null/bad values
			if (sskvp.Value <= 0 )
			{     
				//Debug.Log( sskvp.Key.info.displayName + " was removed from " + ourSignature.info.displayName + " because it's value is below 0.");
				targetsToRemove.Add(sskvp.Key);      //Removes from local List   
				continue;
			}

			//Checks for all Destroyed/null/bad values
			if (sskvp.Key.OurSignature(ourSignature))
			{           
				//Debug.Log( sskvp.Key.info.displayName + " was removed from " + ourSignature.info.displayName + " because ITS MEEEEEEEEEEEEE");
				targetsToRemove.Add(sskvp.Key);      //Removes from local List   
				continue;
			}

            if (!playerIsMe) continue;
           
            UpdateVisibleObject(sskvp.Key, sskvp.Value);
            if (!HostileTorpedo(sskvp.Key)) continue;
           
            if (Calc.WithinDistance(70, transform, sskvp.Key.transform))
            {
                if (!newCloseRangeTorpedo && !notifiedCloseRangeTorps.Contains(sskvp.Key))
                {
                    newCloseRangeTorpedo = sskvp.Key;
                    notifiedCloseRangeTorps.Add(sskvp.Key);                  
                }
            }
            else
            {
                if (!newLongRangeTorpedo && !notifiedLongRangeTorps.Contains(sskvp.Key))
                {
                    newLongRangeTorpedo = sskvp.Key;
                    notifiedLongRangeTorps.Add(sskvp.Key);                  
                }
            }                                      
        }

        if (playerIsMe)
            BroadcastTorpedo(newCloseRangeTorpedo, newLongRangeTorpedo);

        if (targetsToRemove.Count < 1) return true;

        foreach (SonarStats ss in targetsToRemove)                 
            RemoveFromSonarContacts(ss);
        

        return true; //Returns true if there are targets
    }

    float returnFloat;
    float range;
    float NormalizedDistance(SonarStats stats)
    {
        // TODO
        //if (bridge.wep) range = 50;
        //range = bridge.wep.BoltRange() * 2;
        //else
            range = 80;

        if (Calc.WithinDistance(range , transform, stats.transform))
            return 1;
        else if (Calc.WithinDistance(150, transform, stats.transform))
            return 0.3f;
        else
            return 0.1f;
    }

    /// <summary>
    /// a check to see if the input stat i s above a certain contact level
    /// </summary>
	public bool IsContactAbove(float contactLevel, SonarStats stats) {

		if (!sonarContacts.ContainsKey(stats)) return false;

		if (sonarContacts[stats] < contactLevel) return false;    

        return true;
	}

    /*
    /// <summary>
    /// The ping reaction callback thing
    /// </summary>
    /// <param name="stats"></param>
    public void PingTarget(SonarStats stats, float maxDistance)
    {
        if (stats == null) return;
        if(!stats.enabled) return;
        if (!Calc.IsLOS(transform.position, stats.transform.position, sonarMask, LOSerrorDist)) return;      

        if (stats.OurSignature(ourSignature)) return;
        float discoveryLevel = DiscoveryLevelByDistance(stats, maxDistance);

        if (!sonarContacts.ContainsKey(stats))//Immidately add to sonarcontacts if we dont have it
            AddToSonarContacts(stats, discoveryLevel);
        else
            sonarContacts[stats] = discoveryLevel;//Update the the sonarContact's level based on sonar max range

        if (!allHits.Contains(stats))//Add to all hits if we dont have it
            allHits.Add(stats);

        if (playerIsMe)
            UpdateVisibleObject(stats, sonarContacts[stats]);//update the visible object to the ping value
    }*/

/*
/// <summary>
/// Wrapper function to check every added sonarStats for if it is the player, so we dont have to worry about it for AI later
/// </summary>
/// <param name="The Sonar Stats to Add"></param>
/// <param name="Visibility score"></param>
public void AddToSonarContacts(SonarStats stats, float visibility)
{     
if (stats.playerIsMe)
    hasPlayerSignature = true;
sonarContacts.Add(stats, visibility);
}*/

/*
/// <summary>
/// Wrapper function to check every added sonarStats for if it is the player, so we dont have to worry about it for AI later
/// </summary>
public void RemoveFromSonarContacts(SonarStats stats)
{       
    if (stats.playerIsMe) 
        hasPlayerSignature = false;

    if (playerIsMe)
    {
        if (notifiedCloseRangeTorps.Contains(stats))
            notifiedCloseRangeTorps.Remove(stats);
        if(notifiedLongRangeTorps.Contains(stats))
            notifiedCloseRangeTorps.Remove(stats);
        DUI.RemoveVisible(stats);
    }

    if (sonarContacts.ContainsKey(stats))
        sonarContacts.Remove(stats);
}

//Passive Listening Discovery
float Discovered(SonarStats target, float ssLevel)
{
    if (realModuleData == null) return 0;
    float distanceValue = DiscoveryLevelByDistance(target, realModuleData.passiveMaxRange);// a value between 0 and 4
    //Passive listening Set
    if (ssLevel <= distanceValue)
        return distanceValue;
    else 
        return ssLevel- realModuleData.trackingFalloff;
}
*/
//#endregion

#region Actions

/// Spawns and re-initializes the ping
/// 
/*
public void sPing(float power)
{
    if (!onLine) return;

    if (!pingCharged) return;

    if (SonarPingObj() == null) return;

    float clampedAmp = Mathf.Clamp(power, 0, realModuleData.pingChargeSeconds) / realModuleData.pingChargeSeconds;

    Color ourPingColor = Color.Lerp(shortRangeColor, longRangeColor, clampedAmp);
    float pingFrequency = Mathf.Lerp(realModuleData.shortRangePingSpeed, realModuleData.longRangePingSpeed, clampedAmp);
    float distancePower = Mathf.Lerp(realModuleData.maxSonarRange * 0.3f, realModuleData.maxSonarRange, clampedAmp);

    SpiderSound.TweakRTPC("Ping_Power", distancePower,gameObject);
    SpiderSound.MakeSound("Play_Sonar_Ping", gameObject);
    if (IsPlayer() && pingFX)
    {
        Transform pingFXInstance = PoolManager.Pools["WorldPool"].Spawn(pingFX.transform, transform.position, Quaternion.identity, transform);
        PoolManager.Pools["WorldPool"].Despawn(pingFXInstance, 5);
        LitPeripheralMachine.Get().SetPing();
    }
    //spawn the ping object
    Transform thePing = GetPool().Spawn(SonarPingObj().transform, transform.position, Quaternion.identity);
    SonarPing pingObj = thePing.GetComponent<SonarPing>();
   // pingObj.SetQuiet(quietPing);

    //set color
    pingObj.SetColor(ourPingColor);
    pingObj.StartPing(clampedAmp, distancePower, pingFrequency, this);
    pingCharged = false;

    bridge.crewManager.PushHashtagToOfficer("ping", SkillType.Sonar);
}



public float MaxPingPower()
{
    return realModuleData.pingChargeSeconds;
}

//Charges the ping
public float PingCharge()
{
    return pingCount / realModuleData.pingChargeSeconds;
}

//returns the private bool pingCharged
public bool PingCharged()
{

    return pingCharged;
}
#endregion


bool HostileTorpedo(SonarStats torpedo)
{
    if (torpedo.BelongsTo(ourSignature)) return false;
    if (!KeywordTools.HaveTag("Signaltypes/torpedo", torpedo.info.ObjectTags())) return false;
    Targeting torpedoTarget = torpedo.GetComponent<Targeting>();
    if (torpedoTarget.target != ourSignature) return false;
    return true;
}

/// <summary>
/// Handles the sound and chatter for the incoming torpedo warnings, will only fire one each sonar tick, priotitizing new close range signals
/// </summary>
/// <param name="closeRange"></param>
/// <param name="longRange"></param>
void BroadcastTorpedo(SonarStats closeRange, SonarStats longRange)
{

    if (closeRange == null && longRange == null) return;

    //TODO TOrpedo Warning Color interior
    WarnInterior();

    if (closeRange!=null)
        SpiderSound.MakeSound("Play_Contact_Torpedo", closeRange.gameObject);
    else if(longRange!=null)
        SpiderSound.MakeSound("Play_Contact_Torpedo_Light", longRange.gameObject);
    ///  Debug.Log(checkTorp.name + " is a torpedo");
    if (bridge)
        if (bridge.crewManager)
            bridge.crewManager.BroadcastHashtag("torpedo");
        else
            Debug.Log("no crewManager on bridge");
    else
        Debug.Log("no bridge on sonar");
}

/// <summary>
/// Sends a signal to the interior (if found) that there's a torpedo coming
/// </summary>
void WarnInterior()
{
    if ( !bridge ) return;
    if ( !bridge.GetComponent<SideViewerStats>() ) return;
    if ( bridge.GetComponent<SideViewerStats>().intMan == null ) return;

    bridge.GetComponent<SideViewerStats>().intMan.ShowWarningLights();
}


#region UIManipulation

void UpdateVisibleObject(SonarStats ss, float value)
{     
    DUI.UpdateVisible(ss, value, NormalizedDistance(ss));
}


#endregion

#region Checks



//Checks if the current sight range based on the fog level, extend slightly beyond what we can see
public float GetSightRange()
{
    if (realModuleData == null) return 50;
    float currentFogLevel = theFog.endDistance * realModuleData.visualRangePercentage;
    if (currentSightRange != currentFogLevel && oCam.cameraMode == CameraMode.Normal)
        currentSightRange = currentFogLevel;
    //Debug.Log(transform.parent.parent.name + "Sonar Got: " + currentSightRange + " as sight range");
    return currentSightRange;
}

/// <summary>
/// Checks to see if listencheck has been performed lately, returns true, then switches it to false;
/// </summary>
public bool UpdatedSonar()
{
    if (updatedSonar == true)
    {
        updatedSonar = false;
        return true;
    }
    return false;
}


/// <summary>
/// Sight Range Check
/// </summary>
bool InSightRange(Transform target)
{
    if (playerIsMe)
        return InSightRangeCam(target);
    else
        return InSightRangeShip(target);
}

/// <summary>
/// SightRangeCheck for Player
/// </summary>
bool InSightRangeCam(Transform target)
{     
    return Calc.WithinDistance(GetSightRange(), theCam.transform, target.transform);
}

/// <summary>
/// SightRangeCheck for NPCS
/// </summary>
bool InSightRangeShip(Transform target)
{
    return Calc.WithinDistance(GetSightRange(), transform, target.transform);
}


#endregion
/*
#region Tools

//Updates the passive listening based on visual range
void UpdateMaxPassiveListening()
{
    if(realModuleData == null) { Debug.Log("SONAR NOT SET UP ON: ", gameObject); return; }
    realModuleData.passiveMaxRange = GetSightRange();
    SetupListeningSphere(realModuleData.passiveMaxRange);


}

//Returns the discovery level by distance to the target based on the maxDistance
float DiscoveryLevelByDistance(SonarStats target, float maxDistance)
{
    if (realModuleData == null) { Debug.Log("Moduledata not created"); return 0; }
    float targetDist = (transform.position - target.transform.position).magnitude;
    float detectionValue = Mathf.Clamp(realModuleData.currentRatio.DetectionValue(targetDist, maxDistance) + target.SignalBonus(), 0, 3.99f);
   // Debug.Log("Discovered: " + target.info.displayName + " at  " + targetDist + "/" + maxDistance + "= " + detectionValue);
    return detectionValue;
}


//Safe Get off the sonar ping obj for 
public SonarPing SonarPingObj()
{
    if (PrefabDicts.Get() == null) return null;
    if (playerSonarPref == null) playerSonarPref = PrefabDicts.Get().GameObjectOfKey("playerPing").GetComponent<SonarPing>();
    if (npcSonarPref == null) 	 npcSonarPref = PrefabDicts.Get().GameObjectOfKey("pingObj").GetComponent<SonarPing>();

    if (IsPlayer())
    {
        if (sonarPingObj != playerSonarPref)
            sonarPingObj = playerSonarPref;
    }
    else
    {
        if (sonarPingObj != npcSonarPref)
            sonarPingObj = npcSonarPref;
    }
    return sonarPingObj;
}
#endregion


#region List Filters
/// <summary>
/// Gets a list of the Vague Contacts 
/// </summary>
public List<SonarStats> VagueContacts()
{
    List<SonarStats> returnList = new List<SonarStats>();
    foreach (KeyValuePair<SonarStats, float> sskvp in sonarContacts)
    {
        if (sskvp.Value < 1 && sskvp.Value > .4f)
        {
            returnList.Add(sskvp.Key);
        }
    }
    return returnList;
}

/// <summary>
/// Gets a list of the Vague Contacts override for checking if player is in this list
/// </summary>
public List<SonarStats> VagueContacts(out bool isPlayerInThisPart)//TODO can change this into TARGET contact instead later
{
    List<SonarStats> returnList = new List<SonarStats>();
    isPlayerInThisPart = false;
    foreach (KeyValuePair<SonarStats, float> sskvp in sonarContacts)
    {
        if (sskvp.Value < 1 && sskvp.Value > .4f)
        {
            if (sskvp.Key.playerIsMe)
                isPlayerInThisPart = true;
            returnList.Add(sskvp.Key);
        }
    }
    return returnList;
}


List<SonarStats> returnList = new List<SonarStats>();
public List<SonarStats> ContactsAbove(float input)
{
    returnList = new List<SonarStats>();

    foreach (KeyValuePair<SonarStats, float> sskvp in sonarContacts)
    {
        if (sskvp.Key.BelongsTo(ourSignature)) continue;
        if (sskvp.Value > input)
        {
            returnList.Add(sskvp.Key);
        }
    }
    return returnList;
}


/// <summary>
/// Gets a list of the Visible Contacts
/// </summary>
public List<SonarStats> VisibleContacts()
{
    List<SonarStats> returnList = new List<SonarStats>();

    foreach (KeyValuePair<SonarStats, float> sskvp in sonarContacts)
    {
        if (sskvp.Value > 1)
        {
            returnList.Add(sskvp.Key);
        }
    }
    return returnList;
}

/// <summary>
/// Gets a list of the Visible Contacts Override, check if player is in this list
/// </summary>
public List<SonarStats> VisibleContacts(out bool isPlayerInThisPart)//TODO can change this into TARGET contact instead later
{
    List<SonarStats> returnList = new List<SonarStats>();
    isPlayerInThisPart = false;
    foreach (KeyValuePair<SonarStats, float> sskvp in sonarContacts)
    {
        if (sskvp.Value > 1)
        {
            if (sskvp.Key.playerIsMe)
                isPlayerInThisPart = true;
            returnList.Add(sskvp.Key);
        }
    }
    return returnList;
}



SonarInfo ssToCheck;
KeyValuePair<SonarStats, float> closestValuePair = new KeyValuePair<SonarStats, float>(null, 0);
public SonarStats GetClosestStatsWithTags(List<Keyword> tags, List<Keyword> excludeTags, out float visibilty, bool debug = false)
{

    visibilty = 0;
    closestValuePair = new KeyValuePair<SonarStats, float>(null, 0);
    if (sonarContacts == null) { return null; }
    if (sonarContacts.Count < 1) { return null; }

    float shortestDistance = 99999999999;

    foreach (KeyValuePair<SonarStats, float> sskvp in sonarContacts)
    {
        if (sskvp.Key == null) continue;
        ssToCheck = sskvp.Key.info;
        if (ssToCheck == null) continue;
        if (ssToCheck.ObjectTags().Contains("Signaltypes/wreck"))
        {
            if (debug)
                Debug.Log(ssToCheck.displayName + " is a wreck, continue");
            continue;
        }
        if (ssToCheck.HasTags(excludeTags))
        {
            if (debug)
                Debug.Log(ssToCheck.displayName + " is an excluded tag, continue");
            continue;
        }
        if (ssToCheck.HasTags(tags))
        {
            float statDistance = (sskvp.Key.transform.position - transform.position).sqrMagnitude;
            if (statDistance < shortestDistance)
            {
                closestValuePair = sskvp;
                shortestDistance = statDistance;
            }         
        }
    }

    visibilty = closestValuePair.Value;

    if (closestValuePair.Key!=null)
        if (debug)
            Debug.Log("Closest object matching my tags: " + closestValuePair.Key.name, gameObject);
    return closestValuePair.Key;    
}


//Checks for any Torpedo in my current visibile contacts
public SonarStats CheckForTorpedo()
{
    foreach (SonarStats ss in VisibleContacts())
    {
        if (KeywordTools.HaveTag("Signaltypes/torpedo", ss.info.ObjectTags())) return ss;		
    }
    return null;
}
#endregion

/*
/// <summary>
/// Removes all sonar contacts and UI 
/// </summary>
void ClearAllSonarContacts()
{
    if ( sonarContacts.Count < 1 ) return;

    foreach ( KeyValuePair<SonarStats, float> sskvp in sonarContacts )
    {
        RemoveFromSonarContacts(sskvp.Key);
    }

    ResetSonarContacts();
}*/

/*
void Update()
{
    if (!onLine) {

        ClearAllSonarContacts();
        return;
    }
    if (!sonarDataInit) return;

    if(listening)
    {
        if(listenCount < 1)
        {
            listenCount += Time.deltaTime;
        }
        else
        {
            UpdateMaxPassiveListening();
            ListenCheck();
            listenCount = 0;
            CheckTargets();               
            wantRepaint = true;                
        }
    }

    if (realModuleData == null) return;

    //charge the ping
    if(!pingCharged)
    {
        if(pingCount < realModuleData.pingCooldown)
        {
            pingCount += Time.deltaTime;
        }
        else
        {
            pingCount = 0;
            pingCharged = true;
        }
    }		
}	

}*/
#endregion