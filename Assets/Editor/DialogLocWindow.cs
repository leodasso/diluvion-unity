using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;


public class DialogLocWindow : EditorWindow {

    public List<Dialogue> allDialogue = new List<Dialogue>();
    public List<Convo> allConversations = new List<Convo>();
    public string searchTerm = "";

    List<Convo> searchResults = new List<Convo>();
    Vector2 scrollPos;
    public static bool dialogFoldout = true;
    public static bool convoFoldout = true;


    [MenuItem("Diluvion/Window/Dialog Helper")]
    public static void ShowWindow()
    {
        DialogLocWindow newWindow = GetWindow(typeof(DialogLocWindow)) as DialogLocWindow;

        newWindow.allDialogue.Clear();

        DirectoryInfo path = new DirectoryInfo("Assets/Prefabs/Dialogue");
        List<FileInfo> fInfo = new List<FileInfo>();
        fInfo.AddRange(path.GetFiles("*.prefab", SearchOption.AllDirectories));
        fInfo.AddRange(path.GetFiles("*.asset", SearchOption.AllDirectories));

        foreach ( FileInfo f in fInfo )
        {
            string nicePath = f.FullName;
            string nicerPath = nicePath.Substring(nicePath.LastIndexOf("Assets"));

            Object newObject = AssetDatabase.LoadAssetAtPath(nicerPath, typeof(Object)) as Object;

            Convo c = newObject as Convo;
            if (c != null) newWindow.allConversations.Add(c);

            GameObject GO = newObject as GameObject;
            if (GO == null) continue;
            
            Dialogue d = GO.GetComponent<Dialogue>();
            if ( !d )               continue;
            if ( d.omitFromLoc )    continue;
            newWindow.allDialogue.Add(d);
        }
    }

    void OnGUI()
    {

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, true);

        if (!Application.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
        {

            EditorGUILayout.Space();
            GUILayout.Label("Dialogue Localizer", EditorStyles.boldLabel);
            GUILayout.Label("Found " + allDialogue.Count + " dialogues");

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Test All"))
            {
                foreach (Dialogue d in allDialogue) d.TestTerms();
            }

            if (GUILayout.Button("Re-create keys"))
            {
                foreach (Dialogue d in allDialogue) d.SetKeys();
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Pull all from source"))
            {
                foreach (Dialogue d in allDialogue) d.PullFromSource();
            }

            if (GUILayout.Button("Add all to source"))
            {
                foreach (Dialogue d in allDialogue) d.AddTerms();
            }

            EditorGUILayout.Space();

            dialogFoldout = EditorGUILayout.Foldout(dialogFoldout, "All Dialog");
            if (dialogFoldout)
            {
                foreach (Dialogue d in allDialogue)
                {
                    DialogInspector(d);
                }
            }
        }

        EditorGUILayout.Space();

        convoFoldout = EditorGUILayout.Foldout(convoFoldout, "All Convos");
        if (convoFoldout)
        {
            searchTerm = EditorGUILayout.TextField(searchTerm);

            if (string.IsNullOrEmpty(searchTerm)) searchResults.Clear();

            if (GUI.changed && !string.IsNullOrEmpty(searchTerm))
            {
                //Debug.Log("Searching " + allConversations.Count + " conversations.");
                searchResults.Clear();
                foreach (Convo c in allConversations)
                {
                    if (c == null) continue;
                    if (searchResults.Contains(c)) continue;
                    if (c.ToString().ToLower().Contains(searchTerm.ToLower())) searchResults.Add(c);
                }
            }

            foreach (Convo c in searchResults)
                EditorGUILayout.ObjectField(c, typeof(Convo), false);
        }



        EditorGUILayout.EndScrollView();
    }

    void DialogInspector(Dialogue d)
    {

        EditorGUILayout.BeginVertical();

        EditorGUILayout.Space();

        EditorGUILayout.ObjectField(d, typeof(Dialogue));

        EditorGUILayout.LabelField(d.locKey);

        EditorGUILayout.EndVertical();

        GUI.Box(GUILayoutUtility.GetLastRect(), "");
    }
}
