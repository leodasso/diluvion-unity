using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[AddComponentMenu("Mesh Sprite Sorting")]
public class MeshSpriteSorting : MonoBehaviour {
	[Header("Sort Settings")]
	[Tooltip("Sprite sorting order.")]
	public int sortingOrder = -1000000000;

	[Tooltip("Sprite sorting layer.")]
	[UnluckSoftware.SortingLayer]
	public int sortingLayer;

	public Renderer targetRenderer;

#if UNITY_EDITOR
	[CustomEditor(typeof(MeshSpriteSorting))]
	public class SpriteCombinerInspector : Editor {
		public override void OnInspectorGUI() {
			DrawDefaultInspector();
			if (GUI.changed) {
				UpdateSorting();
			}	
		}

		public void OnEnable() {		
			UpdateSorting();
		}

		public void UpdateSorting() {
			MeshSpriteSorting s = (MeshSpriteSorting)target;
			if (s.targetRenderer == null) s.targetRenderer = s.GetComponent<Renderer>();
			if (s.sortingOrder == -1000000000) {
				s.sortingOrder = s.GetComponent<Renderer>().sortingOrder;
			}
			s.targetRenderer.sortingOrder = s.sortingOrder;
			s.targetRenderer.sortingLayerID = s.sortingLayer;
		}
	}
#endif

}
