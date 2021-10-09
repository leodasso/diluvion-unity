//
// KinoFog - Deferred fog effect
//
// Copyright (C) 2015 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(Fog))]
public class FogEditor : Editor
{
    SerializedProperty _gradient;
    SerializedProperty _fogColor;
    SerializedProperty _startDistance;
    SerializedProperty _offsetDistance;
    SerializedProperty _endDistance;
    SerializedProperty _outlineDistance;
    SerializedProperty _useRadialDistance;
    SerializedProperty _fadeToSkybox;
    SerializedProperty _fogMaterial;
    Fog fog;
     
    void OnEnable()
    {

        //_gradient = serializedObject.FindProperty("_gradient");       
        _fogColor = serializedObject.FindProperty("_fogColor");
        _startDistance = serializedObject.FindProperty("_startDistance");
        _offsetDistance = serializedObject.FindProperty("_offsetDistance");
        _endDistance = serializedObject.FindProperty("_endDistance");
        _outlineDistance = serializedObject.FindProperty("_outlineDistance");
        _useRadialDistance = serializedObject.FindProperty("_useRadialDistance");
        _fadeToSkybox = serializedObject.FindProperty("_fadeToSkybox");
        _fogMaterial = serializedObject.FindProperty("_material");

        fog = (Fog)target;
    
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        //EditorGUILayout.PropertyField(_gradient);
        EditorGUILayout.PropertyField(_fogColor);
        EditorGUILayout.PropertyField(_startDistance);
        EditorGUILayout.PropertyField(_offsetDistance);
        EditorGUILayout.PropertyField(_endDistance);
        EditorGUILayout.PropertyField(_outlineDistance);
        EditorGUILayout.PropertyField(_useRadialDistance);
        EditorGUILayout.PropertyField(_fadeToSkybox);
        EditorGUILayout.PropertyField(_fogMaterial);
        
        //if(GUI.changed)
        //    fog.BlitIt();
        serializedObject.ApplyModifiedProperties();
    }



}

