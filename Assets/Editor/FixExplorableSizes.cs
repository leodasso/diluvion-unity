using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using Loot;

public class FixExplorableSizes : EditorWindow
{


    [MenuItem("Tools/FixSelectedExplorables")]
    public static void ShowWindow()
    {
        FixExplorableSizes newWindow = GetWindow(typeof(FixExplorableSizes)) as FixExplorableSizes;

    }

    void OnGUI()
    {
        if (GUILayout.Button("Fix Selected"))
        {
           
            if (Selection.gameObjects == null) return;
            if (Selection.gameObjects.Length < 1) return;
           
        }
    }

    /* SpawnableSize ConvertSize(int oldSize)
     {
         switch (oldSize) 
         {
             case 0:
                 {
                     return SpawnableSize.Small;
                 }
             case 1:
                 {
                     return SpawnableSize.Medium;
                 }
             case 2:
                 {
                     return SpawnableSize.Large;
                 }
             case 3:
                 {
                     return SpawnableSize.Huge;
                 }
             default:
                 {
                     return SpawnableSize.Medium;
                 }
            }
        }*/
}