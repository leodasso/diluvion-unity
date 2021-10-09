using UnityEngine;
using System.Collections;
using Valkyrie.VPaint;

//This class was created to bridge the gap between Unity 3.5 and Unity 4
// apparently, during the switch, the mesh import settings get messed up and the vert order changes
// this is a fix for that, so that the vertex colorer will grab the colors from here if possible

public class VertexDataCache : MonoBehaviour
{
	[HideInInspector] public Vector3[] vertices;
	[HideInInspector] public Vector3[] normals;
	[HideInInspector] public Vector4[] tangents;
	[HideInInspector] public Vector2[] uv1;
	[HideInInspector] public Vector2[] uv2;
	[HideInInspector] public int[] triangles;
	
	[ContextMenu("Cache")]
	public void Cache ()
	{
		var mf = GetComponent<MeshFilter>();
		if(!mf)
		{
			Debug.LogError("This method requires a mesh filter!");
			return;
		}
		
		var mesh = mf.sharedMesh;
		vertices = mesh.vertices;
		normals = mesh.normals;
		tangents = mesh.tangents;
		uv1 = mesh.uv;
		uv2 = mesh.uv2;
		triangles = mesh.triangles;
	}
	
	public Mesh GetMeshInstance ()
	{
		var mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.tangents = tangents;
		mesh.uv = uv1;
		mesh.uv2 = uv2;
		mesh.triangles = triangles;
		return mesh;
	}
}
