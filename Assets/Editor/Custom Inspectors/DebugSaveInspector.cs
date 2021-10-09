using UnityEngine;
using UnityEditor;
using System.Collections;
using Diluvion.SaveLoad;

[CustomEditor(typeof(DebugSave))]
public class DebugSaveInspector : Editor
{
    public Object saveFileOverwrite;
    DebugSave instance;

    void OnEnable()
    {
        instance = (DebugSave)target;

    }


    public override void OnInspectorGUI()
    {

        saveFileOverwrite = EditorGUILayout.ObjectField("Drop DSave Here", saveFileOverwrite, typeof(Object),false);
        //if (saveFileOverwrite != null)
            //if (GUILayout.Button("Import From DSave Format"))
                //instance.saveD = new DiluvionSaveData(DSave.DebugLoad(ObjectPath(saveFileOverwrite)));

        if (GUILayout.Button("Export To DSave Format"))
            instance.ExportToFile();

        DrawDefaultInspector();

        if (GUI.changed)
            EditorUtility.SetDirty(instance);
    }

    //Neat Filepath Getter for object
    public string ObjectPath(UnityEngine.Object saveFile)
    {
        Debug.Log("attempting to convert" + saveFile.name + "into a debugsave");   

        string objectFilePath = AssetDatabase.GetAssetPath(saveFile);

        Debug.Log("Debug save file converting from: " + objectFilePath);
        return objectFilePath;
    }
}
