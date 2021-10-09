#if UNITY_ANDROID
using UnityEditor.Callbacks;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace I2.Loc
{
	public class PostProcessBuild_Android
	{
        // Post Process Scene is a hack, because using PostProcessBuild will be called after the APK is generated, and so, I didn't find a way to copy the new files
        [PostProcessScene]
        public static void OnPostProcessScene()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode &&
              (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android) &&
              (SceneManager.GetActiveScene().buildIndex <= 0))
            {
                string projPath = System.IO.Path.GetFullPath( Application.streamingAssetsPath + "/../../Temp/StagingArea");
                PostProcessAndroid(BuildTarget.Android, projPath);
            }
        }

        //[PostProcessBuild]
        public static void PostProcessAndroid(BuildTarget buildTarget, string pathToBuiltProject)
		{
			if (buildTarget!=BuildTarget.Android && buildTarget!=BuildTarget.Tizen)
				return;

            if (LocalizationManager.Sources.Count <= 0)
				LocalizationManager.UpdateSources();
			var langCodes = LocalizationManager.GetAllLanguagesCode(false);
			if (langCodes.Count <= 0)
				return;
			string stringXML =  "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"+
								"<resources>\n"+
								"    <string name=\"app_name\">{0}</string>\n"+
								"</resources>";

            SetStringsFile( pathToBuiltProject+"/res/values", "strings.xml", stringXML, LocalizationManager.GetAppName(langCodes[0]) );


			var list = new List<string>();
			list.Add( pathToBuiltProject + "/res/values" );
			foreach (var code in langCodes)
			{
				string dir = pathToBuiltProject + "/res/values-" + code;

                SetStringsFile( dir, "strings.xml", stringXML, LocalizationManager.GetAppName(code) );
			}
		}

		static void CreateFileIfNeeded ( string folder, string fileName, string text )
		{
			try
			{
				if (!System.IO.Directory.Exists( folder ))
					System.IO.Directory.CreateDirectory( folder );

				if (!System.IO.File.Exists( folder + "/"+fileName ))
					System.IO.File.WriteAllText( folder + "/"+fileName, text );
			}
			catch (System.Exception e)
			{
				Debug.Log( e );
			}
		}

        static void SetStringsFile(string folder, string fileName, string stringXML, string appName)
        {
            try
            {
                if (!System.IO.Directory.Exists(folder))
                    System.IO.Directory.CreateDirectory(folder);

                if (!System.IO.File.Exists(folder + "/" + fileName))
                {
                    // create the string file if it doesn't exist
                    stringXML = string.Format(stringXML, appName);
                }
                else
                {
                    stringXML = System.IO.File.ReadAllText(folder + "/" + fileName);
                    // find app_name
                    var pattern = "\"app_name\">(.*)<\\/string>";
                    var regexPattern = new System.Text.RegularExpressions.Regex(pattern);
                    if (regexPattern.IsMatch(stringXML))
                    {
                        // Override the AppName if it was found
                        stringXML = regexPattern.Replace(stringXML, string.Format("\"app_name\">{0}</string>", appName));
                    }
                    else
                    {
                        // insert the appName if it wasn't there
                        int idx = stringXML.IndexOf("<resources>");
                        if (idx > 0)
                            stringXML = stringXML.Insert(idx + "</resources>".Length, string.Format("\n    <string name=\"app_name\">{0}</string>\n", appName));
                    }
                }
                System.IO.File.WriteAllText(folder + "/" + fileName, stringXML);
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }
        }
    }
}
#endif