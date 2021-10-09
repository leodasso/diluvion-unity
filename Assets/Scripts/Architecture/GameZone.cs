using System.Collections.Generic;
using Diluvion;
using Diluvion.SaveLoad;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
#endif


/// <summary>
/// Contains all the info needed for a particular open world zone of Diluvion.
/// </summary>
[CreateAssetMenu( fileName = "game zone", menuName = "Diluvion/Game/zone object")]
public class GameZone : ScriptableObject
{
    public float shallowestDepth = -200;

    [Tooltip("The scenes that belong to this zone. Order determines load order.")]
    public List<string> scenes = new List<string>();
    
    [Tooltip("Scenes containing cosmetics, props, and stuff that has no effect on gameplay. They will only load in builds, unless otherwise specified.")]
    public List<string> buildOnlyScenes = new List<string>();

    public LocTerm zoneName;

    [Tooltip("The track that plays when in an explorable in this zone.")]
    public AdventureDifficulty neutralExplorableTrack;

    [DrawWithUnity]
    public UnityEvent onZoneDiscovered;

    [Tooltip("This is for converting old save files")]
    public Zones zone;

    public GameObject mapPrefab;
    //[Space]
    //public MapObject map;

    /// <summary>
    /// Returns scenes to load based on if it's in build or not.
    /// </summary>
    public List<string> ScenesToLoad(bool forceLoadAll = false)
    {
        List<string> loadScenes = new List<string>();
        loadScenes.AddRange(scenes);

        bool loadCosmetics = true;
        #if UNITY_EDITOR
        loadCosmetics = false;
        #endif
        
        if (loadCosmetics || forceLoadAll) 
            loadScenes.AddRange(buildOnlyScenes);

        return loadScenes;
    }


#if UNITY_EDITOR
    [ButtonGroup]
    public void LoadToEditor()
    {
        if (Application.isPlaying) return;

        // Load all the scenes into editor.
        for (int i = 0; i < scenes.Count; i++)
        {
            // The first one should remove previous scenes
            if (i == 0) LoadSceneIntoEditor(scenes [i], true);

            else LoadSceneIntoEditor(scenes [i]);
        }
    }
    
    [ButtonGroup]
    public void LoadAllToEditor()
    {
        if (Application.isPlaying) return;
        
        LoadToEditor();

        // Load all the scenes into editor.
        foreach (string t in buildOnlyScenes)
            LoadSceneIntoEditor(t);
    }


    public static void LoadSceneIntoEditor (string sceneName, bool removePrevious = false)
    {
        Debug.Log("Getting scene by name " + sceneName);

        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            string path = EditorBuildSettings.scenes [i].path;
            
            Debug.Log("Path to scene: " + path);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("Skipping index " + i + " because no path exists in the build settings.");
                continue;
            }
            
            string name = path.Substring(0, path.Length - 6).Substring(path.LastIndexOf('/') + 1);

            if (name == sceneName)
            {
                if (removePrevious)
                {
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                }
                else
                {
                    EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                }
            }
        }
    }

#endif

    /// <summary>
    /// Loads the scenes for this zone in. Also adds it to the save file, if there is one.
    /// </summary>
    [ButtonGroup]
    public void Load()
    {
        if (!Application.isPlaying) return;

        Debug.Log("Loading zone " + name);
        DSave.AddNewZone(this);
        GameManager.Reload(true);
    }
}