using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Diluvion;

public class DUINotification : MonoBehaviour {

    public Text notifierText;
    public Text highlightText;
    Animator animator;
	CanvasGroup myGroup;
	float alpha;

	void Start() {
		myGroup = GetComponent<CanvasGroup>();
	}

    Animator Animator()
    {
        if (animator != null) return animator;
        animator = GetComponent<Animator>();
        return animator;
    }

    public void KillMe()
    {
        Destroy(this.gameObject);
    }

    public void SetSpeed(float speed)
    {
        animator.speed = speed;
    }

	void Update() {

		if (OrbitCam.Get().cameraMode == CameraMode.Normal) alpha = 1;
		else alpha = 0;

		myGroup.alpha = Mathf.Lerp(myGroup.alpha, alpha, Time.deltaTime * 5);
	}

    //Makes a new Notification
    public void NewNotification(string notification, string highlight)
    {

        //Play sound
        GetComponent<AKTriggerPositive>().TriggerPos();

        notifierText.text = notification;
        highlightText.text = highlight;
        Animator().SetTrigger("notify");
    }

}
