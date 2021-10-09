using UnityEngine;
using UnityEditor;

namespace BlendModes
{
	[InitializeOnLoad, CustomEditor(typeof(CameraOverlay)), CanEditMultipleObjects]
	public class CameraOverlayEditor : Editor
	{
		private GUIContent blendModeContent = new GUIContent("Blend Mode", "Blend mode of the overlay texture.");
		private GUIContent overlayTextureContent = new GUIContent("Overlay Texture", "Texture to use as overlay.");
		private GUIContent overlayTintContent = new GUIContent("Overlay Tint", "Tint color of the overlay.");

		private SerializedProperty blendMode;
		private SerializedProperty overlayTexture;
		private SerializedProperty overlayTint;

		private void OnEnable ()
		{
			blendMode = serializedObject.FindProperty("_blendMode");
			overlayTexture = serializedObject.FindProperty("_overlayTexture");
			overlayTint = serializedObject.FindProperty("_tintColor");

			Undo.undoRedoPerformed += SyncParameters;
		}

		private void OnDisable ()
		{
			Undo.undoRedoPerformed -= SyncParameters;
		}

		public override void OnInspectorGUI ()
		{
			if (!Selection.activeGameObject) return;

			var cameraOverlay = Selection.activeGameObject.GetComponent<CameraOverlay>();
			if (!cameraOverlay) return;

				serializedObject.Update();

				EditorGUILayout.Space();
				EditorGUILayout.PropertyField(blendMode, blendModeContent);
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(EditorGUIUtility.labelWidth);
				if (GUILayout.Button("<< previous", EditorStyles.miniButton, GUILayout.MinWidth(80)))
				{
					int blendModeIndex = blendMode.enumValueIndex;
					blendModeIndex--;
					if (blendModeIndex < 0)
						blendModeIndex = System.Enum.GetNames(typeof(BlendMode)).Length - 1;
					blendMode.enumValueIndex = blendModeIndex;
				}
				if (GUILayout.Button("next >>", EditorStyles.miniButton, GUILayout.MinWidth(80)))
				{
					int blendModeIndex = blendMode.enumValueIndex;
					blendModeIndex++;
					if (blendModeIndex >= System.Enum.GetNames(typeof(BlendMode)).Length)
						blendModeIndex = 0;
					blendMode.enumValueIndex = blendModeIndex;
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space();
	
				EditorGUILayout.PropertyField(overlayTexture, overlayTextureContent);
				EditorGUILayout.PropertyField(overlayTint, overlayTintContent);

				serializedObject.ApplyModifiedProperties();

				if (GUI.changed) SyncParameters();
		}

		private void SyncParameters ()
		{
			foreach (var selectedGO in Selection.gameObjects)
			{
				var cameraOverlay = selectedGO.GetComponent<CameraOverlay>();
				if (!cameraOverlay) continue;

				cameraOverlay.BlendMode = cameraOverlay.BlendMode;
				EditorUtility.SetDirty(selectedGO);
			}
		}
	}
}
