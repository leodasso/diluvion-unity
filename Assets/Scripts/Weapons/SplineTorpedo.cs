using FluffyUnderware.Curvy;
using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteInEditMode]
public class SplineTorpedo : MonoBehaviour
{

	[TabGroup("Design"), LabelText("Speed over time")]
	public AnimationCurve speedCurve = new AnimationCurve(new Keyframe(0,15), new Keyframe(0.5f,14),  new Keyframe(1,2),new Keyframe(1.5f,1),new Keyframe(2,15),new Keyframe(3,20));

	[TabGroup("Design"), Tooltip("The time from start that the drive effects will begin. Cosmetic.")]
	public float driveBeginTime = 1;

	[TabGroup("Design")] 
	public ParticleSystem driveParticle;

	[SerializeField, TabGroup("Debug")] bool driving = false;
	[SerializeField, TabGroup("Debug")] CurvySpline splineToFollow;
	[SerializeField, TabGroup("Debug")] float tf = 0;
	[SerializeField, TabGroup("Debug")] float timeSinceStart;
	
	/// <summary>
	/// Direction of the flow of the curve i am moving
	/// </summary>
	int direction = 1;

	PseudoVelocity psy;

	PseudoVelocity MyPsy
	{
		get
		{
			if (psy != null) return psy;
			return psy = GetComponent<PseudoVelocity>();
		}
	}

	void Update()
	{
		MoveAlongSpline();

		if (timeSinceStart >= driveBeginTime && !driving)
		{

			SetDriveParticle(true);
			driving = true;
			var akTrigger = GetComponent<AKTriggerPositive>();
			if (akTrigger) akTrigger.TriggerPos();
		}
	}

	void SetDriveParticle(bool emissionEnabled)
	{
		if (!driveParticle) return;
		var emission = driveParticle.emission;
		emission.enabled = emissionEnabled;
	}
	
	[Button, TabGroup("Debug")]
	public void Reset()
	{
		tf = 0;
		timeSinceStart = 0;
		driving = false;
	}
	
	public void SetSpline(TorpedoSpline ts)
	{
		//Debug.Log("Setting spline on " + name + " to " + ts.MySpline);
		splineToFollow = ts.MySpline;
	}
	

	void MoveAlongSpline()
	{
		if (splineToFollow == null) return;
		
		timeSinceStart += Time.deltaTime;
		float speed = speedCurve.Evaluate(timeSinceStart);

		if (tf < 1)
		{
			Quaternion velForward =  Quaternion.LookRotation(MyPsy.velocity.normalized);
			transform.rotation = velForward;

			transform.position = splineToFollow.MoveBy(ref tf, ref direction, speed * Time.deltaTime, CurvyClamping.Clamp) +
			                     splineToFollow.transform.position;
		}
		else 
			transform.Translate(transform.forward.normalized * speed * Time.deltaTime);
	}

	void OnSpawned()
	{
		SetDriveParticle(false);
	}
	

	void OnDisable()
	{
		Reset();
	}
}
