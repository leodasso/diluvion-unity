using UnityEngine;
using UnityEditor;


public class KeyboardMenuItems : MonoBehaviour
{
    [MenuItem("GameObject/UI/Heathen/Keyboards/QWERTY")]
    static void CreateQWERTY(MenuCommand menuCommand)
    {
        GameObject prefabBox = AssetDatabase.LoadAssetAtPath("Assets/_Heathen Engineering/UIX/Prefabs/Keyboards/Basic Keyboard (QWERTY).prefab", typeof(GameObject)) as GameObject;

        GameObject newBox = Instantiate(prefabBox);
        newBox.name = "QWERTY Keyboard";
        GameObjectUtility.SetParentAndAlign(newBox, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(newBox, "Create " + newBox.name);
        Selection.activeObject = newBox;
    }

    [MenuItem("GameObject/UI/Heathen/Keyboards/AZERTY")]
    static void CreateAZERTY(MenuCommand menuCommand)
    {
        GameObject prefabBox = AssetDatabase.LoadAssetAtPath("Assets/_Heathen Engineering/UIX/Prefabs/Keyboards/Basic Keyboard (AZERTY).prefab", typeof(GameObject)) as GameObject;

        GameObject newBox = Instantiate(prefabBox);
        newBox.name = "AZERTY Keyboard";
        GameObjectUtility.SetParentAndAlign(newBox, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(newBox, "Create " + newBox.name);
        Selection.activeObject = newBox;
    }

    [MenuItem("GameObject/UI/Heathen/Keyboards/Korean")]
    static void CreateKorean(MenuCommand menuCommand)
    {
        GameObject prefabBox = AssetDatabase.LoadAssetAtPath("Assets/_Heathen Engineering/UIX/Prefabs/Keyboards/Korean Keyboard (QWERTY).prefab", typeof(GameObject)) as GameObject;

        GameObject newBox = Instantiate(prefabBox);
        newBox.name = "Korean Keyboard";
        GameObjectUtility.SetParentAndAlign(newBox, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(newBox, "Create " + newBox.name);
        Selection.activeObject = newBox;
    }
}

