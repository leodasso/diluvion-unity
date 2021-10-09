using UnityEngine;
using UnityEditor;
using UnityEditorInternal; // Don't remove!
using System.Collections.Generic;

namespace BlendModes
{
	[InitializeOnLoad, CustomEditor(typeof(BlendModeEffect)), CanEditMultipleObjects]
	public class BmEffectEditor : Editor
	{
		private bool showEditor, showRenderMode, showMaterialParams, showMaskMode, showMaskBehaviour, showFramebufferInfo, showUnityProWarning;

		private GUIContent blendModeContent = new GUIContent("Blend Mode", "Blend mode of the object.");
		private GUIContent renderModeContent = new GUIContent("Render Mode",
			"The mode in which blend mode effect is executed.\n\nGrab: performs a grab pass for each object with blend mode effect per frame, which may cause a significant performance drop when applying the effect to lots of objects simultaneously.\n\nUnified Grab: uses a shared grab texture and perform only one grab pass per frame for all the objects in this mode, which will yield better performance when using multiple instances of the effect. The only drawback is that objects in Unified Grab mode won't blend with each other.\n\nFramebuffer: utilizes special OpenGL extension instead of the grab pass to access framebuffer, which is extremely faster and will work smoothly on mobile devices. The device has to support framebuffer_fetch extension for this to work. Consult the documentation for more info.");
		private GUIContent textureContent = new GUIContent("Texture", "Texture of the object.");
		private GUIContent tintColorContent = new GUIContent("Tint Color", "Tint color of the object.");
		private GUIContent maskModeContent = new GUIContent("Mask Mode", "Use to configure masking.\n\nDisabled: ignore masks, blend with everything (default).\n\nNothing But Mask: blend only with objects affected by mask.\n\nEverything But Mask: blend with everything, except objects affected by mask.");
		private GUIContent maskBehaviourContent = new GUIContent("Mask Behaviour", "Controls what should be rendered in the masked areas.\n\nCutout: pixels from the masked areas will be discarded (become transparent).\n\nNormal: pixels from the masked areas will be rendered in normal blend mode.");
		private GUIContent enableAutoMaskContent = new GUIContent("Auto Mask", "Whether the object should be considered as a mask.");

		private SerializedProperty blendMode;
		private SerializedProperty renderMode;
		private SerializedProperty texture;
		private SerializedProperty tintColor;
		private SerializedProperty maskMode;
		private SerializedProperty maskBehaviour;
		private SerializedProperty enableAutoMask;

		private static Dictionary<GameObject, bool> affectedObjects = new Dictionary<GameObject, bool>();

		static BmEffectEditor ()
		{
			EditorApplication.update += Update;
		}

		// Hack to control prefab objects material when adding/removing BlendModeEffect component. 
		// If only prefabs would fire OnEnable/Disable events...
		static void Update ()
		{
			foreach (var selectedGo in Selection.gameObjects)
			{
				if (PrefabUtility.GetPrefabType(selectedGo) != PrefabType.Prefab) continue;

				var blendEffect = selectedGo.GetComponent<BlendModeEffect>();
				if (!blendEffect)
				{
					// User removed BlendModeEffect.
					if (affectedObjects.ContainsKey(selectedGo))
					{
						var tempBmComponent = selectedGo.AddComponent<BlendModeEffect>();
						tempBmComponent.OnEnable();
						tempBmComponent.OnDisable();
						DestroyImmediate(tempBmComponent, true);
						affectedObjects.Remove(selectedGo);
						EditorUtility.SetDirty(selectedGo);
					}
					continue;
				}
				else
				{
					// First launch or user added BlendModeEffect.
					if (!affectedObjects.ContainsKey(selectedGo))
					{
						blendEffect.OnEnable();
						affectedObjects.Add(selectedGo, true);
						EditorUtility.SetDirty(selectedGo);
					}

					// User enabled/disabled BlendModeEffect.
					if (affectedObjects[selectedGo] != blendEffect.enabled)
					{
						if (blendEffect.enabled) blendEffect.OnEnable();
						else blendEffect.OnDisable();
						affectedObjects[selectedGo] = blendEffect.enabled;
						EditorUtility.SetDirty(selectedGo);
					}
				}
			}
		}

		private void OnEnable ()
		{
			blendMode = serializedObject.FindProperty("_blendMode");
			renderMode = serializedObject.FindProperty("_renderMode");
			texture = serializedObject.FindProperty("_texture");
			tintColor = serializedObject.FindProperty("_tintColor");
			maskMode = serializedObject.FindProperty("_maskMode");
			maskBehaviour = serializedObject.FindProperty("_maskBehaviour");
			enableAutoMask = serializedObject.FindProperty("_enableAutoMask");

			Undo.undoRedoPerformed += SyncParameters;
		}

		private void OnDisable ()
		{
			Undo.undoRedoPerformed -= SyncParameters;
		}

		public override void OnInspectorGUI ()
		{
			if (!Selection.activeGameObject) return;

			var blendEffect = Selection.activeGameObject.GetComponent<BlendModeEffect>();
			if (!blendEffect) return;

			if (Event.current.type == EventType.Layout)
			{
				showEditor = blendEffect.ObjectType != ObjectType.Unknown;
				showMaterialParams = blendEffect.ObjectType == ObjectType.MeshDefault || 
					blendEffect.ObjectType == ObjectType.ParticleDefault;
				showMaskMode = blendEffect.BlendMode != BlendMode.Normal;
				showMaskBehaviour = blendEffect.MaskMode != MaskMode.Disabled;
				showFramebufferInfo = blendEffect.RenderMode == RenderMode.Framebuffer;
				showUnityProWarning = ShowUnityProWarning();
			}

			if (showEditor)
			{
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

				EditorGUILayout.PropertyField(renderMode, renderModeContent);

				if (showMaskMode)
				{
					EditorGUILayout.PropertyField(maskMode, maskModeContent);
					if (showMaskBehaviour) EditorGUILayout.PropertyField(maskBehaviour, maskBehaviourContent);
				}
				EditorGUILayout.PropertyField(enableAutoMask, enableAutoMaskContent);

				if (showMaterialParams)
				{
					EditorGUILayout.PropertyField(texture, textureContent);
					EditorGUILayout.PropertyField(tintColor, tintColorContent);
				}

				serializedObject.ApplyModifiedProperties();

				if (GUI.changed) SyncParameters();

				if (showFramebufferInfo) 
					EditorGUILayout.HelpBox("Framebuffer mode will be active on mobile devices with framebuffer_fetch extension support. While in editor, Grab mode will be used for preview.", MessageType.Info);

				if (showUnityProWarning)
					EditorGUILayout.HelpBox("Grab and Unified Grab modes require Unity 4 Pro license to work correctly.", MessageType.Warning);
			}
			else EditorGUILayout.HelpBox("Can't find a compatible renderer component to apply blend mode effect.", MessageType.Warning);
		}

		private void SyncParameters ()
		{
			foreach (var selectedGO in Selection.gameObjects)
			{
				var blendEffect = selectedGO.GetComponent<BlendModeEffect>();
				if (!blendEffect) continue;

				blendEffect.SetBlendMode(blendEffect.BlendMode, blendEffect.RenderMode);
				EditorUtility.SetDirty(selectedGO);
			}
		}

		private bool ShowUnityProWarning ()
		{
			if (Application.unityVersion[0] != '4') return false;

			if (!Application.HasProLicense()) return true;

			#if UNITY_IOS 
			if (!InternalEditorUtility.GetLicenseInfo().Contains("iPhone Pro")) return true;
			#endif

			#if UNITY_ANDROID
			if (!InternalEditorUtility.GetLicenseInfo().Contains("Android Pro")) return true;
			#endif

			return false;
		}
	}
}
