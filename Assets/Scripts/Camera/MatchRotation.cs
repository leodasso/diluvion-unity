using UnityEngine;
using Diluvion;
using Diluvion.Ships;
using Sirenix.OdinInspector;

public class MatchRotation : MonoBehaviour
{

	public enum  MatchingType
	{
		SpecificObject,
		Camera,
		PlayerShip,
		PlayerAim
	}

	public MatchingType MatchTarget = MatchingType.Camera;
	
	[ShowIf("MatchingObject")]
	public Transform objectToMatch;
	
	[Range(.1f, 99)]
	public float speed = 1;

	bool MatchingObject()
	{
		return MatchTarget == MatchingType.SpecificObject;
	}

	
	// Update is called once per frame
	void LateUpdate () {
	
		if (MatchTarget == MatchingType.Camera && Camera.main != null)
            MatchObj(Camera.main.transform);
		
		if (MatchTarget == MatchingType.SpecificObject) 
			MatchObj(objectToMatch.transform);
		
		if (MatchTarget == MatchingType.PlayerShip && PlayerManager.PlayerShip() != null) 
			MatchObj(PlayerManager.PlayerShip().transform);
		
		if (MatchTarget == MatchingType.PlayerAim) MatchPlayerAim();
	}

	void MatchPlayerAim()
	{
		if (!PlayerManager.PlayerControls()) return;
		if (!PlayerManager.PlayerControls().Aimer()) return;
		
		MatchObj(PlayerManager.PlayerControls().Aimer().transform);

	}

	void MatchObj(Transform theObject) {

		transform.rotation = Quaternion.Slerp(transform.rotation, theObject.rotation, Time.deltaTime * speed);
	}
}
