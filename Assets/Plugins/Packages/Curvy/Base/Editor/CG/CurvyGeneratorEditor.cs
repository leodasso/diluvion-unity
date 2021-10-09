// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using UnityEditor;
using System.Collections;
using FluffyUnderware.Curvy.Generator;
using System.Collections.Generic;
using FluffyUnderware.DevToolsEditor.Extensions;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.CurvyEditor.Generator
{
    [CustomEditor(typeof(CurvyGenerator))]
    public class CurvyGeneratorEditor : CurvyEditorBase<CurvyGenerator>
    {
        
       

        protected override void OnCustomInspectorGUI()
        {
            GUILayout.Space(5);
            if (Target)
                EditorGUILayout.HelpBox("# of Modules: " + Target.Modules.Count,MessageType.Info);
        }

        public override void OnInspectorGUI()
        {
            if (IsPrefab)
            {
                EditorGUILayout.HelpBox("Curvy Generator Template", MessageType.Info);
            }
            else
            {

                GUILayout.BeginHorizontal(GUILayout.Height(24));
                if (GUILayout.Button(new GUIContent(CurvyStyles.OpenGraphTexture, "Edit Graph")))
                    CGGraph.Open(Target);

                if (GUILayout.Button(new GUIContent(CurvyStyles.DeleteTexture, "Clear Graph"), GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true)) && EditorUtility.DisplayDialog("Clear", "Clear graph?", "Yes", "No"))
                    Target.Clear();
                GUILayout.EndHorizontal();

                base.OnInspectorGUI();

                
            }
            
        }

       
    }

    
}
