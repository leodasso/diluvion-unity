using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour {

    public static List<string> scenesToLoad = new List<string>();
    public delegate void DoneLoading();
    
    static DoneLoading doneLoadingDelegate;
    bool _setMainActive;
    TickLoadingBar graphic;
    List<AsyncOperation> loadingScenes = new List<AsyncOperation>();

    /// <summary>
    /// Fades out the screen, brings in the loading scene, and begins to load the scene(s) in the given list.
    /// </summary>
    /// <param name="scenes">scenes to load. The first one will be active</param>
    /// <param name="doneLoading">Function to call when loading is complete.</param>
    public static void Load(List<string> scenes, DoneLoading doneLoading)
    {
        Debug.Log("Begin loading");
        if (scenes.Count < 1) return;
        doneLoadingDelegate = doneLoading;
        scenesToLoad.Clear();
        scenesToLoad.AddRange(scenes);
        FadeOverlay.FadeInThenOut(1, Color.black, AddLoadScene);
    }

    public static void Load(string scene)
    {
        scenesToLoad.Clear();
        scenesToLoad.Add(scene);
        doneLoadingDelegate = null;
        FadeOverlay.FadeInThenOut(1, Color.black, AddLoadScene);
    }

    static void AddLoadScene()
    {
        SceneManager.LoadScene("loading", LoadSceneMode.Single);
    }

    static void RemoveLoadScene()
    {
        SceneManager.UnloadSceneAsync("loading");        
    }

    void Update()
    {
        if (loadingScenes.Count < 1) return;
        if (loadingScenes[0].isDone == false) return;

        Debug.Log("First scene loading operation complete!");

        if (!_setMainActive)
        {
            _setMainActive = true;
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(scenesToLoad[0]));
            Debug.Log("First scene has now been set active!");
        }
    }

    IEnumerator Start () 
    {
        graphic = GetComponentInChildren<TickLoadingBar>();

        if (scenesToLoad.Count < 1) yield break;
        
        // Allow time for other start operations before the loading begins
        yield return new WaitForSeconds(1);

        // Load each scene in the list
        for (int i = 0; i < scenesToLoad.Count; i++)
        {
            // Start the async loading of the scene
            Debug.Log("Creating a new async operation.");
            AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(scenesToLoad[i], LoadSceneMode.Additive);
            sceneLoad.allowSceneActivation = false;
            
            loadingScenes.Add(sceneLoad);

            // While the scene is still loading...
            while (sceneLoad.progress < .9f)
            {
                // Get & display the total loading progress
                float totalProgress = (i + sceneLoad.progress) / scenesToLoad.Count;
                graphic.SetLoading(totalProgress);

                // wait for the frame to complete before continuing the loop
                yield return null;
            }

            sceneLoad.allowSceneActivation = true;

            // Don't allow for the 'done loading' behavior to complete until we've guaranteed that the scenes are loaded and active.
            // otherwise, the ship could attempt to spawn in when there's no spawn points active, resulting in a broken load.
            while (!sceneLoad.isDone) yield return null;
            
            Debug.Log("Scene load operation for " + scenesToLoad[i]);

            // Set the first loaded scene as the active scene
            if (i == 0)
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(scenesToLoad[i]));
                Debug.Log("Set active scene to " + SceneManager.GetActiveScene().name);
            }
        }

        // Finished loading!
        Debug.Log("Completed loading, beginning fade transition to remove loading scene");
        FadeOverlay.FadeInThenOut(1, Color.black, RemoveLoadScene);
        
        

        yield return null;

        Debug.Log("Invoking 'done loading' delegate");
        doneLoadingDelegate?.Invoke();

        doneLoadingDelegate = null;
    }
}