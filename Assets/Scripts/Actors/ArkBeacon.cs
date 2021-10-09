using UnityEngine;
using System.Collections;
using Rewired;
using Diluvion;
using Diluvion.Ships;

public class ArkBeacon : MonoBehaviour, IOnDead {

	public float energy = 100;
	[HideInInspector]	public float energyValue = 1;

	public float energyPerShot = 5;

	[Tooltip("Cooldown when full, and cooldown when near empty.")]
	public float fireCooldown = .1f;
	public float emptyCooldown = .5f;

	public float ammoForce;
	public SimpleAmmo ammoPrefab;
	public Transform gunPoint;
	public float inaccuracy = 5;

	public float rotSpeed = 10;
	public float moveSpeed = 10;
	public float hoverRadius = 20;
	public float distanceAheadOfShip = 5;

	[Tooltip("The force with which it's ejected away form player when out of energy.")]
	public float deathForce = 50;

	public delegate void OnFire();
	public OnFire onFire;               // Called whenever firing

    public event OnDead onDead;

    public Material materialBase;	

	float actualCooldown;
	float maxEnergy;
	Vector3 offset;
	Player player;
	Material localMat;
	Color initColor;
	Color emptyColor = Color.black;
	Light myLight;
	float initLightIntensity;
	bool canFire = true;
	Transform playerShip;
	ParticleSystem beaconParticle;


	// Use this for initialization
	void Start () {

		// Find light
		myLight = GetComponentInChildren<Light>();
		initLightIntensity = myLight.intensity;

		// prepare material for color changing
		localMat = new Material(materialBase);
		initColor = localMat.GetColor("_EmissionColor");
		foreach (Renderer render in GetComponentsInChildren<Renderer>())
			if(render.sharedMaterial.name == localMat.name) render.sharedMaterial = localMat;

		maxEnergy = energy;
		offset = Random.onUnitSphere * hoverRadius;
		player = ReInput.players.GetPlayer(0);
	}

	private Bridge playerBridge;

	public Bridge PlayerBridge
	{
		get
		{
			if (playerBridge != null) return playerBridge;
			return playerBridge = PlayerManager.PlayerShip().GetComponent<Bridge>();
		}
		
	} 
	
	
	void OnEnable() {
		SetColliders(true);
		//SendMessage("BeaconEnabled", SendMessageOptions.DontRequireReceiver);



		beaconParticle = GetComponentInChildren<ParticleSystem>();
		if (beaconParticle != null) {
			Debug.Log("Stopping particle system.");
			beaconParticle.Stop();
		}

		// find the player's ship controls
	
	}

	void OnDisable() {
		SetColliders(false);
		if (PlayerBridge) PlayerBridge.disableWeapons = false;
	}

	
	// Update is called once per frame
	void Update () {

		if (OrbitCam.Get().cameraMode != CameraMode.Normal) return;

		if (PlayerBridge) PlayerBridge.disableWeapons = true;

		// Get the normalized energy value, b/t 0 and 1
		energyValue = energy / maxEnergy;

		// Set emission color and light intensity to represent energy remaining
		Color glowColor = Color.Lerp(emptyColor, initColor, energyValue);
		localMat.SetColor ("_EmissionColor", glowColor);

		myLight.intensity = Mathf.Lerp(0, initLightIntensity, energyValue);

		// Set cooldown values based on energy remaining
		actualCooldown = Mathf.Lerp(emptyCooldown, fireCooldown, energyValue);

		Aim();

		if (PlayerManager.pBridge == null) return;

		// Follow the player
		playerShip = PlayerManager.pBridge.transform;
		Vector3 gotoPos = playerShip.position + playerShip.transform.forward * distanceAheadOfShip + offset;
		transform.position = Vector3.Lerp(transform.position, gotoPos, Time.deltaTime * moveSpeed);

        /*if(player.GetButtonDown("fire bolt"))
            SpiderWeb.SpiderSound.MakeSound("Play_Ark_Boss_Laser", gameObject);*/

        if (player.GetButton("fire bolt")) Fire();

       /* if(player.GetButtonUp("fire bolt"))
            SpiderWeb.SpiderSound.MakeSound("Stop_Ark_Boss_Laser", gameObject);*/
    }


	void Aim() {


		ShipControls playerControls = PlayerManager.pBridge.GetComponent<ShipControls>();

		if (playerControls == null) return;

		Vector3 aimPoint = playerControls.LastAimedPosition;
		Vector3 aimDirection = aimPoint - transform.position;
		Quaternion newRotation = Quaternion.LookRotation(aimDirection);


		transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * rotSpeed);
	}


	void Fire() {

		if (energy < 0) return;
		if (!canFire) 	return;

		if (onFire  != null) onFire();

		SimpleAmmo newAmmo = Instantiate(ammoPrefab, gunPoint.position, gunPoint.transform.rotation) as SimpleAmmo;
       
		//add random rotation
		float randAmt = inaccuracy;
		newAmmo.transform.Rotate(new Vector3(Random.Range(-randAmt, randAmt), Random.Range(-randAmt, randAmt), Random.Range(-randAmt, randAmt)));

		//add force
		float force = Random.Range(ammoForce, ammoForce * 1.5f);
		newAmmo.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0, 0, force));

		//shake camera
		OrbitCam.ShakeCam( .2f, transform.position);

		//get ammo component
		newAmmo.friendHull = PlayerManager.pBridge.hull;
		newAmmo.Init();

		energy -= energyPerShot;
        GetComponent<AKTriggerCallback>().Callback();
        if (energy < 0) Die();

		canFire = false;

		StartCoroutine(Cooldown());
	}

	void Die() {

		Rigidbody rb = GetComponent<Rigidbody>();
		rb.isKinematic = false;
		rb.interpolation = RigidbodyInterpolation.Interpolate;
        //SpiderWeb.SpiderSound.MakeSound("Stop_Ark_Boss_Laser", gameObject);
        if (playerShip == null) return;

		Vector3 diff = playerShip.transform.position - transform.position;
		rb.AddForce(-diff * rb.mass * deathForce);
		rb.AddTorque(diff * rb.mass * deathForce);
        if (onDead!=null)
            onDead(10);
        
        enabled = false;
    }

	IEnumerator Cooldown() {
		yield return new WaitForSeconds(actualCooldown);
		canFire = true;
		yield break;
	}
		
    
	/// <summary>
	/// Set colliders on or off
	/// </summary>
	void SetColliders(bool isTrigger) {

		foreach (Collider col in GetComponentsInChildren<Collider>()) {		
			col.isTrigger = isTrigger;
		}
	}
}