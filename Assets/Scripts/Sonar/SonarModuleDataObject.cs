using UnityEngine;
using System.Collections;
using HeavyDutyInspector;

/*
[System.Serializable]
[CreateAssetMenu(fileName = "new sonar data", menuName = "Diluvion/Sonar Data")]
public class SonarModuleDataObject: ScriptableObject {

	[SerializeField]
	public SonarModuleData sonarData;

	public SonarModuleDataObject() {

	}
}

[System.Serializable]
public class SonarModuleData  {


	[Button("Set default values", "DefaultValues", true)]
	public bool hidden1;


    public DetectionRatio minRatio;
    public DetectionRatio maxRatio;
    public DetectionRatio currentRatio;


	[Comment("Green properties can be modified, others are auto-set.", CommentType.Info)]

	[Background( ColorEnum.Green)]
	public float shortRangePingSpeed = 150; 	// speed at short ping	

	[Background( ColorEnum.Green)]
	public float longRangePingSpeed = 50;       // speed at long ping

	[Background( ColorEnum.Green)]
	public float visualRangePercentage = 0.8f;//80% visual range

	public float passiveMaxRange = 50; 			//passive listening range, Should be set to viewDistance

	[Background( ColorEnum.Green)]
	public float maxSonarRange = 300;		//Power of the Ping

	[Background( ColorEnum.Green)]
	public float pingChargeSeconds = 3;  		//Length of ping charge time  
    [Background(ColorEnum.Green)]
    public float pingCooldown = 3;         //Length of ping charge time  

    [Comment ("Speed at which signatures decay.", CommentType.Info)]
	[Background( ColorEnum.Green)]
	public float trackingFalloff = 0.1f;		//Speed at which signatures decay
	public float currentEfficiency;

	//Constructors for different Situations
	public SonarModuleData()
	{
		currentEfficiency = 0.5f;
		shortRangePingSpeed = 150;     // speed at short ping       
		longRangePingSpeed = 50;       //Move speed at long ping
		visualRangePercentage = 0.8f;
		passiveMaxRange = 50; 			//Power of the Ping
		maxSonarRange = 300;     //Radius of Listening trigger sphere: Absolute maximum range of noticing something
		minRatio = new DetectionRatio(0.8f,0.3f,0.15f);
		maxRatio = new DetectionRatio(0.9f,0.8f, 0.5f);
		currentRatio = new DetectionRatio(minRatio);//default to a copy of minratio
		pingChargeSeconds = 3;  		//Length of ping charge time  
		trackingFalloff = 0.1f;        //Speed at which signatures decay
        DefaultValues();


    }

	public void DefaultValues() {

		minRatio = new DetectionRatio(1, .75f, .4f);
		minRatio.UpdateCurve();

		maxRatio = new DetectionRatio(1, .9f, .8f);
		maxRatio.UpdateCurve();

		shortRangePingSpeed = 150;
		longRangePingSpeed = 100;
		visualRangePercentage = 1;
        maxSonarRange = 500;
		pingChargeSeconds = 3;
		trackingFalloff = .1f;
	}

	public SonarModuleData(SonarModuleData sm)
	{
		currentEfficiency = sm.currentEfficiency;
		shortRangePingSpeed = sm.shortRangePingSpeed;
		longRangePingSpeed = sm.longRangePingSpeed;
		visualRangePercentage = sm.visualRangePercentage;
		passiveMaxRange = sm.passiveMaxRange;
        float flip05 = 1 - sm.currentEfficiency;
        float normalizedEfficiency = Mathf.Lerp(1, 0, flip05 * 2);//This goes from 1 to 0
        minRatio = new DetectionRatio(sm.minRatio);     
		maxRatio = new DetectionRatio(sm.maxRatio);
		currentRatio = new DetectionRatio(sm.currentRatio);
        pingCooldown = sm.pingCooldown;
        currentRatio.SetRanges(Vector3.Lerp(minRatio.ToVector(), maxRatio.ToVector(), normalizedEfficiency));
		pingChargeSeconds = sm.pingChargeSeconds;
		maxSonarRange = sm.maxSonarRange;
		trackingFalloff = sm.trackingFalloff;
	}

	/// <summary>
	/// Constructor for Upgradeable sonarData
	/// </summary>
	/// <param name="Base(Max) module Data"></param>
	/// <param name="Represents the crew efficiency of the module"></param>
	public SonarModuleData(SonarModuleData sm, float efficiency)
	{
		//Direct Efficiency: Values that become smaller with efficiency
		currentEfficiency = efficiency;
		shortRangePingSpeed = sm.shortRangePingSpeed * efficiency;
		longRangePingSpeed = sm.longRangePingSpeed * efficiency;
      
        maxSonarRange = sm.maxSonarRange*efficiency;

		//reverse Efficiency: Values that become Larger as efficiency goes down
		pingChargeSeconds = sm.pingChargeSeconds * 1 + 1 - efficiency;
        pingCooldown = sm.pingCooldown * 1 + 1 - efficiency;

        //Ranges that need a 0-1 lerp
        float flip05 = 1 - efficiency;
		float normalizedEfficiency = Mathf.Lerp(1, 0, flip05 * 2);//This goes from 1 to 0

		//Catch for Null Ratio's
		maxRatio = new DetectionRatio(sm.maxRatio);       
		minRatio = new DetectionRatio(sm.minRatio);
		currentRatio = new DetectionRatio(sm.currentRatio);

		//Sets the current Ratio to a Value between min and max Ratio
		currentRatio.SetRanges(Vector3.Lerp(minRatio.ToVector(), maxRatio.ToVector(), normalizedEfficiency));

		//Unaffected by efficiency(for now)      
		trackingFalloff = sm.trackingFalloff;
		//Passive max range will be manipulated by the fogRange
		passiveMaxRange = sm.passiveMaxRange;
		visualRangePercentage = sm.visualRangePercentage;
	}

    public void SetChargeTime(SonarModuleData baseData, float percentage)
    {
        pingChargeSeconds = baseData.pingChargeSeconds / percentage;
        pingCooldown = baseData.pingCooldown / percentage;

    }

    public void SetPingSpeed(SonarModuleData baseData, float percentage)
    {
        shortRangePingSpeed = baseData.shortRangePingSpeed* percentage;
        longRangePingSpeed = baseData.longRangePingSpeed* percentage;
    }

    //TODO YAGNI move ratios independant of max range
    public void SetRange(SonarModuleData baseData, float percentage)
    {
        maxSonarRange = baseData.maxSonarRange * percentage;
        SetPingSpeed(baseData, percentage);
        //Ranges that need a 0-1 lerp
   
        //Catch for Null Ratio's
        maxRatio = new DetectionRatio(baseData.maxRatio);
        minRatio = new DetectionRatio(baseData.minRatio);
        currentRatio = new DetectionRatio(baseData.currentRatio);

        //Sets the current Ratio to a Value between min and max Ratio
        currentRatio.SetRanges(Vector3.LerpUnclamped(minRatio.ToVector(), maxRatio.ToVector(), Mathf.Clamp01(percentage/4)));
    }

}
*/