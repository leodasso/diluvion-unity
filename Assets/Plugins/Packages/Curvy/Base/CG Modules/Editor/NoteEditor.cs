// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using UnityEditor;
using FluffyUnderware.Curvy.Generator.Modules;
using System.Collections.Generic;

namespace FluffyUnderware.CurvyEditor.Generator.Modules
{
    [CustomEditor(typeof(Note))]
    public class NoteEditor : CGModuleEditor<Note>
    {
        /*
        // Skip Label
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.UpdateIfDirtyOrScript();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Text"), new GUIContent(""));
            serializedObject.ApplyModifiedProperties();
                

        }
         */
    }




}
