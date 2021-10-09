using System.Collections;
using System.Collections.Generic;
using Diluvion.Ships;
using Loot;
using UnityEngine;
using Sirenix.OdinInspector;
using SpiderWeb;
using TMPro;

namespace DUI
{

	public class WeaponSwapHUD : DUIPanel
	{

		public List<DItemWeapon> weapons;
		public int selectedIndex;
		[Tooltip("What weapon module is having its weapons displayed currently?")]
		public WeaponModule displayedModule;
		int _prevIndex;

		[Space]
		public TextMeshProUGUI weaponName;
		public Transform weaponDisplayParent;
		public GameObject weaponDisplayPrefab;
		
		[Space]
		public float weaponIconSize;
		public float padding;
		[Tooltip("After swapping, how many seconds will the element remain visible")]
		public float cooldownTime;
		float _cooldownTimer;
		
		[Tooltip("How many weapons show on either side of the selected (center) weapon")]
		public int maxExtents = 3;
		[Range(0, 1), Tooltip("Min size (as a percentage of default size) of the weapon icons. Icons will approach this size" +
		                      "as their index gets closer to either extreme.")]
		public float minSize = .5f;
		
		List<WeaponSwapIcon> _weaponDisplays = new List<WeaponSwapIcon>();

		static WeaponSwapHUD _instance;

		/// <summary>
		/// Display a weapon swap.
		/// </summary>
		/// <param name="direction">The direction to swap in (pos or neg being right or left</param>
		/// <param name="caller">The weapon system calling for a swap</param>
		public static void SwapWeapon(int direction, WeaponSystem caller)
		{
			if (!_instance)
			{
				// instantiate the hud if it hasn't been instantiated yet
				_instance = UIManager.Create(UIManager.Get().weaponSwapPanel as WeaponSwapHUD);
				Debug.Log("Creating weapon swap instance.", _instance);
			}
			
			_instance.InstanceSwapWeapon(direction, caller);
		}

		void InstanceSwapWeapon(int direction, WeaponSystem caller)
		{
			// make visible again
			alpha = 1;
			_cooldownTimer = cooldownTime;

			// Check the hud's displayed module. Refresh if there's been a change in modules
			if (displayedModule != caller.module)
			{
				displayedModule = caller.module;
				FullRefresh(caller);
			}

			//If the hud has faded fully out, do a refresh anyways 
			if (alpha < .05f) FullRefresh(caller);
			
			//find the index of the currently equipped weapon
			var equipped = caller.equippedWeapon;
			selectedIndex = weapons.IndexOf(equipped);

			// finally, apply the change to the index
			selectedIndex += direction;
			LoopIndex();
			
			// Tell the weapon system to equip the new weapon
			caller.EquipWeapon(weapons[selectedIndex]);
			
			// Audio
			if (direction > 0) SpiderSound.MakeSound("Play_WeaponSwap_Bolt", gameObject);
			else SpiderSound.MakeSound("Play_WeaponSwap_Torpedo", gameObject);
		}

		/// <summary>
		/// Refreshes list of weapons based on the given weapon system 'caller', and refreshes the weapon icons.
		/// </summary>
		/// <param name="caller">The weapon system calling for a swap</param>
		void FullRefresh(WeaponSystem caller)
		{
			weapons.Clear();
			weapons.AddRange(caller.WeaponsInInventory());
			Refresh();
		}

		protected override void Update()
		{
			base.Update();

			for (int i = 0; i < _weaponDisplays.Count; i++)
			{
				UpdateDisplay(_weaponDisplays[i], i);
			}

			if (_prevIndex != selectedIndex)
			{
				_prevIndex = selectedIndex;
				UpdateText();
			}

			if (_cooldownTimer > 0)
				_cooldownTimer -= Time.unscaledDeltaTime;
			else 
				alpha = 0;
		}

		void UpdateText()
		{
			weaponName.text = _weaponDisplays[selectedIndex].weapon.LocalizedName();
		}


		int _displayIndex;
		int _extent;
		float _xPos;
		
		void UpdateDisplay(WeaponSwapIcon display, int index)
		{
			// Get index for the display
			_displayIndex = index - selectedIndex;
			_extent = Mathf.Abs(_displayIndex);
			
			// Set position
			_xPos = _displayIndex * (weaponIconSize + padding);
			display.anchoredPos = new Vector2(_xPos, display.anchoredPos.y);
			
			// Set alpha
			float distFromCenter = (float)_extent / maxExtents;
			float newAlpha = Mathf.Lerp(1, 0, distFromCenter);
			display.canvasGroup.alpha = newAlpha;
			
			// Set size
			display.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * minSize, distFromCenter);
		}

		[ButtonGroup("index")]
		public void IncreaseIndex()
		{
			selectedIndex++;
			LoopIndex();
		}

		[ButtonGroup("index")]
		public void DecreaseIndex()
		{
			selectedIndex--;
			LoopIndex();
		}

		/// <summary>
		/// If the index is less than/greater than the bounds of the weapons list, this loops it around to the opposite end.
		/// </summary>
		void LoopIndex()
		{
			if (selectedIndex < 0) selectedIndex = _weaponDisplays.Count - 1;
			else if (selectedIndex >= _weaponDisplays.Count) selectedIndex = 0;
		}

		[Button()]
		void Refresh()
		{
			SpiderWeb.GO.DestroyChildren(weaponDisplayParent);
			_weaponDisplays.Clear();
			foreach (var w in weapons) InstantiateDisplay(w);
			UpdateText();
		}

		
		/// <summary>
		/// Instantiates a new weapon display into the list for the given weapon
		/// </summary>
		void InstantiateDisplay(DItemWeapon weapon)
		{
			var newDisplay = Instantiate(weaponDisplayPrefab, weaponDisplayParent).GetComponent<WeaponSwapIcon>();
			newDisplay.ApplyWeapon(weapon);
			_weaponDisplays.Add(newDisplay);
		}
	}
}