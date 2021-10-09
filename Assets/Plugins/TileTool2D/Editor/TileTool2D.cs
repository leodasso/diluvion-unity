/****************************************
TileTool2D
Copyright 2016 Unluck Software	
www.chemicalbliss.com
*****************************************/

using UnityEngine;
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5
using UnityEditor.SceneManagement;
#endif
using System;
using System.Collections.Generic;
using UnityEditor;
using System.Collections;
using System.IO;
using UnityEditorInternal;
using System.Reflection;


[Serializable]
public class TileTool2D : EditorWindow {
	//Window
	public static EditorWindow win;

	//GUI Textures
	[SerializeField]
	public Texture select;
	public static Texture brush;
	public static Texture brushActive;
	public static Texture eraser;
	public static Texture eraserActive;
	public static Texture magicWand;
	public static Texture magicWandActive;
	public static Texture snapT;
	public static Texture snapTActive;
	public static Texture selectT;
	public static Texture selectTActive;
	public static Texture box;
	public static Texture boxActive;
	public static Texture boxErase;
	public static Texture boxEraseActive;

	//Tools
	public string mode = "Draw";
	public string tool = "Draw";

	//Drawing

	public GameObject drawField;
	public Vector2 currentDrawPos;
	public GameObject drawTile;
	public static bool mouseDown;

	//Layers
	public static GameObject canvas;
	public static Vector2 layerScroll;

	public int currentLayer;

	public bool onlyVisible = false;

	//Preview

	public GameObject[] tiles;

	public Vector2 scrollPos;

	public int spriteSize = 64;

	//Snapping
	public Vector3 prevPosition;
	public bool doSnap = false;
	public float snapValue = 1.0f;
	public Transform[] _selection;

	//GUI Style
	public static GUIStyle invisibleButton;
	public int invisibleButtonMargin = 2;
	public GUIStyle smallButton;
	static public bool alt;
	public float adjust = 0.32f;
	public string[] sets;
	public string currentSet;
	public int _index = 0;
	public int layerCount;

	//Unity sorting layers
	[SerializeField]
	public string[] sortingLayers;

	private bool warningSort;

	[SerializeField]
	public TileTool2DCanvas[] CanvasContainer;
	[SerializeField]
	public int canvaseIndex = 0;
	[SerializeField]
	public string[] canvasSets;

	Vector2 startPos = new Vector2();
	bool dragging = false;

	Tile2D newTile2D;

	[SerializeField]
	public bool enableUndo = false;

	[SerializeField]
	public bool enablePhysics = false;

	//Handle Colors
	public Color32 handlesColorRed = new Color32((byte)255, (byte)0, (byte)0, (byte)100);
	public Color32 handlesColorWhite =  new Color32((byte)155, (byte)155, (byte)255, (byte)100);
	public Color32 handlesColorGreen = new Color32((byte)0, (byte)255, (byte)0, (byte)100);

	[MenuItem("Window/TileTool2D/TileTool2D")]
	public static void ShowWindow() {
		win = EditorWindow.GetWindow(typeof(TileTool2D));
		win.titleContent = new GUIContent(" TileTool2D", Resources.Load<Texture>("GUITextures/TileTool2DIcon"));
		win.minSize = new Vector2(435.0f, 400.0f);
	}

	public void OnEnable() {
		CreateDrawField();
		//	FindCanvases();
		//	CreateLayerHolder(false);
		FindCanvases();
		ChangeCanvas();
		if (tool != "Select") EnableSceneGUI(true);
		sortingLayers = GetSortingLayerNames();
		LoadGUITextures();
		LoadSet();
		if (drawTile == null && tiles.Length > 0) drawTile = tiles[0].gameObject;
	}

	public void LoadGUITextures() {
		if (select == null) select = Resources.Load<Texture>("GUITextures/TT2DSelected");
		if (brush == null) brush = Resources.Load<Texture>("GUITextures/TT2DBrush");
		if (brushActive == null) brushActive = Resources.Load<Texture>("GUITextures/TT2DBrushActive");
		if (eraser == null) eraser = Resources.Load<Texture>("GUITextures/TT2DEraser");
		if (eraserActive == null) eraserActive = Resources.Load<Texture>("GUITextures/TT2DEraserActive");
		if (magicWand == null) magicWand = Resources.Load<Texture>("GUITextures/TT2DMagicWand");
		if (magicWandActive == null) magicWandActive = Resources.Load<Texture>("GUITextures/TT2DMagicWandActive");
		if (snapT == null) snapT = Resources.Load<Texture>("GUITextures/TT2Dsnap");
		if (snapTActive == null) snapTActive = Resources.Load<Texture>("GUITextures/TT2DsnapActive");
		if (selectT == null) selectT = Resources.Load<Texture>("GUITextures/TT2Dselect");
		if (selectTActive == null) selectTActive = Resources.Load<Texture>("GUITextures/TT2DselectActive");
		if (box == null) box = Resources.Load<Texture>("GUITextures/TT2DBox");
		if (boxActive == null) boxActive = Resources.Load<Texture>("GUITextures/TT2DBoxActive");
		if (boxErase == null) boxErase = Resources.Load<Texture>("GUITextures/TT2DBoxErase");
		if (boxEraseActive == null) boxEraseActive = Resources.Load<Texture>("GUITextures/TT2DBoxEraseActive");
	}

	public void OnDisable() {
		RemoveDrawField();
		EnableSceneGUI(false);
	}

	public void EnableSceneGUI(bool enabled) {
		SceneView.onSceneGUIDelegate -= OnSceneGUI;
		if (enabled) SceneView.onSceneGUIDelegate += OnSceneGUI;
	}

	// Adjusts scroll windows based on how many layers there are
	// Saves amount of layers then checks if that number has changed >> Sorts all sprite sorting orders if amount of layers has changed
	public void OnFocus() {
		FindCanvases();
		ChangeCanvas();
		adjust = AdjustLayerScrollSize();
		if (!canvas) return;
		if (layerCount == 0) {
			layerCount = canvas.transform.childCount;
			return;
		}
		if (layerCount != canvas.transform.childCount) {
			SortAll(false);
			layerCount = canvas.transform.childCount;
		}
	}

	// Returns string based on draw mode 
	public string ModeText() {
		string snapText = "";
		if (tool == "Select" && doSnap) snapText = " + Snap";
		if (tool == "Draw") return ("Draw");
		if (tool == "Erase") return ("Erase");
		if (tool == "Fix") return ("Fix");
		if (tool == "BoxDraw") return ("BoxDraw ");
		if (tool == "BoxErase") return ("BoxErase");
		return ("Select" + snapText);
	}

    bool EditableCanvas()
    {
        if (!canvas) return false;
        TileTool2DCanvas canvasComponent = canvas.GetComponent<TileTool2DCanvas>();
        if (!canvasComponent) return false;
        return canvasComponent.editable;
    }


    bool flipX;

	// Handles everything that happens in scene view, updates as long as window is open
	public void OnSceneGUI(SceneView sceneview)
    {
        if (!canvas)
        {
            ChangeCanvas();
            return;
        }

        if (!EditableCanvas())
        {
            ChangeCanvas();
            return;
        }

		if (!drawTile) return;
		Tile2D tile2D = drawTile.GetComponent<Tile2D>();
		if (!tile2D) {
			Debug.LogError(drawTile + "is not a TileTool2D object - Please attach Tile2D component to the object.");
			drawTile = null;
			return;
		}


        Event e = Event.current;

        // Toggle flip x axis
        switch (e.type)
        {
            case EventType.KeyDown:
            {
                if (Event.current.keyCode == KeyCode.X)
                    flipX = !flipX;
                break;
            }
        }
        if (flipX) Handles.Label(HandleUtility.GUIPointToWorldRay(e.mousePosition).origin, "      Flip X");


        if (Event.current.type == EventType.Layout)
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));


		if (e.shift && e.control) alt = true; else alt = false;

		// Draw shortcuts
		if (tool == "Draw") {
			if (!alt && e.shift) mode = "Erase";
			else if (!alt && e.control) mode = "Fix";
			else mode = "Draw";
		} else if (tool == "BoxDraw") {
			if (!alt && e.shift) mode = "BoxErase"; else mode = "BoxDraw";
		}

		if (mode == "BoxErase") {
			Tile2D t = (Tile2D)drawTile.GetComponent(typeof(Tile2D));
			Vector2 endPos = (Vector2)HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
			int width = (int)(Mathf.Abs((startPos - endPos).x) /t.tileSize);
			int height = (int)(Mathf.Abs((startPos - endPos).y) /t.tileSize);
			width = Mathf.Clamp(width, 0, 48);
			height = Mathf.Clamp(height, 0, 48 - width);

			if (!dragging) {
				SceneView.RepaintAll();
				Vector2 pos = (Vector2)HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
				pos.x = Round(pos.x, t.tileSize) + t.tileSize * 0.5f;
				pos.y = Round(pos.y, t.tileSize) + t.tileSize * 0.5f;
				Handles.color = handlesColorRed;
				Handles.CubeCap(1, pos, Quaternion.identity, t.tileSize);
				startPos = (Vector2)HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
			}

			if (e.button == 0 && e.type == EventType.MouseDrag) {
				dragging = true;
				SceneView.RepaintAll();
			}

			int dy = -1;
			int dx = -1;
			if (startPos.y < endPos.y) dy = 1;
			if (startPos.x < endPos.x) dx = 1;
			if (e.button == 0 && e.type == EventType.MouseUp) {
				if (!dragging) return;
				dragging = false;
				for (int i = 0; i < width + 1; i++) {
					for (int n = 0; n < height + 1; n++) {
						Vector3 pos = new Vector3(Round(startPos.x, t.tileSize) + t.tileSize * 0.5f + (i * t.tileSize *dx), Round(startPos.y, t.tileSize) + t.tileSize * 0.5f + (n * t.tileSize *dy), 0.0f);
						Erase(pos);
					}
				}
                MarkSceneDirty();
            }

			if (dragging) {
				if (e.keyCode == KeyCode.Escape || e.type == EventType.MouseDown) {
					dragging = false;
					return;
				}
				for (int i = 0; i < width + 1; i++) {
					Handles.color = handlesColorRed;
					for (int n = 0; n < height + 1; n++) {
						Vector3 pos = new Vector3(Round(startPos.x, t.tileSize) + t.tileSize * 0.5f + (i * t.tileSize *dx), Round(startPos.y, t.tileSize) + t.tileSize * 0.5f + (n * t.tileSize *dy), 0.0f);
						Handles.CubeCap(1, pos, Quaternion.identity, t.tileSize);
					}
				}
				Handles.Label(startPos, " " + (int)(width + 1.5) + " x " + (int)(height + 1.5));
			}
			return;
		}

		if (tool == "BoxDraw") {
			Tile2D t = (Tile2D)drawTile.GetComponent(typeof(Tile2D));
			Vector2 endPos = (Vector2)HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
			int width = (int)(Mathf.Abs((startPos - endPos).x) /t.tileSize);
			int height = (int)(Mathf.Abs((startPos - endPos).y) /t.tileSize);
			width = Mathf.Clamp(width, 0, 48);
			height = Mathf.Clamp(height, 0, 48 - width);

			if (!dragging) {
				SceneView.RepaintAll();
				Vector2 pos = (Vector2)HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
				pos.x = Round(pos.x, t.tileSize) + t.tileSize * 0.5f;
				pos.y = Round(pos.y, t.tileSize) + t.tileSize * 0.5f;
				Handles.color = handlesColorWhite;
				Handles.CubeCap(1, pos, Quaternion.identity, t.tileSize);
				startPos = (Vector2)HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
			}

			if (e.button == 0 && e.type == EventType.MouseDrag) {
				dragging = true;
				SceneView.RepaintAll();
			}

			int dy = -1;
			int dx = -1;
			if (startPos.y < endPos.y) dy = 1;
			if (startPos.x < endPos.x) dx = 1;
			if (e.button == 0 && e.type == EventType.MouseUp) {
				if (!dragging) return;
				dragging = false;
				for (int i = 0; i < width + 1; i++) {
					for (int n = 0; n < height + 1; n++) {
						Vector3 pos = new Vector3(Round(startPos.x, t.tileSize) + t.tileSize * 0.5f + (i * t.tileSize *dx), Round(startPos.y, t.tileSize) + t.tileSize * 0.5f + (n * t.tileSize *dy), 0.0f);
						Draw(pos);
					}
				}
                MarkSceneDirty();
            }

			if (dragging) {
				if (e.keyCode == KeyCode.Escape || e.type == EventType.MouseDown) {
					dragging = false;
					return;
				}
				for (int i = 0; i < width + 1; i++) {
					Handles.color = handlesColorWhite;
					for (int n = 0; n < height + 1; n++) {
						Vector3 pos = new Vector3(Round(startPos.x, t.tileSize) + t.tileSize * 0.5f + (i * t.tileSize *dx), Round(startPos.y, t.tileSize) + t.tileSize * 0.5f + (n * t.tileSize *dy), 0.0f);
						Handles.CubeCap(1, pos, Quaternion.identity, t.tileSize);
					}
				}
				Handles.Label(startPos, " " + (int)(width + 1.5) + " x " + (int)(height + 1.5));
			}
			return;
		}

		if (tool == "Draw" || tool == "Erase" || tool == "Fix") {
			SceneView.RepaintAll();
			Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
			RaycastHit hit = new RaycastHit();
			Tile2D t = (Tile2D)drawTile.GetComponent(typeof(Tile2D));
			if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity)) {
				if (e.button == 0 && (e.type == EventType.MouseDrag || e.type == EventType.MouseDown)) {

                    MarkSceneDirty();

					if (canvas.transform.childCount == 0) CreateLayer();
					if (currentLayer > canvas.transform.childCount - 1) currentLayer = 0;
					if (!canvas.transform.GetChild(currentLayer).gameObject.activeInHierarchy) return;
					if (currentDrawPos.x != Round(hit.point.x, t.tileSize) || currentDrawPos.y != Round(hit.point.y, t.tileSize))
						if (tool == "Draw") Draw(hit.point);
					Erase(hit.point);
					//Magic(hit.point);
				}
			}
			if (mode == "Erase") Handles.color = handlesColorRed;
			else if (mode == "Fix") Handles.color = handlesColorGreen;
			else Handles.color = handlesColorWhite;

			Vector2 d;
			d.x = Round(hit.point.x, t.tileSize);
			d.y = Round(hit.point.y, t.tileSize);
			Handles.CubeCap(1, new Vector3(d.x + t.tileSize * 0.5f, d.y + t.tileSize * 0.5f, 0.0f), Quaternion.identity, t.tileSize);
		}
	}

    void MarkSceneDirty()
    {
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
#else
					EditorApplication.MarkSceneDirty();
#endif
    }

    public void Draw(Vector3 hit) {
		Tile2D t = (Tile2D)drawTile.GetComponent(typeof(Tile2D));
		currentDrawPos.x = Round(hit.x, t.tileSize);
		currentDrawPos.y = Round(hit.y, t.tileSize);
		if (mode == "Draw" || tool == "BoxDraw") {
			GameObject dupeTarget = (GameObject)PrefabUtility.InstantiatePrefab(drawTile);
			newTile2D = (Tile2D)dupeTarget.transform.GetComponent(typeof(Tile2D));
			dupeTarget.transform.parent = canvas.transform.GetChild(currentLayer);
			dupeTarget.transform.position = new Vector3(currentDrawPos.x, currentDrawPos.y, 0);

            if (flipX)
            {
                dupeTarget.transform.localScale = Vector3.Scale(dupeTarget.transform.localScale, new Vector3(-1, 1, 1));
            }

			newTile2D.CacheComponents();
			if (canvas.transform.GetChild(currentLayer).childCount > 0)
				newTile2D.cacheRenderer.sortingLayerName = canvas.transform.GetChild(currentLayer).GetChild(0).GetComponent<Renderer>().sortingLayerName;
			newTile2D.CheckOverlap(enableUndo, enablePhysics);
			if (enableUndo) Undo.RegisterCreatedObjectUndo(dupeTarget, "TileTool2D: Draw");
			newTile2D.CheckIt(enableUndo, enablePhysics);

			if (!alt) newTile2D.FindCloseByHighestSortOrder();
			else newTile2D.FindCloseByLowestSortOrder();

			if (canvas.transform.GetChild(currentLayer).GetChild(0).GetComponent<Renderer>().sortingLayerID == 0)
				newTile2D.FixSortingLayer((canvas.transform.childCount - currentLayer - 1) * 2000 - 30000);
		}
	}

	public void Erase(Vector3 hit) {
		if (mode == "Erase" || mode == "Fix" || mode == "BoxErase") {
			Tile2D t = (Tile2D)drawTile.GetComponent(typeof(Tile2D));
			currentDrawPos.x = Round(hit.x, t.tileSize);
			currentDrawPos.y = Round(hit.y, t.tileSize);

			Collider2D[] tiles;



			float overLapSize = t.tileSize;
			tiles = Physics2D.OverlapAreaAll(new Vector2(hit.x - overLapSize, hit.y - overLapSize), new Vector2(hit.x + overLapSize, hit.y + overLapSize));


			// Physics based erase / fix
			if (enablePhysics) {
				for (int i = 0; i < tiles.Length; i++) {
					Tile2D _t = tiles[i].GetComponent<Tile2D>();
					if (_t.transform.parent != canvas.transform.GetChild(currentLayer)) return;
					if (tiles[i].transform.position.x == currentDrawPos.x && tiles[i].transform.position.y == currentDrawPos.y) {
						if (mode == "Erase" || mode == "BoxErase") {
							_t.transform.position = new Vector3(9999999.999999f, 9999999.999999f, 0);
							_t.BeautifyCloseByTiles(enableUndo, enablePhysics);
							if (enableUndo) Undo.DestroyObjectImmediate(tiles[i].gameObject);
							else DestroyImmediate(tiles[i].gameObject);
						} else {

							if (enableUndo) Undo.RegisterCompleteObjectUndo(_t.gameObject, "TileTool2D: Magic");
							_t.CheckIt(enableUndo, enablePhysics);
						}
					}
				}
			}

			// Layer based erase / fix
			Tile2D[] tiles2 = canvas.transform.GetChild(currentLayer).GetComponentsInChildren<Tile2D>();
			for (int i = 0; i < tiles2.Length; i++) {
				Rect rect = new Rect(hit.x - t.tileSize, hit.y - t.tileSize, t.tileSize*2, t.tileSize*2);
				Vector2 offsetPos = new Vector2(tiles2[i].transform.position.x + t.tileSize, tiles2[i].transform.position.y + t.tileSize);
				if (rect.Contains(offsetPos)) {
					Tile2D _t = tiles2[i];
					if (_t.transform.parent != canvas.transform.GetChild(currentLayer)) return;
					if (tiles2[i].transform.position.x == currentDrawPos.x && tiles2[i].transform.position.y == currentDrawPos.y) {
						if (mode == "Erase" || mode == "BoxErase") {
							_t.transform.position = new Vector3(9999999.999999f, 9999999.999999f, 0);
							_t.BeautifyCloseByTiles(enableUndo, enablePhysics);
							if (enableUndo) Undo.DestroyObjectImmediate(tiles2[i].gameObject);
							else DestroyImmediate(tiles2[i].gameObject);
						} else {
							if (enableUndo) Undo.RegisterCompleteObjectUndo(_t.gameObject, "TileTool2D: Magic");
							_t.CheckIt(enableUndo, enablePhysics);
						}
					}
				}
			}
		}
	}


	//public void Magic(Vector3 hit) {
	//	if (magic || magicLock) {
	//		Tile2D t = null;		
	//		Tile2D[] tiles = layerHolder.transform.GetChild(currentLayer).GetComponentsInChildren<Tile2D>();
	//		for (int i = 0; i < tiles.Length; i++) {
	//			Rect rect = new Rect(hit.x - eraseSize, hit.y - eraseSize, eraseSize*2, eraseSize*2);
	//			Vector2 offsetPos = new Vector2(tiles[i].transform.position.x + .5f, tiles[i].transform.position.y + .5f);
	//			if (rect.Contains(offsetPos)) {
	//				t = (Tile2D)tiles[i].transform.GetComponent(typeof(Tile2D));
	//				if (t.transform.parent != layerHolder.transform.GetChild(currentLayer)) return;
	//				if(enableUndo) Undo.RegisterCompleteObjectUndo(t.gameObject, "TileTool2D: Magic");
	//				t.CheckIt(enableUndo, enablePhysics);
	//			}
	//		}
	//	}
	//}

	public void Fix(Transform tr) {
		for (int i = 0; i < tr.childCount; i++) {
			float pp = (float)i;
			if (EditorUtility.DisplayCancelableProgressBar(
				"Fix Seams",
				"Fixing Seams between tiles",
				pp / tr.childCount)) {
				EditorUtility.ClearProgressBar();
				return;
			} else {
				Tile2D t = (Tile2D)tr.GetChild(i).GetComponent(typeof(Tile2D));
				//if (enableUndo) Undo.RegisterCompleteObjectUndo(t.gameObject, "TileTool2D: Magic");
				t.FindTiles(enablePhysics);
				t.Beautify();
				t.AddAttachment();
			}
		}
		EditorUtility.ClearProgressBar();
	}

	public void LoadSet() {
		string[] dir = Directory.GetDirectories("Assets/TileTool2D/Resources/Tiles/");
		sets = new string[dir.Length];
		for (int i = 0; i < dir.Length; i++) {
			
			string[] split = dir[i].Split("/"[0]);
			sets[i] = split[split.Length - 1];
		}
		//	sets[sets.Length - 1] = "All";
		//	if (currentSet == "All") currentSet = "";
		currentSet = sets[_index];

		string resourcePath = "Tiles/" + currentSet;
		
		Debug.Log("Loading from " + resourcePath);
		
		
		
		tiles = Resources.LoadAll<GameObject>( resourcePath);
		Debug.Log("Number of tiles: " + tiles.Length);
		
		//	if (_index == -1) _index = sets.Length - 1;
	}

	public void ChangeCanvas() {
		if (CanvasContainer.Length == 0) return;
		if (CanvasContainer.Length < canvaseIndex) canvaseIndex = 0;
		TileTool2DCanvas c = CanvasContainer[canvaseIndex];
		if (c != null) canvas = CanvasContainer[canvaseIndex].gameObject;
	}

	public float layerPanelHeight = .5f;
	public void OnGUI()
    {
		//GUI Style elements
		invisibleButton = new GUIStyle();
		invisibleButton.margin = new RectOffset(invisibleButtonMargin, invisibleButtonMargin, invisibleButtonMargin, invisibleButtonMargin);
		smallButton = new GUIStyle(GUI.skin.button);
		smallButton.fixedHeight = 16.0f;
		smallButton.fontSize = 9;

		GUILayout.Space(4.0f);

		// Canvas area
		GUILayout.BeginHorizontal();

		if (!canvas) EditorGUILayout.LabelField("No canvas found...", GUILayout.Width(Mathf.Min(position.width * 0.35f, 250)));
		else canvaseIndex = EditorGUILayout.Popup(canvaseIndex, canvasSets, GUILayout.Width(Mathf.Min(position.width * 0.35f, 250)));


        if (GUI.changed) ChangeCanvas();

        if (GUILayout.Button("NEW CANVAS", smallButton, GUILayout.Width(85.0f))) {
			CreateLayerHolder(true);
		}

		if (!canvas) GUI.enabled = false;

        // we dont want to allow canvases to be modified if theyre not in the origin position. So here
        // will be buttons for modify / return canvas.
        if (canvas)
        {
            TileTool2DCanvas canvasComponent = canvas.GetComponent<TileTool2DCanvas>();
            
            if (canvasComponent.editable)
            {
                if (GUILayout.Button("Return canvas"))
                {
                    canvasComponent.ReturnTransform();
                }
            }else
            {
                if (GUILayout.Button("Edit canvas"))
                {
                    canvasComponent.MemorizeTransform();
                    canvasComponent.PosForEdit();
                }
            }
        }

		if (enableUndo) GUI.color = Color.cyan;
		else GUI.color = Color.white;
		if (GUILayout.Button("ENABLE UNDO", smallButton, GUILayout.Width(95.0f))) {
			enableUndo = !enableUndo;
			if (enableUndo) EditorUtility.DisplayDialog("Enable Undo", "Undo takes a lot of resources.\n\n(Slow)", "OK");
			else EditorUtility.DisplayDialog("Enable Undo", "Undo functionality disabled.\n\n(Fast)", "OK");
		}

		if (enablePhysics) GUI.color = Color.cyan;
		else GUI.color = Color.white;
		if (GUILayout.Button("PHYSICS", smallButton, GUILayout.Width(65.0f))) {
			enablePhysics = !enablePhysics;
			if (enablePhysics) EditorUtility.DisplayDialog("Enable Physics", "Physics requires that all tiles  have colliders to be detected.\n\n(Fast)", "OK");
			else EditorUtility.DisplayDialog("Disable Physics", "Without using physics TileTool2D will loop trough all tiles in a layer to identify what kind of tile to use.\n\n(Slow)", "OK");
		}

		GUI.color = Color.white;
		GUILayout.EndHorizontal();

	    if (!EditableCanvas())
	    {
		    //Debug.Log("canvas not edibale");
		    return;
	    }

		// Tile folder area
		EditorGUILayout.BeginHorizontal();
		_index = EditorGUILayout.Popup(_index, sets, GUILayout.Width(Mathf.Min(position.width * 0.48f, 250)));
		spriteSize = (int)GUILayout.HorizontalSlider((float)spriteSize, 16.0f, 128.0f);
		GUILayout.EndHorizontal();
		if (GUI.changed) {
			LoadSet();
			FixLayerWindowSize();
		}

	    //layerPanelHeight = EditorGUILayout.FloatField(layerPanelHeight);

		//Scroll area for preview images
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false, GUILayout.Width(position.width), GUILayout.Height(position.height - (132 + (position.height * adjust))));
		EditorGUILayout.BeginHorizontal();
		int row = 0;
		if (!canvas) {
			GUI.enabled = true;
			GUILayout.Label("Please create a canvas to start using TileTool2D.");
			GUI.enabled = false;
		}

	    //Debug.Log("There's " + tiles.Length + " tiles.");

		for (int i = 0; i < tiles.Length; i++)
        {
	        int n = (int)((position.width/(spriteSize+invisibleButtonMargin))-.2f);
			if (tiles[i] == null || !canvas) break;
			Tile2D tile2D = tiles[i].GetComponent<Tile2D>();
			Sprite selectedSprite;
			if (tile2D && tile2D.thumbnail)
				selectedSprite = tile2D.thumbnail;
			else
				selectedSprite = tiles[i].GetComponent<SpriteRenderer>().sprite;

			if (selectedSprite == null) selectedSprite = Resources.Load<Sprite>("GUITextures/TT2DNotFound");
			Rect texCoords = new Rect(  selectedSprite.textureRect.x / selectedSprite.texture.width,
										selectedSprite.textureRect.y / selectedSprite.texture.height,
										selectedSprite.textureRect.width / selectedSprite.texture.width,
										selectedSprite.textureRect.height / selectedSprite.texture.height);

			//Texture preview aspect ratio
			float aspectW = 1f;
			float aspectH = 1f;
			if (selectedSprite.rect.height != selectedSprite.rect.width) {
				if (selectedSprite.rect.height < selectedSprite.rect.width)
					aspectW = selectedSprite.rect.height / selectedSprite.rect.width;
				else
					aspectH = selectedSprite.rect.width / selectedSprite.rect.height;
			}

			Rect texPos = new Rect(i % n * spriteSize + 2*(i % n) +2 , row*spriteSize + 2*row +2, spriteSize * aspectH, spriteSize * aspectW);
			if (tiles[i] == drawTile) texPos = new Rect(i % n * spriteSize + 2 * (i % n) + 2 + spriteSize * .075f, row * spriteSize + 2 * row + 2 + spriteSize * .075f, spriteSize * .85f * aspectH, spriteSize * .85f * aspectW);
			GUI.DrawTextureWithTexCoords(texPos, selectedSprite.texture, texCoords);
			if (GUILayout.Button("", invisibleButton, GUILayout.Width((float)spriteSize), GUILayout.Height((float)spriteSize))) {
				if (drawTile == tiles[i]) Selection.activeObject = tiles[i];
				drawTile = tiles[i];
				if (enablePhysics) {
					Collider2D c = drawTile.GetComponent<Collider2D>();
					if (c == null) Debug.LogError("No Collider found on " + drawTile + " : TileTool2D tiles require a collider. \n Disable collider on Awake instead. Disable physics by clicking the -Physics- button in TileTool2D. Double click thumnail in TileTool2D to select prefab.");
				}
			}
			if (tiles[i] == drawTile) GUI.DrawTexture(new Rect((float)((spriteSize + invisibleButtonMargin) * (i % n) + invisibleButtonMargin),
															   (float)((spriteSize + invisibleButtonMargin) * row + invisibleButtonMargin),
															   (float)spriteSize, (float)spriteSize), select);
			//Check if it time to start a new row based on window width
			if (i % n == n - 1) {
				row++;
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
			}
		}


		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndScrollView();

		//Button and slider area
		EditorGUILayout.BeginHorizontal();

		Texture b;

		b = selectT;
		if (tool == "Select") b = selectTActive;
		if (GUILayout.Button(b, invisibleButton, GUILayout.Width(32.0f), GUILayout.Height(32.0f))) {
			tool = mode = "Select";
			EnableSceneGUI(false);
		}
		if (tool != "Select") GUI.enabled = false;
		b = snapT;
		if (doSnap) b = snapTActive;
		if (GUILayout.Button(b, invisibleButton, GUILayout.Width(32.0f), GUILayout.Height(32.0f))) {
			doSnap = !doSnap;
		}
		if (canvas) GUI.enabled = true;

		b = brush;
		if (tool == "Draw") b = brushActive;
		if (GUILayout.Button(b, invisibleButton, GUILayout.Width(32.0f), GUILayout.Height(32.0f))) {
			if (tool == "Draw") tool = mode = "Select";
			else tool = mode = "Draw";
			if (tool == "Draw") EnableSceneGUI(true); else EnableSceneGUI(false);
		}
		b = eraser;
		if (tool == "Erase") b = eraserActive;
		if (GUILayout.Button(b, invisibleButton, GUILayout.Width(32.0f), GUILayout.Height(32.0f))) {
			if (tool == "Erase") tool = mode = "Select";
			else tool = mode = "Erase";
			if (tool == "Erase") EnableSceneGUI(true); else EnableSceneGUI(false);
		}
		b = magicWand;
		if (tool == "Fix") b = magicWandActive;
		if (GUILayout.Button(b, invisibleButton, GUILayout.Width(32.0f), GUILayout.Height(32.0f))) {
			if (tool == "Fix") tool = mode = "Select";
			else tool = mode = "Fix";
			if (tool == "Fix") EnableSceneGUI(true); else EnableSceneGUI(false);
		}


		b = box;
		if (tool == "BoxDraw") {
			b = boxActive;
		}
		if (GUILayout.Button(b, invisibleButton, GUILayout.Width(32.0f), GUILayout.Height(32.0f))) {
			if (tool == "BoxDraw") tool = mode = "Select";
			else tool = mode = "BoxDraw";
			if (tool == "BoxDraw") EnableSceneGUI(true); else EnableSceneGUI(false);
		}

		b = boxErase;
		if (tool == "BoxErase") {
			b = boxEraseActive;
		}
		if (GUILayout.Button(b, invisibleButton, GUILayout.Width(32.0f), GUILayout.Height(32.0f))) {
			if (tool == "BoxErase") tool = mode = "Select";
			else tool = mode = "BoxErase";
			if (tool == "BoxErase") EnableSceneGUI(true); else EnableSceneGUI(false);
		}

		GUILayout.Space(4.0f);
		EditorGUILayout.BeginVertical();

		if (drawTile) GUILayout.Label("Selected Tile:\n" + drawTile.name, GUILayout.Height(32));
		//	EditorGUILayout.BeginHorizontal();
		//	GUILayout.Label("Erase", GUILayout.Width(35));
		//	eraseSize = GUILayout.HorizontalSlider(eraseSize, 0.1f, 4.0f, GUILayout.Height(14));
		//	EditorGUILayout.EndHorizontal();		
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(2.0f);

		//Scroll area for layers
		layerScroll = EditorGUILayout.BeginScrollView(layerScroll, false, false, GUILayout.Width(position.width), GUILayout.Height(position.height * adjust + 25));
		EditorGUILayout.BeginHorizontal();

		if (canvas != null)
        {
			for (int j = 0; j < canvas.transform.childCount; j++) 
			{
				
				Transform childLayer = canvas.transform.GetChild(j);
				
				GUI.color = Color.gray;
				if (currentLayer == j) GUI.color = Color.white;
				
				//Fix window width when the scrollbar appears
				int fixWidth = 9;
				if (canvas.transform.childCount > 4) fixWidth = 24;
				EditorGUILayout.BeginHorizontal("Box", GUILayout.Width(position.width - fixWidth));
				GUI.color = Color.white;

				
				// Hide / show
				string hideText = "H";
				if (!childLayer.gameObject.activeInHierarchy) {
					hideText = "H";
					GUI.color = Color.red;
				}

                GUIContent hideLabel = new GUIContent(hideText, "hide/show layer");

				if (GUILayout.Button(hideLabel, smallButton, GUILayout.Width(25.0f))) {
					childLayer.gameObject.SetActive(!childLayer.gameObject.activeInHierarchy);
				}
				GUI.color = Color.white;
				
				

				// Selection 
                GUIContent selectLabel = new GUIContent("S", "select layer");

				if (currentLayer == j) GUI.color = Color.cyan;
				if (GUILayout.Button(selectLabel, smallButton, GUILayout.Width(25.0f))) {
					currentLayer = j;
					OnlyShowVisibleLayer(j, childLayer.gameObject);
				}
				
				// Set layer name
				childLayer.name = EditorGUILayout.TextField(childLayer.name, GUILayout.Width(75.0f));

				//Unity Layer Sorting
				if (childLayer.childCount == 0)
					GUI.enabled = false;
				else
					if (canvas) GUI.enabled = true;
				
				int sindex = 0;
				int cindex = 0;

				// Check the layer for a tile, so we can get the current unity sorting layer of this layer
				if (childLayer.childCount > 0)
				{
					Renderer tileRenderer = childLayer.GetComponentInChildren<Renderer>();
					cindex = sindex = GetSortIndex(tileRenderer.sortingLayerName);
				}

				sindex = EditorGUILayout.Popup(sindex, sortingLayers, GUILayout.Width(60f));
				if (cindex != sindex) {
					if (!warningSort) {
						Debug.LogWarning("TileTool2D: Unity Sorting Layers overrides TileTool2D layer sorting, use <b>Sort All</b> if going back to TileTool layers");
						warningSort = true;
					}

					SetLayerSortingLayers(sortingLayers[sindex], canvas.transform.GetChild(j));
				}
				if (canvas) GUI.enabled = true;
				if (j == 0)
					GUI.enabled = false;
				if (GUILayout.Button("▲", smallButton, GUILayout.Width(20.0f))) {
					LayerMove((canvas.transform.childCount - j) * 2000 - 30000, childLayer);
					LayerMove((canvas.transform.childCount - j - 1) * 2000 - 30000, canvas.transform.GetChild(j - 1));
					childLayer.SetSiblingIndex(j - 1);
					if (currentLayer == j)
						currentLayer = j - 1;
					else if (currentLayer == j - 1)
						currentLayer = j;
				}
				if (canvas) GUI.enabled = true;
				if (j == canvas.transform.childCount - 1)
					GUI.enabled = false;
				if (GUILayout.Button("▼", smallButton, GUILayout.Width(20.0f))) {
					LayerMove((canvas.transform.childCount - j - 2) * 2000 - 30000, childLayer);
					LayerMove((canvas.transform.childCount - j - 1) * 2000 - 30000, canvas.transform.GetChild(j + 1));
					childLayer.SetSiblingIndex(j + 1);
					if (currentLayer == j)
						currentLayer = j + 1;
					else if (currentLayer == j + 1)
						currentLayer = j;
				}
				if (canvas) GUI.enabled = true;


                // leo code for color
                GameObject layer = childLayer.gameObject;
                if (layer)
                {
                    // Check for / add color children component

	                ColorChildren colorer = layer.GetComponent<ColorChildren>();
	                if (colorer == null) colorer = layer.AddComponent<ColorChildren>(); //SpiderWeb.GO.MakeComponent<ColorChildren>(layer);
                    SpriteRenderer sr = colorer.GetComponent<SpriteRenderer>();

                    BlendModes.BlendMode oldMode = colorer.blendMode;

                    // Get the color for the layer
                    Color c = sr.color;
                    sr.color = EditorGUILayout.ColorField(sr.color, GUILayout.Width(35));

                    //blend modes
                    colorer.blendMode = (BlendModes.BlendMode)EditorGUILayout.EnumPopup(colorer.blendMode, GUILayout.Width(55));

                    // if the color has changed
                    if (GUI.changed)
                    {
                        // apply the new color
                        if (c != sr.color)
                        {
                            colorer.SetColor(sr.color);
                        }

                        if (oldMode != colorer.blendMode)
                        {
                            colorer.ApplyBlendMode();
                        }

                        // mark object as dirty so it saves
                        EditorUtility.SetDirty(canvas);
                        EditorUtility.SetDirty(colorer);
                    }                   
                }

				GUILayout.FlexibleSpace();
				//GUILayout.Label("" + child.childCount);
				if (childLayer.childCount == 0) GUI.enabled = false;
				if (GUILayout.Button("FIX", smallButton, GUILayout.Width(40.0f)) && (childLayer.childCount == 0 || EditorUtility.DisplayDialog("Fix Tile Seams?", "Fix " + childLayer.name + " containing " + childLayer.childCount + " tiles?\n\nThis can not be undone. \nConsider saving scene before using this feature.", "Yes", "No"))) {
					Fix(childLayer);
				}
				if (canvas) GUI.enabled = true;
				if (GUILayout.Button("X", smallButton, GUILayout.Width(20.0f)) && (childLayer.childCount == 0 || EditorUtility.DisplayDialog("Delete Layer?", "Delete " + childLayer.name + " containing " + childLayer.childCount + " tiles?", "Yes", "No"))) {
					if (enableUndo) Undo.DestroyObjectImmediate(childLayer.gameObject);
					else DestroyImmediate(childLayer.gameObject);
					FixLayerWindowSize();
					SortAll(false);
				}

                GUI.color = Color.white;

                EditorGUILayout.EndHorizontal();
				if (j < canvas.transform.childCount) {
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
				}
			}
		}


		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndScrollView();

		//Area for status text and add layer button
		GUI.color = Color.clear;
		GUILayout.BeginHorizontal("Box", GUILayout.Width(position.width - 9));
		GUI.color = Color.white;
		GUILayout.Label("Mode: " + ModeText());
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("SORT ALL", smallButton, GUILayout.Width(65.0f))) {
			SortAll(true);
		}
		if (GUILayout.Button("CLEAR SORT", smallButton, GUILayout.Width(80.0f))) {
			ClearSortAll();
		}
		if (GUILayout.Button("NEW LAYER", smallButton, GUILayout.Width(75.0f))) {
			CreateLayer();
		}
		EditorGUILayout.EndHorizontal();
	}

	public void FixLayerWindowSize() {
		adjust = AdjustLayerScrollSize();
		layerCount = canvas.transform.childCount;
	}

	public void OnlyShowVisibleLayer(int n, GameObject go) {
		if (onlyVisible) {
			for (int i = 0; i < canvas.transform.childCount; i++) {
				GameObject l = canvas.transform.GetChild(i).gameObject;
				if (i != currentLayer) {
					l.SetActive(false);
				} else l.SetActive(true);
			}
		}
	}

	public void LayerMove(int layerIndex, Transform layer) {
		for (int i = 0; i < layer.childCount; i++) {
			Tile2D t = layer.GetChild(i).GetComponent<Tile2D>();
            if (!t) continue;
			t.FixSortingLayer(layerIndex);
		}
	}

	public void SortAll(bool dialog) {
		if (canvas == null) return;
		if (!dialog || EditorUtility.DisplayDialog("Sort All", "Sort all tiles in all layers? \nConsider saving scene before using this feature.", "Yes", "No")) {
			for (int i = 0; i < canvas.transform.childCount; i++) {
                if (enableUndo)
                {
                    if (canvas)
                        if (canvas.transform.GetChild(i))
                            Undo.RegisterCompleteObjectUndo(canvas.transform.GetChild(i).gameObject, "TileTool2D: Sort All");
                }
				LayerMove((canvas.transform.childCount - i - 1) * 2000 - 30000, canvas.transform.GetChild(i));
			}
		}
	}

	public void ClearSortAll() {
		if (canvas != null) {
			if (!EditorUtility.DisplayDialog("Clear Sort", "Clear sorting on all tiles in all layers? \nConsider saving scene before using this feature.", "Yes", "No")) return;
			Renderer[] renderers = canvas.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < renderers.Length; i++) {
				if (enableUndo) Undo.RegisterCompleteObjectUndo(renderers[i].transform, "TileTool 2D: Sorting Layer Changed");
				renderers[i].sortingOrder = 0;
			}
		}
	}

    int counter = 0;
	public void OnInspectorUpdate()
    {
        if (!canvas || !EditableCanvas()) return;

        // refresh every 10 frames
        if (counter < 10)
        {
            counter++;
            return;
        }
        counter = 0;

        if (canvas)
        {

            // apply the new color
            for (int i = 0; i < canvas.transform.childCount; i++)
            {
                GameObject layerObj = canvas.transform.GetChild(i).gameObject;

                if (layerObj)
                {
                    ColorChildren c = layerObj.GetComponent<ColorChildren>();
                    if (c)
                    {
                        c.SetColor();
                        c.ApplyBlendMode();
                    }
                }
            }

            // mark object as dirty so it saves
            EditorUtility.SetDirty(canvas);

        }

        //Repaint();
        //SceneView.RepaintAll();
        //EditorUtility.ClearProgressBar();
    }


	public void Update() {

        if (!canvas || !EditableCanvas()) return;

        //SceneView.RepaintAll();
        if (Selection.transforms.Length > 0 && !EditorApplication.isPlaying && doSnap && Selection.transforms[0].position != prevPosition) snap(true);
		CreateDrawField();
		if (EditorApplication.isPlaying)
			RemoveDrawField();
		//CreateLayerHolder(false);
	}

	public void RemoveDrawField() {
		if (drawField != null)
			DestroyImmediate(drawField);
	}

	public void CreateDrawField() {
		if (drawField == null) {
			drawField = GameObject.Find("TileTool2D");
			if (drawField == null) {
				drawField = new GameObject();
				BoxCollider b = drawField.AddComponent<BoxCollider>();
				b.size = new Vector3(9999999.0f, 9999999.0f, 0.0f);
				drawField.hideFlags = HideFlags.DontSaveInBuild;
				drawField.hideFlags = HideFlags.DontSaveInEditor;
				drawField.hideFlags = HideFlags.HideAndDontSave;
				drawField.name = "TileTool2D";
			}
		}
	}

	public void FindCanvases() {
		CanvasContainer = GameObject.FindObjectsOfType<TileTool2DCanvas>();
		canvasSets = new String[CanvasContainer.Length];
		for (int i = 0; i < CanvasContainer.Length; i++) {
			canvasSets[i] = CanvasContainer[i].name;
		}
	}

	public void CreateLayerHolder(bool force) {
		FindCanvases();
		if (!force && canvas == null && CanvasContainer.Length > 0) {
			canvaseIndex = 0;
		} else if (canvas == null || force) {
			canvas = GameObject.Find("TileTool2DLayers");
			if (canvas && !canvas.GetComponent<TileTool2DCanvas>())
				canvas.AddComponent<TileTool2DCanvas>();
			if (canvas == null || force) {
				canvas = new GameObject();
				int n = CanvasContainer.Length;
				canvas.name = "TileTool2D Canvas " + n;
				canvas.AddComponent<TileTool2DCanvas>();
				CreateLayer();
				canvaseIndex = 0;
			}
		}
		FindCanvases();
	}

	public void CreateLayer() {
		if (canvas.transform.childCount > 30) {
			Debug.LogError("TileTool2D: Max 30 layers");
			return;
		}
		if (canvas == null) return;
		GameObject newLayer = new GameObject();
		newLayer.name = "New Layer";
		newLayer.transform.parent = canvas.transform;
		newLayer.transform.SetSiblingIndex(0);
		adjust = AdjustLayerScrollSize();
		layerCount = canvas.transform.childCount;
		if (enableUndo) Undo.RegisterCreatedObjectUndo(newLayer, "TileTool2D: New Layer");
	}

	public float AdjustLayerScrollSize()
    {
		if (!canvas || canvas.transform.childCount <= 1) return 0.02f;

        return Mathf.Clamp(canvas.transform.childCount * .04f, .1f, 0.4f);
	}

	public void snap(bool onlyTiles) {
		_selection = Selection.transforms;
		try {
			for (int i = 0; i < Selection.transforms.Length; i++) {
				Tile2D TTT = (Tile2D)_selection[i].GetComponent(typeof(Tile2D));
				if (onlyTiles && (TTT != null) || !onlyTiles) {
					snapValue = TTT.tileSize;
					if (!onlyTiles) {
						if (enableUndo) Undo.RecordObjects(_selection, "TileTool: Snapping");
					}
					Vector3 t = Selection.transforms[i].transform.position;
					t.x = Round(t.x, snapValue);
					t.y = Round(t.y, snapValue);
					t.z = Round(t.z, snapValue);
					Selection.transforms[i].transform.position = t;
				}
			}
			prevPosition = Selection.transforms[0].position;
		} catch (System.Exception e) {
			Debug.LogError("Nothing to move.  " + e);
		}
	}

	public float Round(float input, float size) {
		float snappedValue = 0.0f;
		snappedValue = size * Mathf.Floor((input / size));
		return (snappedValue);
	}

	public String[] GetSortingLayerNames() {
		Type internalEditorUtilityType = typeof(InternalEditorUtility);
		PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
		return (String[])sortingLayersProperty.GetValue(null, new System.Object[0]);
	}

	public int GetSortIndex(string layerName) {
		int n = 0;
		for (int i = 0; i < sortingLayers.Length; i++) {
			if (sortingLayers[i] == layerName) {
				return i;
			}
		}
		return n;
	}

	public void SetLayerSortingLayers(string layerName, Transform layer) {
		Renderer[] renderers = layer.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < renderers.Length; i++) {
			if (enableUndo) Undo.RegisterCompleteObjectUndo(renderers[i].transform, "TileTool 2D: Sorting Layer Changed");
			renderers[i].sortingLayerName = layerName;
		}
	}
}