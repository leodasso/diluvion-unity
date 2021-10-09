using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HeavyDutyInspector;

public class ApplyMaterialToAll : MonoBehaviour {

	public List<Material> materials;
	[Button("Apply", "Apply", true)] public bool hidden1;

	// Use this for initialization
	void Start () {
	
	}

	void Apply() {
		if (materials == null) 		return;
		if (materials.Count < 1) return;

		foreach (MeshRenderer m in GetComponentsInChildren<MeshRenderer>()) {

			// Find the associated mesh
			MeshFilter mf = m.GetComponent<MeshFilter>();
			if (mf == null) continue;

			// Get the number of submeshes ( and thus materials) used by this renderer
			int subMeshes = mf.sharedMesh.subMeshCount; 

			// Create an array with that number of materials based on the list
			Material[] tempMats = new Material[subMeshes];
			for (int i = 0; i < subMeshes; i++) {
				tempMats[i] = materials[i];
			}

			// Apply the list of materials to the selected mesh renderer
			m.sharedMaterials = tempMats;

		}
	}
}
