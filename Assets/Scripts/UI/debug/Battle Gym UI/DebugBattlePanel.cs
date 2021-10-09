using System.Collections;
using System.Collections.Generic;
using Diluvion;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(CanvasGroup))]
public class DebugBattlePanel : MonoBehaviour
{

	[ToggleLeft]
	public bool panelActive;
	CanvasGroup _canvasGroup;
	
	// Use this for initialization
	void Start ()
	{
		_canvasGroup = GetComponent<CanvasGroup>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			SetPanelActive(!panelActive);
		}
	}

	void SetPanelActive(bool isActive)
	{
		panelActive = isActive;

		_canvasGroup.alpha = panelActive ? .75f : .2f;
		_canvasGroup.blocksRaycasts = _canvasGroup.interactable = panelActive;

		if (panelActive)
		{
			GameManager.Freeze(this);
			//Cursor.visible = true;
		}
		else GameManager.UnFreeze(this);
	}
}
