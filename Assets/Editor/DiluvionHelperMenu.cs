using System.Collections.Concurrent;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using Diluvion;
using Diluvion.Ships;

/// <summary>
/// Adds little helpful menus to the editor.
/// </summary>
public class DiluvionHelperMenu : MonoBehaviour {

    #region game management
    
    [MenuItem("Diluvion/Begin Game &b", true)]
    static bool ValidateBeginGame()
    {
        return Application.isPlaying;
    }

    [MenuItem("Diluvion/Begin Game &b", priority = -599)]
    static void BeginGame()
    {
        GameManager.LoadSaveData();
        GameManager.BeginGame(GameManager.Get().saveData);
    }
    
    [MenuItem("Diluvion/Begin New Game %&b", true, priority = -599)]
    static bool ValidateBeginNewGame()
    {
        return Application.isPlaying;
    }
    
    [MenuItem("Diluvion/Begin New Game %&b", priority = -599)]
    static void BeginNewGame()
    {
        GameManager.LoadNewGame();
        //GameManager.LoadSaveData();
        GameManager.BeginGame(GameManager.Get().saveData);
    }
    
    [MenuItem("Game Manager", menuItem = "Diluvion/Game Manager &g", priority = -599)]
    static void SelectThis ()
    {
        Selection.activeObject = GameManager.Get();
    }
    
    #endregion

    #region runtime functions
    
    [MenuItem("Diluvion/Change boarding party... &1", true, priority = -99)]
    static bool ValidateBoardingParty()
    {
        return Application.isPlaying;
    }

    [MenuItem("Diluvion/Change boarding party... &1", priority = -99)]
    static void OpenBoardingParty()
    {
        PlayerManager.AskForBoardingParty();
    }
    
    
    [MenuItem("Add random sailor", menuItem = "Diluvion/Add random sailor &2", validate = true)]
    static bool ValidateAddRandomSailor ()
    {
        CrewManager playerCrew =  PlayerManager.PlayerCrew();
        return playerCrew != null;
    }

    [MenuItem("Add random sailor", menuItem = "Diluvion/Add random sailor &2", priority = -99)]
    static void AddRandomSailor ()
    {
        Sailor s = Instantiate(CharactersGlobal.SailorTemplate());
        s.Randomize(8);

        CrewManager playerCrew =  PlayerManager.PlayerCrew();
        playerCrew.AddCrewman(s);
    }
    

    [MenuItem("Add 10,000 Gold", menuItem = "Diluvion/Add 10,000 Gold &3", validate = true)]
    static bool ValAddGold ()
    {
        Inventory playerInv = PlayerManager.PlayerInventory();
        return playerInv != null;
    }
    
    [MenuItem("Add 10,000 Gold", menuItem = "Diluvion/Add 10,000 Gold &3", priority = -99)]
    static void AddGold()
    {
        Inventory playerInv = PlayerManager.PlayerInventory();
        playerInv.AddGold(10000);
    }
    
        
    [MenuItem("Diluvion/Select.../destroy outer space children", priority = -99)]
    static void SelectFarChildren ()
    {
        Vector3 origin = Selection.activeGameObject.transform.position;

        float maxDist = 1000;

        List<Transform> allChilds = new List<Transform>();

        // add all children to a list
        foreach (GameObject go in Selection.gameObjects)
            allChilds.AddRange(go.GetComponentsInChildren<Transform>());

        Debug.Log(allChilds.Count + " Children found....");
        int destroyed = 0;

        // Check all the children
        foreach (Transform t in allChilds)
        {
            // If they are outer space (i.e. really far away)
            if (Vector3.Distance(t.position, origin) > maxDist)
            {
                DestroyImmediate(t.gameObject);
                destroyed++;
            }
        }

        Debug.Log("Found and destroyed " + destroyed + " children waaay far out.");
    }

    [MenuItem("Diluvion/Select.../outer space children", validate = true, priority = -99)]
    static bool ValidateFarChildren()
    {
        if (Selection.gameObjects.Length < 1) return false;
        return true;
    }
    
    
    
    [MenuItem("Diluvion/Select.../Interior of selection &i", priority = -99)]
    static void SelectInterior()
    {
        InteriorManager im = Selection.activeGameObject.GetComponentInChildren<SideViewerStats>().Interior();

        Selection.activeGameObject = im.gameObject;
        SceneView.lastActiveSceneView.FrameSelected();

        SceneView.lastActiveSceneView.orthographic = true;
        SceneView.lastActiveSceneView.pivot = im.transform.position;
        SceneView.lastActiveSceneView.rotation = im.transform.rotation;
        SceneView.lastActiveSceneView.Repaint();
    }

    [MenuItem("Diluvion/Select.../Interior of selection &i", true, priority = -99)]
    static bool ValidateSelectInterior()
    {
        if (Selection.activeGameObject == null) return false;
        SideViewerStats sv = Selection.activeGameObject.GetComponentInChildren<SideViewerStats>();
        if (sv == null) return false;
        if (sv.Interior() == null) return false;
        return true;
    }
    

    /// <summary>
    /// Select the player ship
    /// </summary>
    [MenuItem("Diluvion/Select.../player ship &#s", priority = -99)]
    static void SelectPlayerShip()
    {
        Selection.activeGameObject = PlayerManager.PlayerShip() ;
        SceneView.lastActiveSceneView.FrameSelected();
    }
    
    [MenuItem("Diluvion/Select.../player ship &#s", true, priority = -99)]
    static bool ValidateSelectPlayerShip()
    {
        if (PlayerManager.PlayerShip() == null) return false;
        return true;
    }
    
    #endregion
    
    #region assets
    
    [MenuItem("Diluvion/reference lists.../Items &#i")]
    static void SelectItemsList()
    {
        Selection.activeObject = ItemsGlobal.Get();
    }

    [MenuItem("Diluvion/reference lists.../Quests &w")]
    static void SelectQuestsList()
    {
        Selection.activeObject = QuestsGlobal.Get();
    }

    [MenuItem("Diluvion/reference lists.../Chassis &h")]
    static void SelectChassisList()
    {
        Selection.activeObject = SubChassisGlobal.Get();
    }
    [MenuItem("Diluvion/reference lists.../Loadout &l")]
    static void SelectLoadoutList()
    {
        Selection.activeObject = LoadoutsGlobal.Get();
    }

    [MenuItem("Diluvion/reference lists.../Captain &c")]
    static void SelectCaptainsList()
    {
        Selection.activeObject = CaptainsGlobal.Get();
    }

    [MenuItem("Diluvion/reference lists.../Characters")]
    static void SelectCharList ()
    {
        Selection.activeObject = CharactersGlobal.Get();
    }

    [MenuItem("Diluvion/reference lists.../Inventories")]
    static void SelectInvList ()
    {
        Selection.activeObject = InvGlobal.Get();
    }

    [MenuItem("Diluvion/reference lists.../Ship modules")]
    static void SelectModList ()
    {
        Selection.activeObject = ModulesGlobal.Get();
    }

    [MenuItem("Diluvion/reference lists.../popups")]
    static void SelectPopupList ()
    {
        Selection.activeObject = PopupsGlobal.Get();
    }

    [MenuItem("Diluvion/reference lists.../sonar")]
    static void SelectSonarList ()
    {
        Selection.activeObject = SonarGlobal.Get();
    }

    [MenuItem("Diluvion/reference lists.../tags")]
    static void SelectTagList ()
    {
        Selection.activeObject = TagsGlobal.Get();
    }

    [MenuItem("Diluvion/reference lists.../Zones")]
    static void SelectZonesList ()
    {
        Selection.activeObject = ZonesGlobal.Get();
    }

    [MenuItem("Diluvion/reference lists.../reload all &r")]
    static void ReloadResourceLists()
    {
        List<Object> objects = ObjectsAtPath("Assets/Resources/global lists", SearchOption.TopDirectoryOnly, ".asset");
        foreach (Object o in objects)
        {
            GlobalList l = o as GlobalList;
            if (l) l.FindAll();
        }

    }

    [MenuItem("Diluvion/Localization/Terms library &l")]
    static void SelectLocLibrary()
    {
        Object i2Lang = AssetDatabase.LoadAssetAtPath("Assets/I2/Resources/I2Languages.prefab", typeof(Object));
        Selection.activeObject = i2Lang;
    }

    [MenuItem("Diluvion/Assets.../Queries #&q")]
    static void SelectQueries()
    {
        List<Object> queries = ObjectsAtPath("Assets/Prefabs/Queries", SearchOption.AllDirectories);
        if (queries.Count < 1) return;

        Selection.activeObject = queries[0];
    }

    [MenuItem("Diluvion/Assets.../Dialog &d")]
    static void SelectDialog()
    {
        List<Object> d = ObjectsAtPath("Assets/Prefabs/Dialogue", SearchOption.TopDirectoryOnly, ".prefab");
        if (d.Count < 1) return;

        Selection.activeObject = d[0];
    }

    [MenuItem("Diluvion/Assets.../Actions &a")]
    static void SelectActions()
    {
        List<Object> d = ObjectsAtPath("Assets/Prefabs/Actions", SearchOption.TopDirectoryOnly, ".asset");
        if (d.Count < 1) return;

        Selection.activeObject = d[0];
    }

    [MenuItem("Diluvion/Assets.../Quests &q")]
    static void SelectQuests()
    {
        List<Object> d = ObjectsAtPath("Assets/Prefabs/Quests", SearchOption.AllDirectories, ".asset");
        if (d.Count < 1) return;

        Selection.activeObject = d[0];
    }

    [MenuItem("Diluvion/Assets.../Sub Chassis")]
    static void SelectSubs ()
    {
        List<Object> d = ObjectsAtPath("Assets/Chassis", SearchOption.AllDirectories, ".asset");
        if (d.Count < 1) return;

        Selection.activeObject = d [0];
    }
    
    #endregion


    [MenuItem("GameObject/Create Other/Diluvion/interior room")]
    static void CreateInteriorRoom()
    {
        GameObject roomObj = new GameObject();
        roomObj.name = "new room";
        roomObj.transform.position = Vector3.zero;
        roomObj.layer = LayerMask.NameToLayer("Interior");


        roomObj.AddComponent<InteriorGrid>();
        roomObj.AddComponent<Room>();

        GameObject canvasObj = new GameObject();
        canvasObj.name = "new canvas";
        canvasObj.transform.parent = roomObj.transform;
        canvasObj.transform.localPosition = Vector3.zero;
        canvasObj.layer = LayerMask.NameToLayer("Interior");

        canvasObj.AddComponent<TileTool2DCanvas>();

        Selection.activeGameObject = roomObj;
        SceneView.lastActiveSceneView.FrameSelected();
    }


    /// <summary>
    /// Get all objects that are in the given directory
    /// </summary>
    public static List<Object> ObjectsAtPath(string path, SearchOption directoryOption = SearchOption.TopDirectoryOnly, string suffix = "")
    {
        List<Object> objList = new List<Object>();

        DirectoryInfo info = Directory.CreateDirectory(path);

        FileInfo[] fInfo = info.GetFiles("*" + suffix, directoryOption);
        foreach (FileInfo f in fInfo)
        {
            string nicePath = f.FullName;
            string nicerPath = nicePath.Substring(nicePath.LastIndexOf("Assets"));
            Object newObject = AssetDatabase.LoadAssetAtPath(nicerPath, typeof(Object)) as Object;
            objList.Add(newObject);
        }

        return objList;
    }
}
