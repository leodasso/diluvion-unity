// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.Curvy;
using FluffyUnderware.DevTools.Extensions;
using FluffyUnderware.DevToolsEditor;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.CurvyEditor
{

    public class CurvyEditorUtility
    {
        public static string HelpURL(string comp) { return HelpURL(comp, null); }
        public static string HelpURL(string comp, string anchor)
        {
            if (string.IsNullOrEmpty(comp))
                return string.Empty;
            return CurvySpline.DOCLINK + "legacy";
        }

        public static void SendBugReport()
        {
            string par =string.Format("@Operating System@={0}&@Unity Version@={1}&@Curvy Version@={2}", SystemInfo.operatingSystem, Application.unityVersion,CurvySpline.VERSION);
            Application.OpenURL(CurvySpline.WEBROOT + "bugreport?"+ par.Replace(" ", "%20"));
        }


        /// <summary>
        /// Converts a path/file relative to Curvy's root path to the real path, e.g. "ReadMe.txt" gives "Curvy/ReadMe.txt"
        /// </summary>
        /// <param name="relativePath">a path/file inside the Curvy package, WITHOUT the leading Curvy</param>
        /// <returns>the real path, relative to Assets</returns>
        public static string GetPackagePath(string relativePath)
        {
            return GetCurvyRootPath() + relativePath.TrimStart('/', '\\');
        }
        /// <summary>
        /// Converts a path/file relative to Curvy's root path to the real absolute path
        /// </summary>
        /// <param name="relativePath">a path/file inside the Curvy package, WITHOUT the leading Curvy</param>
        /// <returns>the absolute system path</returns>
        public static string GetPackagePathAbsolute(string relativePath)
        {
            return Application.dataPath + "/" + GetPackagePath(relativePath);
        }

        /// <summary>
        /// Gets the Curvy folder relative path, e.g. "Packages/Curvy/" by default
        /// </summary>
        /// <returns></returns>
        public static string GetCurvyRootPath()
        {
            // Quick check for the regular path
            if (System.IO.File.Exists(Application.dataPath + "/Packages/Curvy/Base/CurvySplineBase.cs"))
                return "Packages/Curvy/";
            
            // Still no luck? Do a project search
            var guid = AssetDatabase.FindAssets("curvysplinebase");
            if (guid.Length == 0)
            {
                DTLog.LogError("[Curvy] Unable to locate CurvySplineBase.cs in the project! Is the Curvy package fully imported?");
                return null;
            }
            else
                return AssetDatabase.GUIDToAssetPath(guid[0]).TrimStart("Assets/").TrimEnd("Base/CurvySplineBase.cs");
        }

        /// <summary>
        /// Gets the Curvy folder absolute path, i.e. Application.dataPath+"/"+CurvyEditorUtility.GetCurvyRootPath()
        /// </summary>
        /// <returns></returns>
        public static string GetCurvyRootPathAbsolute()
        {
            return Application.dataPath + "/"+ GetCurvyRootPath();
        }

        

    }

   
   

    public class CurvyGUI
    {

#region ### GUI Controls ###

        public static bool Foldout(ref bool state, string text) { return Foldout(ref state, new GUIContent(text), null); }
        public static bool Foldout(ref bool state, string text, string helpURL) { return Foldout(ref state, new GUIContent(text), helpURL); }

        public static bool Foldout(ref bool state, GUIContent content, string helpURL, bool hierarchyMode=true)
        {
            Rect r = GUILayoutUtility.GetRect(content, CurvyStyles.Foldout);
            
            r.y += 5;
            int lvl = EditorGUI.indentLevel;
            EditorGUI.indentLevel = Mathf.Max(0, lvl - 1);
            r = EditorGUI.IndentedRect(r);
            
            int off = (hierarchyMode) ? 12 : 0;
            
                r.x -= off;
            
            GUI.Box(new Rect(r.x, r.y-2, r.width+off, r.height+5), "", EditorStyles.toolbarButton);
            r.width = r.width + off - CurvyStyles.HelpTexture.width;
            
            state = GUI.Toggle(r, state, content, CurvyStyles.Foldout);
            if (state && !string.IsNullOrEmpty(helpURL))
            {
                r.x = r.xMax;
                r.width = CurvyStyles.HelpTexture.width;
                if (GUI.Button (r, new GUIContent (CurvyStyles.HelpTexture, "Help"), CurvyStyles.BorderlessButton))
                    Application.OpenURL(helpURL);
            }
            

            EditorGUI.indentLevel = lvl;

            return state;
        }

#endregion

    }
}

    
