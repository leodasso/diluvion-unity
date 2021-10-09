using System;
using System.Collections;
using System.Collections.Generic;
using Diluvion;
using Diluvion.Ships;
using DUI;
using Sirenix.OdinInspector;
using UnityEngine;

public class WeaponHUD : DUIPanel
{

	[ReadOnly]
	public Bridge bridge;

	[ReadOnly]
	public WeaponSystem weaponSystem;
	
	[ReadOnly]
	public Cannon equippedCannon;

	protected ShipControls shipControls;
	

	public virtual void Init(WeaponSystem forWeaponSystem)
	{
		shipControls = bridge.GetComponent<ShipControls>();
		weaponSystem = forWeaponSystem;
	}


	protected override void Update()
	{
		base.Update();
		
		if (weaponSystem == null) return;
		if (weaponSystem.equippedWeapon)
			equippedCannon = weaponSystem.equippedWeapon.weapon;
	}
}
