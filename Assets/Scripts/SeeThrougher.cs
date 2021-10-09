using System.Collections;
using System.Collections.Generic;
using Diluvion;
using UnityEngine;
using Sirenix.OdinInspector;

public class SeeThrougher : MonoBehaviour
{

	public bool affectChildren = false;
	
	[Range(0, 1)]
	[OnValueChanged("RefreshOpacity")]
	public float opacity = 1;
	
	void RefreshOpacity()
	{
		SetOpacity(opacity);
	}
	
	[ReadOnly]
	public Shader seeThruShader;
	public MeshRenderer meshRenderer;

	[ReadOnly]
	public bool altered;

	private Material sharedMat;
	private Material newMat;

	List<SeeThrougher> children = new List<SeeThrougher>();

	private void Awake()
	{
		seeThruShader = GameManager.Get().stippleShader;
	}

	/// <summary>
	/// This component will control the opacity of all child mesh renderers.
	/// </summary>
	public void SetAsMaster()
	{
		affectChildren = true;
		AddToChildMeshRenderers();
	}

	public void LerpOpacity(float newOpacity)
	{
		if (!gameObject.activeInHierarchy) return;
		StopAllCoroutines();
		StartCoroutine(LerpOpacityRoutine(newOpacity));
	}

	IEnumerator LerpOpacityRoutine(float newOpacity)
	{
		float prog = 0;
		float oldOpacity = opacity;

		while (prog < 1)
		{
			prog += Time.unscaledDeltaTime * 4;
			opacity = Mathf.Lerp(oldOpacity, newOpacity, prog);
			SetOpacity(opacity);
			yield return null;
		}
		
		SetOpacity(newOpacity);
	}


	[ButtonGroup("create")]
	public void AddToChildMeshRenderers()
	{
	
		List<Renderer> renders = new List<Renderer>();
		renders.AddRange(GetComponentsInChildren<MeshRenderer>());
		renders.AddRange(GetComponentsInChildren<TrailRenderer>());
		
		foreach (var VARIABLE in renders)
		{
			if (VARIABLE.GetComponent<SeeThrougher>()) continue;
			children.Add(VARIABLE.gameObject.AddComponent<SeeThrougher>());
		}
	}

	[ButtonGroup("create")]
	void ClearFromChildren()
	{
		foreach (var VARIABLE in children)
		{
			DestroyImmediate(VARIABLE);
		}
	}


	[Button]
	void SetSeeThrough()
	{
		if (!meshRenderer) meshRenderer = GetComponent<MeshRenderer>();
		if (!meshRenderer) return;
		if (altered) return;

		altered = true;

		sharedMat = meshRenderer.sharedMaterial;


		if (!newMat)
		{
			meshRenderer.material.shader = seeThruShader;
			newMat = meshRenderer.material;
		}
		else
		{
			meshRenderer.material = newMat;
		}
	}

	[Button]
	void ReturnToNormal()
	{
		if (!meshRenderer) meshRenderer = GetComponent<MeshRenderer>();
		if (!meshRenderer) return;
		if (!sharedMat) return;

		altered = false;
		meshRenderer.material = sharedMat;
	}

	public void SetOpacity(float value)
	{
		value = Mathf.Clamp01(value);

		if (affectChildren)
		{
			foreach (var VARIABLE in  children)
			{
				if (VARIABLE == null) continue;
				VARIABLE.SetOpacity(value);
			}
		}

		if (value < .98f)
		{
			SetSeeThrough();
			
			Color c = new Color(1, 1, 1, value);
		
			if (meshRenderer)
				meshRenderer.material.SetColor("_Color", c);
		}
		
		else ReturnToNormal();

	}
}
