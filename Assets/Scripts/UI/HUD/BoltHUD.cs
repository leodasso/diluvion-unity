using System.Collections;
using System.Collections.Generic;
using Diluvion.Ships;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays:
/// --Accuracy of weapon
/// --Range of weapon
/// --available mounts (which ones can physically aim at the bridge's aim)
/// cooldown
/// </summary>
public class BoltHUD : WeaponHUD
{
	[Space] 
	public RectTransform mainReticule;
	public RectTransform fineReticule;
	public TextMeshProUGUI rangeText;
	public Transform mountsParent;
	public GameObject mountDisplayPrefab;

	[Tooltip("Images that will change color to indicate hostility of the aimed target")]
	public List<Image> colorChangingImages;

	public Color outOfRange = Color.grey;
	public Color neutral = Color.white;
	public Color hostile = Color.red;
	public Color friendly = Color.green;

	[Space] 
	[Tooltip("How many pixels in screen space should be used to represent one degree of weapon fire spread")]
	public float pixelsPerDegree = 5;
	
	[ReadOnly]
	public float accuracySize = 10;

	float _totalAccuracy;
	Image _mainReticuleSprite;
	bool _createdMounts;
	readonly List<MountHUD> _mounts = new List<MountHUD>();
	Color rangeColor = Color.white;

	protected override void Start()
	{
		base.Start();
		_mainReticuleSprite = mainReticule.GetComponent<Image>();
	}

	Transform _aimTarget;
	IAlignable _alignable;
	
	protected override void Update()
	{
		base.Update();

		if (!weaponSystem) return;
		if (!Camera.main) return;

		// create the mounts UI
		if (!_createdMounts)
		{
			CreateMounts();
			_createdMounts = true;
		}

		// Change the size of the reticle based on the accuracy of the weapon
		accuracySize = weaponSystem.equippedWeapon.weapon.shotSpread * pixelsPerDegree;
		fineReticule.sizeDelta = new Vector2(accuracySize, accuracySize);

		// Adjust the color based on current aim. start with range check
		if (!weaponSystem.InRange()) SetColor(outOfRange);
		// Then check if aiming at anything we can shoot
		else if (weaponSystem.AimingAtVulnerable())
		{
			_aimTarget = weaponSystem.GetSystemTarget();
			_alignable = _aimTarget.GetComponent<IAlignable>();

			if (_alignable != null)
			{
				switch (_alignable.getAlignment())
				{
						case AlignmentToPlayer.Friendly : SetColor(friendly); break;
						case AlignmentToPlayer.Hostile : SetColor(hostile); break;
						default: SetColor(neutral); break;
				}
			}

		}
		// Default color
		else SetColor(neutral);

		
		// Display info about the mounts
		// Get an index of the active mount we can use to compare with the index of the mount GUI to show which is active
		int adjustedFireIndex = weaponSystem.fireIndex - 1;
		if (adjustedFireIndex < 0) adjustedFireIndex = _mounts.Count - 1;
		
		// Send info to the mounts GUI
		for (int i = 0; i < _mounts.Count; i++)
		{
			_mounts[i].activeMount = i == adjustedFireIndex;
			_mounts[i].ready = weaponSystem.module.ValidMount(weaponSystem.mounts[i]);
		}

		// display range to target
		if (!weaponSystem.InRange()) rangeColor = Color.gray;
		else rangeColor = Color.white;

		rangeText.color = rangeColor;
		float range = weaponSystem.RangeToTarget();
		rangeText.text = range.ToString("000");
	}

	void SetColor(Color color)
	{
		foreach (var sprite in colorChangingImages) sprite.color = color;
	}

	void LateUpdate()
	{
		if (!weaponSystem) return;
		if (!Camera.main) return;

		mainReticule.transform.position = FollowTransform(weaponSystem.GetTarget().transform.position, 20, Camera.main);
		fineReticule.transform.position = FollowTransform(weaponSystem.TotalAimPosition(), 20, Camera.main);
	}


	/// <summary>
	/// Create the UI for each mount to indicate its status
	/// </summary>
	void CreateMounts()
	{
		for (int i = 0; i < weaponSystem.mounts.Count; i++)
		{
			GameObject newMount = Instantiate(mountDisplayPrefab, mountsParent);
			_mounts.Add(newMount.GetComponent<MountHUD>());
		}
	}
}
