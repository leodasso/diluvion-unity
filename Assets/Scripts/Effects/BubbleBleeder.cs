using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using SpiderWeb;
using Diluvion;

public class BubbleBleeder : MonoBehaviour
{

	[Range(0,1), OnValueChanged("ChangeIntensity")]
	public float bleedIntensity = 0;
	
	[Range( 0,5)]
	public float bleedBuffer =0;

	public int baseBubbleRate = 20;

	public List<GameObject> instances = new List<GameObject>();

	[SerializeField] int bleedPoints = 4;



	GameObject BubblePrefab(float intensity)
	{
		return SurfaceMaterial.GetImpact(intensity, RoundingBias.Floor);
	}
	SurfaceMaterial surfaceMaterial;

	private SurfaceMaterial SurfaceMaterial {
		get
		{
			if (surfaceMaterial != null) return surfaceMaterial;
			return surfaceMaterial = Resources.Load<SurfaceMaterial>("effects/bubbleBleed particles");
		}

	}

	[SerializeField]
	private float stepSize = 0.1f;
	
	#region init

	void Start()
	{
		if (instances.Count < 1)
			SetRandomBleedPoints();
		
		ChangeIntensity();
		
		if(GetComponentInParent<Hull>())
			GetComponentInParent<Hull>().lostHealth += HealthStatus;
	}

	/// <summary>
	/// The delegate callback
	/// </summary>
	void HealthStatus(float currentHealth, float currentPercent)
	{
		ChangeIntensity(1-currentPercent);
	}
	
	#endregion
	
	#region editingFunctions
	
	[Button]
	void MovePointsIn()
	{
		MovePoints(true);
	}
	
	[Button]
	void MovePointsOut()
	{
		MovePoints(false);
	}
	
	/// <summary>
	/// Moves all the instances along their forward vector
	/// </summary>
	void MovePoints(bool inward)
	{
		foreach (GameObject ps in instances)
		{	
			Vector3 direction = inward ? -ps.transform.forward : ps.transform.forward;
			ps.transform.position += (direction*stepSize);
		}
	}

	private float sphereSize = 0;
	
	[Button]
	void SetRandomBleedPoints()
	{
		ResetInstances();
		for (int i = 0; i < bleedPoints; i++)
		{
			Vector3 worldMeshPoint = SubSizedSpherePoint();
			RaycastHit hit;
			Color rayColor = Color.red;
			float normalizedPoint = i*1.0f / bleedPoints*1.0f;
			if (Physics.Raycast(worldMeshPoint, transform.position - worldMeshPoint, out hit, sphereSize))
			{
				Quaternion normalRotation = Quaternion.LookRotation((hit.normal));
				
				GameObject system = Instantiate(BubblePrefab(normalizedPoint), hit.point, normalRotation);
				system.transform.SetParent(transform);
				instances.Add(system);
				rayColor = Color.green;
			}
			
			Debug.DrawRay(worldMeshPoint, transform.position-worldMeshPoint,rayColor,1);
		}
		MovePointsIn();
	}
	
	/// <summary>
	/// Gets a random point on a sphere that surrounds the ship entirely
	/// </summary>
	Vector3 SubSizedSpherePoint()
	{
		if (sphereSize == 0)
		{
			Bounds shipBounds = Calc.GetColliderBounds(gameObject);
			
			sphereSize = Mathf.Max(shipBounds.extents.x, shipBounds.extents.y,shipBounds.extents.z);
		}

		return transform.position + Random.onUnitSphere * sphereSize;
	}
	
	/// <summary>
	/// Resets the instances list, kills the instances
	/// </summary>
	void ResetInstances()
	{
		foreach (GameObject ps in instances)
		{
			if (ps == null) continue;
			DestroyImmediate(ps.gameObject);
		}
		instances.Clear();
	}

	private void OnDrawGizmosSelected()
	{
		if(sphereSize>0)
			Gizmos.DrawWireSphere(transform.position, sphereSize);

		Gizmos.color = Color.cyan;
		foreach (GameObject ps in instances)
		{
			if (ps == null) continue;
			Gizmos.DrawLine(ps.transform.position, ps.transform.position+ps.transform.forward*stepSize);
		}
	}

	#endregion

	void ChangeIntensity()
	{
		ChangeIntensity(bleedIntensity);
	}
	
	/// <summary>
	/// Changes the intensity
	/// </summary>
	/// <param name="intensity"></param>
	void ChangeIntensity(float intensity)
	{
		if (intensity < bleedBuffer) return;
		float instancePercent = 1.0f/ instances.Count*1.0f ;
		float intensityAmount = intensity *  instances.Count;
	
		
		for (int i = 0; i < instances.Count; i++)
		{
			//Debug.Log(i + " / " + intensityAmount);
			GameObject ps = instances[i];
			if (ps == null) continue;
			if(intensityAmount<=i)
				ps.gameObject.SetActive(false);
			else
				ps.gameObject.SetActive(true);

			if (!ps.gameObject.activeInHierarchy) continue;
		
		}
	}
	

}
