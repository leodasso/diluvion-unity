#define HideInternals

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Valkyrie.VPaint;

public class VPaintObjectNotDynamicException : Exception {
	public override string Message {
		get {
			return "Method called requires VPaint Object to be dynamic. Set VPaintObject.isDynamic to true before calling this method.";
		}
	}
}

public delegate Color VPaintObjectPositionalModifier (Color color, float distance);

[AddComponentMenu("VPaint/VPaint Object")]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class VPaintObject : MonoBehaviour, IVPaintIdentifier //, IVertexPaintable 
{
	
	public static List<VPaintObject> all = new List<VPaintObject>();
	public static List<VPaintObject> OverlapSphere (Vector3 position, float radius) 
	{	
		float sqrRadius = radius * radius;
		List<VPaintObject> selected = new List<VPaintObject>();

		foreach(VPaintObject vc in all)
		{
			if(!vc) continue;

			var renderer = vc.GetComponent<Renderer>();
			if(renderer.bounds.SqrDistance(position) < sqrRadius)
				selected.Add(vc);
		}
		return selected;
	}	
	
#if HideInternals
	[HideInInspector] 
#endif
	public Mesh _mesh;
	
	[NonSerialized]
	public Mesh _meshNonSerialized; //non serialized reference because of that asshole Undo

#if HideInternals
	[HideInInspector] 
#endif
	public Material originalMaterial;
	
#if HideInternals
	[HideInInspector] 
#endif
	public Mesh originalMesh;
	
	[NonSerialized] public Color[] colorsBuilder;
	[NonSerialized] public float[] transparencyBuilder;
	[NonSerialized] public int index;
	
	[NonSerialized]	public Color[] myColors;
	[NonSerialized] public Vector3[] myVertices;

#if HideInternals
	[HideInInspector] 
#endif
	public MeshCollider editorCollider;
	
	MeshRenderer _meshRenderer;
	MeshRenderer meshRenderer {
		get{
			if(!_meshRenderer) _meshRenderer = GetComponent<MeshRenderer>();
			return _meshRenderer;
		}
	}
	
	MeshFilter _meshFilter;
	MeshFilter meshFilter {
		get{
			if(!_meshFilter) _meshFilter = GetComponent<MeshFilter>();
			return _meshFilter;
		}
	}
	
	[SerializeField] bool _isDynamic;
	public bool isDynamic 
	{
		get{ return _isDynamic; }
		set{
			if(value != _isDynamic)
			{
				if(!value && editorCollider)
					GameObject.Destroy(editorCollider.gameObject);
				else {
					CreateDynamicCollider();
				}
					
				value = _isDynamic;
			}
		}
	}
	void CreateDynamicCollider ()
	{
		GameObject go = new GameObject(name + " Collider");
		go.hideFlags = HideFlags.HideInHierarchy;
		editorCollider = go.AddComponent<MeshCollider>();
		editorCollider.sharedMesh = originalMesh;
	}
	
	public void OnApplicationQuit () {
		OnDestroy();
	}
	
	public void OnDestroy () {
		if(_mesh && _mesh != originalMesh) DestroyImmediate(_mesh);
		all.Remove(this);
		if(editorCollider) GameObject.DestroyImmediate(editorCollider.gameObject);
	}
	
	public void OnEnable ()
	{
		if(!Application.isPlaying)
		{
			if(!_mesh) return;
			var others = GameObject.FindObjectsOfType(typeof(VPaintObject));
			foreach(var o in others)
			{
				var vp = o as VPaintObject;
				if(vp == this) continue;
				if(vp._mesh == _mesh)
				{
					_mesh = null;
					ResetInstances();
					break;
				}
			}
		}
	} 
	
	public void Awake ()
	{
		if(!Application.isPlaying) return;
		if(_isDynamic)
		{
			CreateDynamicCollider();
		}
	}
	
	/*IVPaintObject Interface*/
	public Color[] GetDefaultColors ()
	{
		return GetMeshInstance().colors;
	}
	public Vector3[] GetVertices ()
	{
		return GetMeshInstance().vertices;
	}
	public void SetColors (Color[] colors)
	{
		Mesh m = GetMeshInstance();
		if(colors.Length != myVertices.Length)
		{
			for(int i = 0; i < colors.Length; i++)
			{
				colors[i] = Color.magenta;
			}
			Debug.LogWarning("Invalid vertex colors assigned to " + name + ". Check the Maintenance window for your VPaint Group for more info.");
			return;
		}
		for(int i = 0; i < colors.Length; i++)
			myColors[i] = colors[i];
		m.colors = myColors;
	}
	
	public bool IsEqualTo (IVPaintIdentifier obj)
	{
		return obj == this;
	}
	/* */
	
	public Bounds GetBounds ()
	{
		if(editorCollider) return editorCollider.bounds;
		var renderer = GetComponent<Renderer>();
		return renderer.bounds;
	}
	
	public void ApplyColorsBuilder ()
	{
		if(colorsBuilder == null) return;
		if(transparencyBuilder == null) return;
		
		Mesh m = GetMeshInstance();
		m.colors = colorsBuilder;
		myColors = colorsBuilder;
		colorsBuilder = null;
		transparencyBuilder = null;
	}
	
	public Mesh GetMeshInstance ()
	{
		if(!this) return null;
		if(!meshFilter) return null;
		
		if(!_mesh){
			if(_meshNonSerialized)
			{
				_mesh = _meshNonSerialized;
#if UNITY_EDITOR
				UnityEditor.EditorUtility.SetDirty(this);
#endif
			}
			else
			{
				if(!meshFilter.sharedMesh && !originalMesh) return null;
				if(originalMesh) meshFilter.sharedMesh = originalMesh;
				originalMesh = meshFilter.sharedMesh;
				var vdc = GetComponent<VertexDataCache>();
				if(vdc) _mesh = vdc.GetMeshInstance();
				else _mesh = GameObject.Instantiate(meshFilter.sharedMesh) as Mesh;
				meshFilter.sharedMesh = _mesh;
				myVertices = _mesh.vertices;
				var cols = _mesh.colors;
				if(cols == null || cols.Length != myVertices.Length)
				{
					Color[] colors = new Color[myVertices.Length];
					myColors = colors;
					_mesh.colors = colors;
				}
				if(_mesh.uv2.Length == 0)
				{
					_mesh.uv2 = _mesh.uv;
				}
//				if(_mesh.normals.Length == 0)
//					_mesh.RecalculateNormals();
				_mesh.RecalculateBounds();
#if UNITY_EDITOR
				UnityEditor.EditorUtility.SetDirty(this);
#endif
			}
		}
		meshFilter.sharedMesh = _mesh;
		
		_meshNonSerialized = _mesh;
		if(myVertices == null) myVertices = _mesh.vertices;
		if(myColors == null) myColors = _mesh.colors;
		return _mesh;
	}
	
	public void ResetInstances ()
	{
		if(_mesh && _mesh != originalMesh) GameObject.DestroyImmediate(_mesh);
		
		_mesh = null;
		_meshNonSerialized = null;
		
		MeshFilter mf = GetComponent<MeshFilter>();
		if(!mf) return;
		
		mf.sharedMesh = originalMesh;
		
		ResetMaterial();
	}
	
	public void SetTangents (Color[] colors)
	{
		Mesh m = GetMeshInstance();
		if(colors.Length != myVertices.Length)
		{
			Debug.LogWarning("Colors length of " + name + " is different than vertices length");
			return;
		}
		Vector4[] tgts = new Vector4[colors.Length];
		for(int i = 0; i < tgts.Length; i++)
			tgts[i] = (Vector4)colors[i];
		m.tangents = tgts;
	}
	
	public void FloodColors (Color color)
	{
		var m = GetMeshInstance();
		var c = myColors;
		if(c == null) c = m.colors;
		if(c == null || c.Length == 0) c = new Color[m.vertices.Length];
		for(int i = 0; i < c.Length; i++)
			c[i] = color;
		m.colors = c;
	}
		
	public void ApplyPositionalModifier (Vector3 worldPosition, VPaintObjectPositionalModifier modifier) 
	{
		Mesh m = GetMeshInstance();
		
		Vector3[] vertices = myVertices;
		Color[] colors = myColors;
		if(colors == null) colors = m.colors;
		
		var len = vertices.Length;
		
		for(int i = 0; i < len; i++)
		{
			Vector3 v = transform.TransformPoint(vertices[i]);
			float distance = Vector3.Distance(v, worldPosition);			
			colors[i] = modifier(colors[i], distance);
		}

		m.colors = colors;
		myColors = colors;
	}
	
	public IEnumerator ApplyPositionalModifierAsync (Vector3 worldPosition, VPaintObjectPositionalModifier modifier, float time)
	{
		Mesh m = GetMeshInstance();
		
		Vector3[] vertices = myVertices;
		Color[] colors = myColors;
		if(colors == null) colors = m.colors;
		
		var len = vertices.Length;
		
		Color[] target = new Color[len];
		for(int i = 0; i < len; i++)
		{
			Vector3 v = transform.TransformPoint(vertices[i]);
			float distance = Vector3.Distance(v, worldPosition);			
			target[i] = modifier(colors[i], distance);
		}
		
		var routine = VPaintUtility.LerpColors(this, colors, target, time);
		while(routine.MoveNext()) yield return null;
	}
	
	public void ApplyColorSpherical (Color color, Vector3 worldPosition, float radius, float falloff, float strength)
	{		
		ApplyPositionalModifier(worldPosition,
		(c, d)=>
		{
			if(radius < d) return c;
			
			float f = Mathf.Pow(1 - (d / radius), falloff) * strength;
			return Color.Lerp(c, color, f);
			
		});
	}
	
	public void ApplyAlphaSpherical (float alpha, Vector3 worldPosition, float radius, float falloff, float strength)
	{		
		ApplyPositionalModifier(worldPosition,
		(c, d)=>
		{
			if(radius < d) return c;
			
			float f = Mathf.Pow(1 - (d / radius), falloff) * strength;
			
			c.a = Mathf.Lerp(c.a, alpha, f);
			
			return c;
		});
	}
	
	Material instancedMaterial;
	public void SetInstanceMaterial (Material m)
	{
		var renderer = GetComponent<Renderer>();
		if(!renderer) return;
		if(!originalMaterial) originalMaterial = renderer.sharedMaterial;
		renderer.sharedMaterial = m;
		instancedMaterial = m;
	}
	public void ResetMaterial ()
	{
		var renderer = GetComponent<Renderer>();
		if(!renderer) return;
		
		if(renderer.sharedMaterial != instancedMaterial) originalMaterial = renderer.sharedMaterial;
		else if(originalMaterial) renderer.sharedMaterial = originalMaterial;
	}
	
}