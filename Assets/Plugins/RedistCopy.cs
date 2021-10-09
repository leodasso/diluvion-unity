﻿// Comment this out to disable copying
//#define DISABLEREDISTCOPY

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

namespace Galaxy {

	// Copy GOG dlls to the appropriate place when building standalone players.
	// Note: Implemented as a direct mirror of the dll copy process implemeneted in the Steamworks.net library (https://github.com/rlabrecque/Steamworks.NET).
	public class RedistCopy {
		const string galaxyAPIRelativeLoc = "Assets/Plugins/GoGGalaxy";
		
		[PostProcessBuild]
		public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {

            #if !DISABLEREDISTCOPY
           // if (DiluvionGameState.Get().GetConfigData().publishingto != PublishingTo.GOG) return;
            if (target == BuildTarget.StandaloneWindows64) {
				string outputLibLocation = Path.GetDirectoryName (pathToBuiltProject);
				Debug.Log ("64: " + outputLibLocation);
				CopyFile ("Win64/Galaxy64.dll", Path.Combine (outputLibLocation, "Galaxy64.dll"));
				//CopyFile ("Win64/GalaxyPeer64.dll", Path.Combine (outputLibLocation, "GalaxyPeer64.dll"));
				CopyFile ("Win64/GalaxyCSharpGlue.dll", Path.Combine (outputLibLocation, "GalaxyCSharpGlue.dll"));
			}
			else if (target == BuildTarget.StandaloneWindows) {
				string outputLibLocation = Path.GetDirectoryName (pathToBuiltProject);
				Debug.Log ("32: " + outputLibLocation);
				CopyFile ("Win32/Galaxy.dll", Path.Combine (outputLibLocation, "Galaxy.dll"));
				//CopyFile ("Win32/GalaxyPeer.dll", Path.Combine (outputLibLocation, "GalaxyPeer.dll"));
				CopyFile ("Win32/GalaxyCSharpGlue.dll", Path.Combine (outputLibLocation, "GalaxyCSharpGlue.dll"));
			} 
			else if (target == BuildTarget.StandaloneOSX || target == BuildTarget.StandaloneOSXIntel || target == BuildTarget.StandaloneOSXIntel64) {
				pathToBuiltProject = pathToBuiltProject + "/"; // Ensure the .app directory name is actually treated as a directory.
				string outputLibLocation = Path.Combine (pathToBuiltProject, "Contents/Frameworks/MonoEmbedRuntime/osx/");

				CopyFile ("OSXUniversal/Galaxy.bundle/Contents/MacOS/libGalaxy.dylib", Path.Combine (outputLibLocation, "libGalaxy.dylib"));
				//CopyFile ("OSXUniversal/Galaxy.bundle/Contents/MacOS/libGalaxyPeer.dylib", Path.Combine (outputLibLocation, "libGalaxyPeer.dylib"));
				CopyFile ("OSXUniversal/Galaxy.bundle/Contents/MacOS/libGalaxyCSharpGlue.dylib", Path.Combine (outputLibLocation, "libGalaxyCSharpGlue.dylib"));
			}
			#endif
		}
		
		private static void CopyFile (string sourceFilePath, string outputFilePath) {
			string strCWD = Directory.GetCurrentDirectory();
			string strSource = Path.Combine(Path.Combine(strCWD, galaxyAPIRelativeLoc), sourceFilePath);
			string strFileDest = outputFilePath;
			
			if (!File.Exists(strSource)) {
				Debug.LogWarning(string.Format("[GoGGalaxy RedistCopy] Could not copy {0} into the project root. {0} could not be found in '{1}'. Place {0} from the redist into the project root manually.", sourceFilePath, galaxyAPIRelativeLoc));
				return;
			}
			
			if (File.Exists(strFileDest)) {
				if (File.GetLastWriteTime(strSource) == File.GetLastWriteTime(strFileDest)) {
					FileInfo fInfo = new FileInfo(strSource);
					FileInfo fInfo2 = new FileInfo(strFileDest);
					if (fInfo.Length == fInfo2.Length) {
						return;
					}
				}
			}
			
			File.Copy(strSource, strFileDest, true);
			File.SetAttributes(strFileDest, File.GetAttributes(strFileDest) & ~FileAttributes.ReadOnly);
			
			if (!File.Exists(strFileDest)) {
				Debug.LogWarning(string.Format("[GoGGalaxy RedistCopy] Could not copy {0} into the built project. File.Copy() Failed. Place {0} from the redist folder into the output dir manually.", sourceFilePath));
			}
		}
	}
}

#endif
