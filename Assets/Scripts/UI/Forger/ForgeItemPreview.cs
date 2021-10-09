using System.Collections;
using System.Collections.Generic;
using DUI;
using Loot;
using UnityEngine;
using UnityEngine.UI;

public class ForgeItemPreview : MonoBehaviour
{

	public ForgeItemPanel descriptionPrefab;
	public Forging forging;
	public Image image;

	ForgeItemPanel _descriptionInstance;

	public void ShowItem(Forging chunk)
	{
		forging = chunk;
		image.sprite = forging.previewSprite;
	}

	public void ShowDescription()
	{
		if (!forging) return;
		if (!ShipComparison.GetInstance()) return;
		_descriptionInstance = Instantiate(descriptionPrefab, ShipComparison.GetInstance().transform);
		_descriptionInstance.transform.position = transform.position;
		_descriptionInstance.ApplyChunk(forging);
	}

	public void HideDescription()
	{
		if (!_descriptionInstance) return;
		Destroy(_descriptionInstance.gameObject);
	}
}
