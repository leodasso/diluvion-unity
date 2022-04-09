using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Diluvion;
using UnityEngine.EventSystems;

[CustomEditor(typeof(InteriorGrid))]
public class NewBehaviourScript : Editor {

    static bool _editingGrid;

    InteriorGrid _grid;

    Vector3 _handlePos;

    Vector3 _squareOffset;

    Vector3 _mousePos;

	void OnEnable()
    {
        _grid = target as InteriorGrid;
        _squareOffset = new Vector3(1, 1, 0) * (InteriorGrid.cellSize / 2);

        _handlePos = new Vector3(_grid.width, _grid.height, 0) * InteriorGrid.cellSize;
        _handlePos += (Vector3)_grid.offset;
        _handlePos -= _squareOffset * 2;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();

        _editingGrid = EditorGUILayout.ToggleLeft("Edit", _editingGrid);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Empty grid"))
        {
            _grid.EmptyCells();
            EditorUtility.SetDirty(target);
        }
        if (GUILayout.Button("snap"))
        {
            _grid.SnapOffset();
            EditorUtility.SetDirty(target);
        }
        EditorGUILayout.EndHorizontal();
    }

    bool _fill;

    void OnSceneGUI()
    {
        if (!_grid.enabled) return;
        if (!_editingGrid) return;
       
        
        // Get the mouse position
        if (Event.current != null)
        {
            _mousePos = Event.current.mousePosition;
            _mousePos.y = SceneView.currentDrawingSceneView.camera.pixelHeight - _mousePos.y;
            _mousePos = SceneView.currentDrawingSceneView.camera.ScreenToWorldPoint(_mousePos);


            _fill = Event.current.control;

        }

        var mouseLabel = new GUIContent("Click to toggle");
        if (_fill)
            mouseLabel = new GUIContent("Click to remove all adjacent");
        
        // draw label over mouse position
        Handles.Label(_mousePos, mouseLabel);

        float size = HandleUtility.GetHandleSize(_grid.transform.position);
        Handles.matrix = Matrix4x4.TRS(_grid.transform.position, _grid.transform.rotation, Vector3.one);

        // Get the total rect size from the offset (lower left) to the handle position (upper right)
        Vector2 area = (Vector2)_handlePos - _grid.offset + (Vector2)_squareOffset;

        // Check how many cells will fit in that area
        int w = Mathf.CeilToInt(area.x / InteriorGrid.cellSize);
        int h = Mathf.CeilToInt(area.y / InteriorGrid.cellSize);

        w = Mathf.Clamp(w, 1, 99);
        h = Mathf.Clamp(h, 1, 99);

        if (w != _grid.width || h != _grid.height)
        {
            _grid.ResizeGrid(w, h);
            EditorUtility.SetDirty(target);
        }

        // Put a handle at lower left corner to drag offset
        Handles.color = Color.green;
        _grid.offset = Handles.Slider2D(_grid.offset, Vector3.forward, Vector3.up, Vector3.left, .5f * size, Handles.CircleHandleCap, InteriorGrid.cellSize);
        Handles.Label(_grid.offset, new GUIContent("lower left"));

        // put a handle at the corner to drag the grid size
        _handlePos = Handles.Slider2D(_handlePos, Vector3.forward, Vector3.up, Vector3.left, .5f * size, Handles.CircleHandleCap, InteriorGrid.cellSize);
        Handles.Label(_handlePos, new GUIContent("upper right"));

        _grid.DoForEachCell(DrawCellButton);
       
    }

    void DrawCellButton(Vector3 pos, int x, int y, int index)
    {
        if (index >= _grid.grid.Length) return;
        Color gridColor = Color.cyan;
        if (_grid.grid[index] == -1) gridColor = new Color(.5f, .5f, .5f, .1f);
        else if (_grid.grid[index] == 1)
        {
            gridColor = Color.red;
            Handles.color = new Color(1, 0, 0, .2f);
			Handles.CubeHandleCap(0, pos, Quaternion.LookRotation(Vector3.forward), InteriorGrid.cellSize, EventType.ContextClick);
            //Handles.CubeCap(0, pos, Quaternion.LookRotation(Vector3.forward), InteriorGrid.cellSize );
        }

        Handles.color = gridColor;

        if (Handles.Button(pos, Quaternion.LookRotation(Vector3.forward), InteriorGrid.cellSize/2, InteriorGrid.cellSize/2, Handles.RectangleHandleCap))
        {
            if (_fill)
            {
                TurnOffAllAdjacent(x, y);
            }
            else
            {
                int i = _grid.grid[index];
                if (i != -1) i = -1;
                else i = 0;
                _grid.grid[index] = i;
            }

            EditorUtility.SetDirty(target);
        }

        Handles.color = Color.white;
    }

    /// <summary>
    /// Recursively turns off all adjacent cells to the cell at the given coordinates.
    /// </summary>
    void TurnOffAllAdjacent(int x, int y)
    {
        if (!_grid.ValidCoords(x, y)) return;
        int index = _grid.IndexOfCoords(x, y, _grid.width);
        if (_grid.grid[index] == -1) return;

        _grid.grid[index] = -1;
        
        TurnOffAllAdjacent(x, y + 1);
        TurnOffAllAdjacent(x - 1, y);
        TurnOffAllAdjacent(x + 1, y);
        TurnOffAllAdjacent(x, y - 1);
    }
}
