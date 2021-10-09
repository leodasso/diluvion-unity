using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Cinemachine.Timeline;
using Diluvion;
using Diluvion.Ships;
using UnityEngine;
using UnityEngine.Playables;
using Sirenix.OdinInspector;
using UnityEngine.Timeline;

[RequireComponent(typeof(PlayableDirector))]
public class TimelineHelper : MonoBehaviour
{
	[ToggleLeft] public bool AssignMainCam;
	[ToggleLeft, ShowIf("AssignMainCam")] public bool PlayOnAssign;

	[ToggleLeft, ShowIf("AssignMainCam")]
	public bool DisableControlsOnPlay;
	
	private PlayableDirector _director;

	// Use this for initialization
	IEnumerator Start ()
	{
		_director = GetComponent<PlayableDirector>();

		while (Camera.main == null) yield return null;

		if (AssignMainCam)
			ApplyNewCamera();

		if (DisableControlsOnPlay)
		{
			while (PlayerManager.PlayerShip() == null)
				yield return null;

			PlayerManager.PlayerShip().GetComponent<ShipControls>().enabled = false;
		}
	}

	void ApplyNewCamera()
	{
		CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();

		if (brain == null)
		{
			Debug.LogError("No cinemachine brain could be found on the main camera.", gameObject);
			return;
		}
		
		_director = GetComponent<PlayableDirector>();
		TimelineAsset timelineAsset = (TimelineAsset) _director.playableAsset;
		
		foreach (var binding in timelineAsset.outputs)
		{
			if (binding.sourceObject is CinemachineTrack)
			{
				// put the main camera cinemachine brain in the track
				_director.SetGenericBinding(binding.sourceObject, brain);
			}
		}
		
		if (PlayOnAssign) _director.Play();

		StartCoroutine(EnableControls((float)_director.duration));
	}

	IEnumerator EnableControls(float delayTime)
	{
		yield return new WaitForSeconds(delayTime);
		
		PlayerManager.PlayerShip().GetComponent<ShipControls>().enabled = true;
	}

}
