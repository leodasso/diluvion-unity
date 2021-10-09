//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2013 - 2014  Illogika
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using System.Collections;

public class InstallScriptTemplates{

	[MenuItem("File/Install Script Templates")]
	public static void InstallScriptTemplatesFunc()
	{
		string unityPath = EditorApplication.applicationPath;
		string originPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/Illogika/ScriptTemplates/");

		string[] paths = Directory.GetFiles(originPath);

		if(Application.platform == RuntimePlatform.OSXEditor)
		{
			unityPath = Path.Combine(unityPath, "Contents/Resources/ScriptTemplates");
		}
		else if(Application.platform == RuntimePlatform.WindowsEditor)
		{
			unityPath = Path.Combine(unityPath.Replace(Path.GetFileName(unityPath), ""), "Data/Resources/ScriptTemplates");
		}

		foreach(string path in paths)
		{
			if(Path.GetExtension(path) == ".txt")
			{
				if(File.Exists(Path.Combine(unityPath, Path.GetFileName(path))))
				   File.Delete(Path.Combine(unityPath, Path.GetFileName(path)));

				File.Copy(path, Path.Combine(unityPath, Path.GetFileName(path)));
				Debug.Log("Copied " + path + " to " + Path.Combine(unityPath, Path.GetFileName(path)));
			}
		}

		EditorUtility.DisplayDialog("Restart Required", "Unity will shut down to complete the installation. You will need to restart it manually.", "OK");
		
		EditorApplication.SaveCurrentSceneIfUserWantsTo();
		EditorApplication.Exit(1);
	}
}
