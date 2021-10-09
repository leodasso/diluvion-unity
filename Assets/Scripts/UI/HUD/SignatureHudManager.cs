using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DUI;
using UnityEngine;

public class SignatureHudManager : MonoBehaviour
{

	/// <summary>
	/// max distance a signature can be from the player's reticule and still display
	/// </summary>
	const float maxDistance = 100;
	static SignatureHudManager _instance;
	List<SignatureHUD> _signatureHuds = new List<SignatureHUD>();
	SignatureHUD _focusHud;

	public static SignatureHudManager Get()
	{
		if (_instance) return _instance;
		GameObject newManager = new GameObject("signature hud manager");
		newManager.transform.parent = UIManager.GetDUI().transform;
		return _instance = newManager.AddComponent<SignatureHudManager>();
	}

	public static void RegisterInstance(SignatureHUD instance)
	{
		if (Get()._signatureHuds.Contains(instance)) return;
		
		Get()._signatureHuds.Add(instance);
	}

	/// <summary>
	/// Tells if this instance is the focus for what the player is aiming at.
	/// </summary>
	public static bool IsFocus(SignatureHUD instance)
	{
		if (instance != Get()._focusHud) return false;

		return instance.DistFromAim() < maxDistance;
	}
	
	// Update is called once per frame
	void Update ()
	{

		_signatureHuds = _signatureHuds.Where(x => x != null).ToList();

		if (_signatureHuds.Count < 1)
		{
			_focusHud = null;
			return;
		}
		
		// order the huds by distance to the reticule
		_signatureHuds = _signatureHuds.OrderBy(x => x.DistFromAim()).ToList();
		_focusHud = _signatureHuds.First();

	}
}
