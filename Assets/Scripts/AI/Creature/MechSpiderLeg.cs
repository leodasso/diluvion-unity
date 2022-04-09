using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;
using RootMotion.Demos;


/// <summary>
/// Leg of the Mech spider. Controls stepping and positioning the IK target
/// </summary>
public class MechSpiderLeg : MonoBehaviour {
		
	public MechSpider mechSpider; // Reference to the target
	public MechSpiderLeg unSync; // One of the other legs that we dont want to be completely in sync with, that is stepping at the same time
	public Vector3 offset; // Offset from the default position
	public float minDelay = 0.2f, maxOffset = 1.0f, stepSpeed = 5.0f, footHeight = 0.15f, velocityPrediction = 0.2f, raycastFocus = 0.1f; // Parameters for stepping
	public AnimationCurve yOffset;

	public GameObject footParticlePrefab; // FX for sand
	public bool debug = false;
	
	private IK ik;
	private float stepProgress = 1f, lastStepTime;
	private Vector3 defaultPosition;
	private RaycastHit hit = new RaycastHit();

	// Is the leg stepping?
	public bool isStepping {
		get {
			return stepProgress < 1f;
		}
	}

	// Gets and sets the IK position for this leg
	public Vector3 position {
		get
		{
			if (!ik) ik = GetComponent<IK>();
			return ik.GetIKSolver().GetIKPosition();
		}
		set {
			if (!ik) ik = GetComponent<IK>();
			ik.GetIKSolver().SetIKPosition(value);
		}
	}
	
	
		
	void Start() {
		
		// Find the ik component
		ik = GetComponent<IK>();

		// Workaround for Unity Win Store/Phone serialization bug
		stepProgress = 1f;
			
		hit = new RaycastHit();
			
		var points = ik.GetIKSolver().GetPoints();
		position = points[points.Length - 1].transform.position;
			
		hit.point = position;

		// Store the default rest position of the leg
		defaultPosition = mechSpider.transform.InverseTransformPoint(position + offset * mechSpider.scale);
	}

	// Find the relaxed grounded positon of the leg relative to the body in world space.
	private Vector3 GetStepTarget(out bool stepFound, float focus, float distance) {
		stepFound = false;

		// place hit.point to the default position relative to the body
		Vector3 stepTarget = mechSpider.transform.TransformPoint(defaultPosition);
		stepTarget += (hit.point - position) * velocityPrediction;

		Vector3 up = mechSpider.transform.up;

		// Focus the ray directions towards the spider body
		Vector3 toBody = mechSpider.body.position - position;
		Vector3 axis = Vector3.Cross(up, toBody);
		up = Quaternion.AngleAxis(focus, axis) * up;

		// Raycast to ground the relaxed position
		if (Physics.Raycast(stepTarget + up * mechSpider.raycastHeight * mechSpider.scale, -up, out hit, mechSpider.raycastHeight * mechSpider.scale + distance, mechSpider.raycastLayers)) 
			stepFound = true;

		if (debug)
		{
			//Debug.Log("Got Step target: " + stepFound);
			Debug.DrawLine(transform.position,  hit.point + mechSpider.transform.up * footHeight * mechSpider.scale, Color.red, 0.01f);
		}
	
		return hit.point + mechSpider.transform.up * footHeight * mechSpider.scale;
	}

	void Update () {
		// if already stepping, do nothing
		if (isStepping) return;

		// Minimum delay before stepping again
		if (Time.time < lastStepTime + minDelay) return;

		// If the unSync leg is stepping, do nothing
		if (unSync != null) {
			if (unSync.isStepping) return;
		}

		// Find the ideal relaxed position for the leg relative to the body
		bool stepFound = false;
		Vector3 idealPosition = GetStepTarget(out stepFound, raycastFocus, mechSpider.raycastDistance * mechSpider.scale);
		if (!stepFound) idealPosition = GetStepTarget(out stepFound, -raycastFocus, mechSpider.raycastDistance * 3f * mechSpider.scale); // Try again with inverted focus
		if (!stepFound) return;

		// If distance to that ideal position is less than the threshold, do nothing
		if (Vector3.Distance(position, idealPosition) < maxOffset * mechSpider.scale * UnityEngine.Random.Range(0.9f, 1.2f)) return;

		// Need to step closer to the ideal position
		StopAllCoroutines();
		StartCoroutine(Step(position, idealPosition));
	}

	/// <summary>
	/// Force the foot to plant on the ground right below where it currently is
	/// </summary>
	public void PlantFoot()
	{
		RaycastHit targetHit;
		Vector3 pos = position + Vector3.up * 10;
		if (Physics.Raycast(pos, Vector3.down, out targetHit,  50 ,mechSpider.raycastLayers))
		{
			Debug.DrawLine(pos, targetHit.point, Color.cyan, 45);
			StopAllCoroutines();
			StartCoroutine(Step(position, targetHit.point));
		}
		else
		{
			 Debug.Log(name + " couldn't find a target point to step to.");
		}
	}

	// Stepping co-routine
	private IEnumerator Step(Vector3 stepStartPosition, Vector3 targetPosition) {
		stepProgress = 0f;
		if (debug)
		{
			//Debug.Log("Starting Step");
			Debug.DrawLine(stepStartPosition, targetPosition, Color.yellow, 0.01f);
		}
			

		// Moving the IK position
		while (stepProgress < 1) {
			;
			stepProgress += Time.deltaTime * stepSpeed;
				
			position = Vector3.Lerp(stepStartPosition, targetPosition, stepProgress);
			position += mechSpider.transform.up * yOffset.Evaluate(stepProgress) * mechSpider.scale;
				
			yield return null;
		}
		
		// Emit sand
		if (footParticlePrefab != null)
		{
			RaycastHit hit;
			Quaternion rotation = footParticlePrefab.transform.rotation;
			if (Physics.Raycast(position + mechSpider.transform.up, -mechSpider.transform.up, out hit, 5,
				LayerMask.GetMask("Terrain")))
			{
				rotation = Quaternion.LookRotation(hit.normal);
				position = hit.point;
			}
				
			
			
			Instantiate(footParticlePrefab, position, rotation);
		}
		position = targetPosition;
			
		lastStepTime = Time.time;
	}
	
}
