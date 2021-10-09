using UnityEngine;
using HeavyDutyInspector;
using System.IO;
using System.Collections.Generic;
using Quests;

public class QuestExporter : MonoBehaviour {

    public string path = "Asset/Quests";

    [Button("Export quests", "ExportChildren", true)]
    public bool hidden1;

	/*
    void ExportChildren()
    {
        List<Quest> childQuests = new List<Quest>();
        childQuests.AddRange(GetComponentsInChildren<Quest>());

        foreach (Quest q in childQuests)
        {
            ExportQuest(q);
        }
    }

    void ExportQuest(Quest q)
    {
        string newPath = path + q.name;

        // Have we created a folder yet?
        bool createdFolder = AssetDatabase.IsValidFolder(newPath);

        // If not, create one.
        if (!createdFolder)
        {
            string guid = AssetDatabase.CreateFolder(path, q.name);
            Debug.Log("Created folder " + AssetDatabase.GUIDToAssetPath(guid));
        }

        Quests.DQuest newQuest = ScriptableObject.CreateInstance(typeof(DQuest)) as DQuest;

        newQuest.name = q.name;
        newQuest.locKey = q.locKey;
        newQuest.hidden = q.hidden;
        newQuest.stillShowWaypoint = q.stillShowWaypoint;
        newQuest.autoStart = q.autoStart;
        newQuest.title = q.title;
        newQuest.description = q.description;

        AssetDatabase.CreateAsset(newQuest, path + newQuest.name + ".asset");
    }
    */
}
