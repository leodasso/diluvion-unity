using System;
using UnityEditor;
using UnityEngine;

namespace ParadoxNotion.Design
{
    public class ReadOnlyDrawer : AttributeDrawer<ReadOnlyAttribute>
    {
        public override object OnGUI(GUIContent content, object instance)
        {
            EditorGUILayout.LabelField(instance.ToString(), content.ToString());
            return instance;
        }    
    }
}