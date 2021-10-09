using System.Collections;
using System.Collections.Generic;
using Diluvion;
using Diluvion.Ships;
using Sirenix.OdinInspector;
using UnityEngine;

public class DeepsResentment : MonoBehaviour
{
	static DeepsResentment _instance;
	
	[Tooltip("The amount of time between removing deep's resentment and destroying it. " +
	         "Allows for particles to fade away before destroying to create a smooth transition.")]
	public float destroyDelay = 5;
	public float initDelay = 3;
	public float damageInterval = 1;
	
	public WindZone windZone;
	public float defaultWind;
	public float repelledWind;

	[Space, ToggleLeft]
	public bool repelled;
	public Forging repeller;
	
	[Space, ReadOnly]
	public GameObject playerShip;

	[ReadOnly]
	public Hull hull;
	
	[ReadOnly]
	public List<ParticleSystem> particles = new List<ParticleSystem>();

	float _initDelayTimer;
	float _damageTimer;
	float _destroyTimer;
	
	// Update is called once per frame
	void Update ()
	{
		if (playerShip)
		{
			// Follow player ship
			transform.position = playerShip.transform.position;
			_destroyTimer = 0;
		}
		else _destroyTimer += Time.deltaTime;
		
		if (_destroyTimer > destroyDelay) Destroy(gameObject);
		
		// Change the wind zone so that particles appear repelled if the ship has a repeller
		windZone.windMain = repelled ? repelledWind : defaultWind;
		
		// Delay doing damage
		if (_initDelayTimer <= initDelay)
		{
			_initDelayTimer += Time.deltaTime;
			return;
		}

		// Do periodic damage
		if (!hull || repelled) return;
		
		_damageTimer += Time.deltaTime;
		if (!(_damageTimer >= damageInterval)) return;
		
		_damageTimer = 0;
		hull.Damage(1, 0, gameObject);
	}

	public void ApplyToPlayerShip()
	{
		if (PlayerManager.PlayerShip() == null) return;
		
		if (!_instance)
		{
			_instance = Instantiate(gameObject).GetComponent<DeepsResentment>();
			_instance.GetParticles();
		}
		
		_instance.AttachToShip();
	}

	public void RemoveFromPlayerShip()
	{
		if (!_instance) return;
		_instance.StopParticles();
		_instance.Clear();
	}

	void AttachToShip()
	{
		_initDelayTimer = 0;
		StartParticles();
		playerShip = PlayerManager.PlayerShip();
		hull = PlayerManager.PlayerHull();
		
		// Check for a repeller
		Bridge b = playerShip.GetComponent<Bridge>();
		repelled = b.bonusChunks.Contains(repeller);
	}

	void GetParticles()
	{
		particles.Clear();
		particles.AddRange(GetComponentsInChildren<ParticleSystem>());
	}

	void StartParticles()
	{
		foreach (var p in particles) p.Play();
	}

	void StopParticles()
	{
		foreach (var p in particles) p.Stop();
	}

	void Clear()
	{
		_initDelayTimer = 0;
		playerShip = null;
		hull = null;
	}
}
