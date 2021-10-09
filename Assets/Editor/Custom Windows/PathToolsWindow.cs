using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class PathToolsWindow : EditorWindow
{

    [MenuItem("Diluvion/Window/Path editor &4")]
    public static void ShowWindow()
    {
        PathToolsWindow window = GetWindow(typeof(PathToolsWindow))as PathToolsWindow;
        window.Init();
    }

    int layerField;
    LayerMask castMask;
    string[] maskNames;

    GUIStyle ToggleButtonStyleNormal = null;
    GUIStyle ToggleButtonStyleToggled = null;
    //bool createAtClick = false;

    #region SafeGetters

    /// <summary>
    /// Builds a pre-formatted button rect
    /// </summary>
    /// <param name="xPosition">input whole number as a percentage of a button width</param>
    /// <param name="yPosition">input whole number as a percentage of button height</param>
    Rect SquareButton(float xPosition, float yPosition)
    {
        Vector2 size = new Vector2(50, 50);
        int yPos  = Mathf.FloorToInt(size.y* yPosition);
        int xPos = Mathf.FloorToInt(size.x * xPosition);
        Vector2 pos = new Vector2(xPos, yPos);
        return new Rect(pos, size);
    }

    Texture createIcon;
    Texture CreateIcon()
    {
        if (createIcon != null) return createIcon;
        createIcon = Resources.Load<Texture>("createWaypointIcon") as Texture;
        return createIcon;
    }


    Texture breakIcon;
    Texture BreakIcon()
    {
        if (breakIcon != null) return breakIcon;
        breakIcon = Resources.Load<Texture>("breakWaypoint") as Texture;
        return breakIcon;
    }

    Texture connectIcon;
    Texture ConnectIcon()
    {
        if (connectIcon != null) return connectIcon;
        connectIcon = Resources.Load<Texture>("connectWaypoints") as Texture;
        return connectIcon;
    }

    Texture terrainInactiveIcon;
    Texture TerrainInactive()
    {
        if (terrainInactiveIcon != null) return terrainInactiveIcon;
        terrainInactiveIcon = Resources.Load<Texture>("terrainCreateInactive") as Texture;
        return terrainInactiveIcon;
    }

    Texture terrainActiveIcon;
    Texture TerrainActive()
    {
        if (terrainActiveIcon != null) return terrainActiveIcon;
        terrainActiveIcon = Resources.Load<Texture>("terrainCreateActive") as Texture;
        return terrainActiveIcon;
    }

    #endregion

    float distanceFromCam = 2;
    bool clickedTerrainCreate;
    bool clickedInBetweenCreate;
    bool storedDelegate;
    Object parentTrans;
  

    public void Init()
    {
        layerField = LayerMask.NameToLayer("Terrain");
    }

    private void OnGUI()
    {
        if(!storedDelegate)
        {
            SceneView.onSceneGUIDelegate += OnScene;
            storedDelegate = true;
        }
        if (ToggleButtonStyleNormal == null)
        {
            ToggleButtonStyleNormal = "Button";
            ToggleButtonStyleToggled = new GUIStyle(ToggleButtonStyleNormal);
            ToggleButtonStyleToggled.normal.background = ToggleButtonStyleToggled.active.background;
        }


        layerField = EditorGUILayout.LayerField(layerField);
        castMask = 1 << layerField;
        parentTrans = EditorGUILayout.ObjectField("Spawn Parent:", parentTrans, typeof( Transform ),true);
        GUILayout.Label("Distance:");
        distanceFromCam = EditorGUILayout.FloatField(distanceFromCam);
        distanceFromCam = GUILayout.HorizontalSlider(distanceFromCam, 1, 150);


        EditorGUILayout.BeginVertical();
        if(GUI.Button(SquareButton(0,2f), CreateIcon()))
            CreateInFrontOfCamera();

        EditorGUILayout.BeginHorizontal();
        if (GUI.Button(SquareButton(0, 3f), ConnectIcon()))        
            ConnectWaypoints();
        if (GUI.Button(SquareButton(1, 3f), BreakIcon()))
            SplitWaypoints();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        clickedTerrainCreate = ToggleButton1(0,4f);
        if (clickedTerrainCreate&& clickedInBetweenCreate)
            clickedInBetweenCreate = false;
        
        clickedInBetweenCreate= ToggleButton2(1,4f);
        if (clickedInBetweenCreate&& clickedTerrainCreate)
            clickedTerrainCreate = false;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }



    /// <summary>
    /// Terrain detection and placement function
    /// </summary>
    /// <param name="view"></param>
    void TerrainPlacement(SceneView view)
    {
        if (!clickedTerrainCreate) return;
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, 3000, castMask)) return;

        //Debug.DrawRay(hit.point, hit.normal * distanceFromCam, Color.yellow);
        DrawWPPreview(hit);
        view.Repaint();

        if (!MouseDownEditor(1)) return;
        Vector3 pointNormal = hit.point + hit.normal * distanceFromCam;
        CreateWaypoint(pointNormal);
    }


    void CenterTerrainPlacement(SceneView view)
    {
        if (!clickedInBetweenCreate) return;
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, 3000, castMask)) return;

        Ray upRay = new Ray(hit.point, hit.normal);
        RaycastHit upHit;
        Vector3 centerPos = hit.point + hit.normal * distanceFromCam;
        if (Physics.Raycast(upRay, out upHit, distanceFromCam, castMask))
        {
            centerPos= hit.point + hit.normal * upHit.distance / 2;

            //TODO YAGNI MULTIPLE RAYS FOR CENTER Vector3 cameraRight = Vector3.Cross(ray.direction, Vector3.up);
        }
        
        DrawInBetweenPreview(upRay, centerPos, upHit);
        view.Repaint();

        if (!MouseDownEditor(1)) return;
        CreateWaypoint(centerPos);
        
    }

    /// <summary>
    /// Main Scene Update Loop
    /// </summary>
    /// <param name="sceneView"></param>
    void OnScene(SceneView sceneView) //TODO Get Update for scene so you can click, ONGUI only updates when clicking on window
    {
        TerrainPlacement(sceneView);
        CenterTerrainPlacement(sceneView);
        
        if (AltHotkey(KeyCode.X))
            ConnectSplit();
      
    }

    #region selection helpers
    
    void DrawInBetweenPreview(Ray ray, Vector3 centerPoint, RaycastHit rh)
    {
        Handles.color = Color.white;
        if (Event.current.control)
            Handles.color = Color.green;

        Vector3 dottedPoint2 = rh.point;
        if (dottedPoint2 == Vector3.zero)
            dottedPoint2 = centerPoint;
        Handles.DrawDottedLine(ray.origin,dottedPoint2, 0.2f);

        Handles.color = Color.cyan;
        Handles.SphereCap(0,centerPoint, Quaternion.identity, 2f);
    }

    
    
    
    /// <summary>
    /// Draws the Waypoint preview where the mouse is
    /// </summary>
    /// <param name="hit"></param>
    void DrawWPPreview(RaycastHit hit)
    {
        Handles.color = Color.white;
        if (Event.current.control)
            Handles.color = Color.green;
        Handles.DrawDottedLine(hit.point, hit.point+ hit.normal * distanceFromCam, 0.2f);

        Handles.color = Color.cyan;
        Handles.SphereCap(0, hit.point + hit.normal * distanceFromCam, Quaternion.identity, 2f);
    }

    static bool _setValue1=true;
    static bool _smoothValue1;
    /// <summary>
    /// Drawws a toggle button
    /// </summary>
     bool ToggleButton1(float xPos, float yPos)
    {
        if (_smoothValue1)
        {
            if (GUI.Button(SquareButton(xPos, yPos), TerrainActive(), _smoothValue1 ? ToggleButtonStyleToggled : ToggleButtonStyleNormal))
            {
              
                _setValue1 = true;
                _smoothValue1 = false;
            }
            GUI.Label(new Rect(xPos*50, (yPos+1)*50, 100,25), "Ctrl+RightClick");
        }

        if(_setValue1)
            if (GUI.Button(SquareButton(xPos, yPos), TerrainInactive(), _smoothValue1 ? ToggleButtonStyleToggled : ToggleButtonStyleNormal))
            {
                _smoothValue1 = true;
                _setValue1 = false;
            }
        return _smoothValue1;
    }
    
    
    static bool _setValue2=true;
    static bool _smoothValue2;
    /// <summary>
    /// Drawws a toggle button
    /// </summary>
    bool ToggleButton2(float xPos, float yPos)
    {
        if (_smoothValue2)
        {
            if (GUI.Button(SquareButton(xPos, yPos), TerrainActive(), _smoothValue2 ? ToggleButtonStyleToggled : ToggleButtonStyleNormal))
            {
              
                _setValue2 = true;
                _smoothValue2 = false;
            }
            GUI.Label(new Rect(xPos*50, (yPos+1)*50, 200,25), "Ctrl+RightClick");
        }

        if(_setValue2)
            if (GUI.Button(SquareButton(xPos, yPos), TerrainInactive(), _smoothValue2 ? ToggleButtonStyleToggled : ToggleButtonStyleNormal))
            {
                _smoothValue2 = true;
                _setValue2 = false;
            }
        return _smoothValue2;
    }
    

    

/// <summary>
/// Creates a waypoint in front of the camera at Distance
/// </summary>
    void CreateInFrontOfCamera()
    {
        Vector3 newPosition = SceneView.lastActiveSceneView.camera.transform.position;
        Quaternion newRotation = SceneView.lastActiveSceneView.rotation;
        Vector3 offsetPos = newRotation * Vector3.forward * distanceFromCam*2;
        CreateWaypoint(newPosition+offsetPos);
    }

    /// <summary>
    /// Creates a waypoint at position
    /// </summary>
    /// <param name="position"></param>
    void CreateWaypoint(Vector3 position)
    {       
        PathMono newPM = PathMono.CreateNode(position);
        if (parentTrans != null)
            newPM.transform.SetParent((Transform)parentTrans);
        Undo.RegisterCreatedObjectUndo(newPM.gameObject, "Created Waypoint");
        ConnectSelected(newPM);
        SelectLast(newPM.gameObject);
    }

    /// <summary>
    /// Selects the input object
    /// </summary>
    /// <param name="go"></param>
    void SelectLast(Object go)
    {
        List<Object> gos = new List<Object>();
        gos.Add(go);
        Selection.objects = gos.ToArray();
    }

    /// <summary>
    /// Adds the object to the Selection
    /// </summary>
    /// <param name="go"></param>
    void AddToSelected(Object go)
    {
        List<Object> gos = new List<Object>(Selection.objects);      
        gos.Add(go);
        Selection.objects = gos.ToArray();
    }

    /// <summary>
    /// HotKey event, only happens when window is active
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    bool AltHotkey(KeyCode code)
    {
        if (!Event.current.alt) return false;
        if (Event.current.type != EventType.KeyDown) return false;
        if (Event.current.keyCode != code) return false;
        return true;
    }

    /// <summary>
    /// MouseDown event
    /// </summary>
    /// <param name="button"></param>
    /// <returns></returns>
    bool MouseDownEditor(int button)
    {
        if (!Event.current.control) return false;
        if (Event.current.type == EventType.MouseDown && Event.current.button == button)
            return true;
        return false;
    }

    #endregion

    #region Connection manipulation

    /// <summary>
    /// Connects Selected Waypoints to all other Selected waypoints
    /// </summary>
    void ConnectWaypoints()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            PathMono pm = go.GetComponent<PathMono>();
            if (pm == null) continue;
            ConnectSelected(pm);
        }
    }

    /// <summary>
    /// Connects input waypoint to all selected waypoints
    /// </summary>
    /// <param name="connectTo"></param>
    void ConnectSelected(PathMono connectTo)
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            PathMono pm = go.GetComponent<PathMono>();
            if (pm == null) continue;
            connectTo.Connect(pm);
        }
    }

    /// <summary>
    /// Disconnects all selected waypoints from all other selected waypoints
    /// </summary>
    void SplitWaypoints()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            PathMono pm = go.GetComponent<PathMono>();
            if (pm == null) continue;
            SplitConnected(pm);
        }
    }

    /// <summary>
    /// Disconnects input waypoint from all selected waypoints
    /// </summary>
    /// <param name="connectTo"></param>
    void SplitConnected(PathMono connectTo)
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            PathMono pm = go.GetComponent<PathMono>();
            if (pm == null) continue;
            connectTo.RemoveConnection(pm);
        }
    }


    List<PathMono> connectedThisTime = new List<PathMono>();
    /// <summary>
    /// Flips all selected connections to the opposite
    /// </summary>
    void ConnectSplit()
    {
        connectedThisTime = new List<PathMono>();
        foreach (GameObject go in Selection.gameObjects)
        {
            PathMono pm = go.GetComponent<PathMono>();
            if (pm == null) continue;
            if (connectedThisTime.Contains(pm)) continue;
            ConnectSplitSelected(pm);
        }
    }

    /// <summary>
    /// Flips the connection between the input waypoint and all selectd waypoints
    /// </summary>
    /// <param name="p"></param>
    void ConnectSplitSelected(PathMono p)
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            PathMono pm = go.GetComponent<PathMono>();            
            if (pm == null) continue;
            connectedThisTime.Add(pm);
            connectedThisTime.Add(p);
            if (p.IsConnected(pm))
                p.Disconnect(pm);
            else
            {
                p.Connect(pm);
                
            }
        }
    }
    #endregion
}