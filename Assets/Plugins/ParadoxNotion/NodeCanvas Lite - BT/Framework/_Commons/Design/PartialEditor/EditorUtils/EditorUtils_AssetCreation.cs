#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;


namespace ParadoxNotion.Design{

    /// <summary>
    /// ScriptableObject asset creation tools
    /// </summary>

	partial class EditorUtils {

		///Create asset of type T and show or not the File Panel
		public static T CreateAsset<T> (bool displayFilePanel) where T : ScriptableObject {
			return (T)CreateAsset(typeof(T), displayFilePanel);
		}
		
		///Create asset of type T
		public static T CreateAsset<T> () where T : ScriptableObject {
			return (T)CreateAsset(typeof(T));
		}
		
		///Create asset of type T at target path
		public static T CreateAsset<T> (string path) where T : ScriptableObject {
			return (T)CreateAsset(typeof(T), path);
		}

		///Create asset of type and show or not the File Panel
		public static ScriptableObject CreateAsset(System.Type type, bool displayFilePanel){
			ScriptableObject asset = null;
			var path = EditorUtility.SaveFilePanelInProject (
	         	       "Create Asset of type " + type.ToString(),
	            	   	type.Name + ".asset",
	                	"asset", "");
			asset = CreateAsset(type, path);
			return asset;
		}
		
		///Create asset of type
		public static ScriptableObject CreateAsset(System.Type type){
			var asset = CreateAsset(type, GetAssetUniquePath(type.Name + ".asset") );
			return asset;
		}
		
		///Create asset of type at target path
		public static ScriptableObject CreateAsset(System.Type type, string path){
			if (string.IsNullOrEmpty(path))
				return null;
			ScriptableObject data = null;
			data = ScriptableObject.CreateInstance(type);
			AssetDatabase.CreateAsset(data, path);
			AssetDatabase.SaveAssets();
			return data;
		}

		
		///Get a unique path at current project selection for creating an asset, providing the "filename.type"
		public static string GetAssetUniquePath(string fileName){
			var path = AssetDatabase.GetAssetPath (Selection.activeObject);
			if (path == "")
			    path = "Assets";
			if (System.IO.Path.GetExtension(path) != "")
			    path = path.Replace(System.IO.Path.GetFileName (AssetDatabase.GetAssetPath (Selection.activeObject)), "");
			return AssetDatabase.GenerateUniqueAssetPath(path + "/" + fileName);
		}

/*
		//Nice hack to attach a ScriptableObject to a game object. Used to work up to Unity version 5.5
		class Dummy : MonoBehaviour{}
		public static T AddScriptableComponent<T>(GameObject target) where T:ScriptableObject{
			return (T)AddScriptableComponent(target, typeof(T));
		}

		public static ScriptableObject AddScriptableComponent(GameObject target, System.Type type){

			var scriptable = ScriptableObject.CreateInstance(type);
			var monoScript = MonoScript.FromScriptableObject(scriptable);
			Object.DestroyImmediate(scriptable, true);
			if (monoScript == null){
				Debug.LogError(string.Format("Can't resolve MonoScript for ScriptableObject type '{0}'", type.Name));
				return null;
			}
			
			var dummy = target.AddComponent(typeof(Dummy));
			Undo.RegisterCreatedObjectUndo(dummy, "IScriptableComponent");
			//Undo.RecordObject(dummy, "IScriptableComponent");
			var id = dummy.GetInstanceID();

			var serObject = new SerializedObject(dummy);
			var scriptProp = serObject.FindProperty("m_Script");
			scriptProp.objectReferenceValue = monoScript;
			serObject.ApplyModifiedProperties();
			serObject.UpdateIfDirtyOrScript();
			//Object.DestroyImmediate(dummy, true);

			var o = (ScriptableObject)EditorUtility.InstanceIDToObject(id);
			return o;
		}
*/
	}
}


#endif