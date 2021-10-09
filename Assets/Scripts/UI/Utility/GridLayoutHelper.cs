using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Sirenix.OdinInspector;

[ExecuteInEditMode]
public class GridLayoutHelper : MonoBehaviour {

	public GridLayoutGroup gridLayoutGroup;

    [ToggleLeft]
	[Tooltip("Makes each cell's width match the layout group's width.")]
	public bool matchCellWidth;

    [MinValue(1), ShowIf("matchCellWidth")]
    public int columnCount = 1;

	public bool widthCopiesHeight;

    [ToggleLeft]
    [Tooltip("Makes each cell's height match the layout group's height.")]
	public bool matchCellHeight;

    [MinValue(1), ShowIf("matchCellHeight")]
    public int rowCount = 1;

    [Space]
    [ToggleLeft]
    [Tooltip("Divides the cell's width / height by the number of cells.")]
	public bool makeCellsFitWidth;

    [ToggleLeft]
    public bool makeCellsFidHeight;


	// Use this for initialization
	void Start () {
	
		// If no layout group defined, search for one on this object
		if (gridLayoutGroup == null) 
			gridLayoutGroup = GetComponent<GridLayoutGroup>();
	}
	
	// Update is called once per frame
	void Update () {

		Match();	
	}

	void Match() 
	{
		if (gridLayoutGroup == null) return;

		float width = gridLayoutGroup.cellSize.x;
		float height = gridLayoutGroup.cellSize.y;

		float divisor = (float)gridLayoutGroup.transform.childCount;

		if (matchCellWidth)
		{
			width = gridLayoutGroup.GetComponent<RectTransform>().rect.width - gridLayoutGroup.padding.left -
			        gridLayoutGroup.padding.right;
            if (makeCellsFitWidth)
                width /= divisor;
            else width /= columnCount;

			width -= gridLayoutGroup.spacing.x;
		}

		if (matchCellHeight) {
			height = gridLayoutGroup.GetComponent<RectTransform>().rect.height - gridLayoutGroup.padding.top - 
			         gridLayoutGroup.padding.bottom;
            if (makeCellsFidHeight)
                height /= divisor;
            else height /= rowCount;

			height -= gridLayoutGroup.spacing.y;
		}

		if (widthCopiesHeight) width = height;

		// Apply new size to the grid layout
		gridLayoutGroup.cellSize = new Vector2(width, height);
	}
}
