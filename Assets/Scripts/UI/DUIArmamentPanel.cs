using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Diluvion.Ships;
using SpiderWeb;
using TMPro;
using Sirenix.OdinInspector;

public class DUIArmamentPanel : MonoBehaviour {

	public Image weapSprite;
	public TextMeshProUGUI weaponText;
    public TextMeshProUGUI qtyText;

	[ReadOnly]
	public int qty = 1;

	[ReadOnly] public WeaponModule weapon;


	public void Refresh(WeaponModule module)
	{
		weapon = module;
		
		qtyText.text = qty.ToString();

		weaponText.text = weapon.LocalizedName();

		weapSprite.sprite = weapon.weaponIcon;
	}
}
