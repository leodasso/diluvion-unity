using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Diluvion;
using Diluvion.Ships;

[RequireComponent(typeof(Animator))]
public class AnimationHelper : MonoBehaviour {

	public float animationSpeed = 1;
    [InfoBox("Input empty gameobjets with AKEvent Components on them, set to Callback")]
    public List<AkEvent> akAudioCollection = new List<AkEvent>();
	public bool autoPlay = false;
	public Transform lookTarget;
	public float minAudioDist = 5;
	public float shakeAmount = 1;

	[Range(.05f, 2)]
	public float prereqCheckInterval = 1;

	public bool disableAnimator;

	Animator animator;

    string sceneToLoad;

	// Use this for initialization
	void Start () {

		
		if (disableAnimator) SafeAnimator().enabled = false;

        if ( !autoPlay )
            SafeAnimator().speed = 0;

        else Play();
	}

    Animator SafeAnimator()
    {
        if (animator) return animator;
        animator = GetComponent<Animator>();
        return animator;

    }


	public void Play() 
	{
		
        if (!SafeAnimator()) return;
		
        SafeAnimator().enabled = true;
        SafeAnimator().speed = animationSpeed;
	}
	
	public void Shake(float time) 
	{
		OrbitCam.ShakeCam(shakeAmount, transform.position);
	}

	public void DisableDocking() 
	{
		PlayerManager.UndockPlayer();
	}

	public void DisableSideView() {
        if (!PlayerManager.pBridge) return;
		PlayerManager.pBridge.GetComponent<ShipControls>().canSideView = false;
	}

	public void CameraLookAtThis() {

		if (lookTarget != null) {
			OrbitCam.Get ().biasTarget = lookTarget;
		} else {

			OrbitCam.Get ().biasTarget = transform;
		}
	}

	public void HideUI()
	{
		UIManager.ShowAllUI(false);
	}

	public void ShowUI()
	{
		UIManager.ShowAllUI(true);
	}

	public void TryCloseDialog()
    {
        UIManager.Clear<DUI.DialogBox>();
	}


	public void RemoveGuests() {
		PlayerManager.PlayerCrew().RemoveAllGuests();
	}


    /// <summary>
    /// Plays the input index sound event corresponding to a gameobject reference in the akaudiocollection list. These objects need an AKEvent Component with a trigger set to CallBack
    /// </summary>
    /// <param name="index"></param>
	public void PlayAudio(int index)
    {
        if (akAudioCollection.Count < 1) return;
		if (index >= akAudioCollection.Count) return;
        if (akAudioCollection[index] == null) return;

        akAudioCollection[index].GetComponent<AKTriggerCallback>().Callback();
	}

    /// <summary>
    /// Calls a fade overlay, loads the new scene when the screen is fully black, then fades in
    /// </summary>
    /// <param name="newScene"></param>
    public void ChangeScene(string newScene)
    {
        sceneToLoad = newScene;
        FadeOverlay.FadeInThenOut(1.5f, Color.black, LoadScene);
    }

    void LoadScene()
    {
        SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
    }
}
