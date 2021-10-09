// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using FluffyUnderware.Curvy.Generator;
using FluffyUnderware.Curvy.Generator.Modules;
using FluffyUnderware.DevToolsEditor;

namespace FluffyUnderware.CurvyEditor.Generator.Modules
{
    [CustomEditor(typeof(BuildVolumeCaps))]
    public class BuildVolumeCapsEditor : CGModuleEditor<BuildVolumeCaps>
    {
        

        public override void OnModuleDebugGUI()
        {
            CGVMesh vmesh = Target.OutVMesh.GetData<CGVMesh>();
            if (vmesh)
            {
                EditorGUILayout.LabelField("Vertices: " + vmesh.Count);
                EditorGUILayout.LabelField("Triangles: " + vmesh.TriangleCount);
                EditorGUILayout.LabelField("SubMeshes: " + vmesh.SubMeshes.Length);
            }
        }
        
        
    }
   
}
