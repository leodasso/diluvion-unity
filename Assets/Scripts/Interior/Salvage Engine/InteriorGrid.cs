using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Diluvion
{
    /// <summary>
    /// Creates a grid of square cells in an interior, and can procedurally place elements within.
    /// </summary>
    [ExecuteInEditMode]
    public class InteriorGrid : MonoBehaviour
    {
        public static float cellSize = .25f;
        public Vector2 offset;

        [Space]
        [HideInInspector]
        public int width = 5;
        [HideInInspector]
        public int height = 5;

        /// <summary>
        /// An array of all the cells in this interior grid, starting at bottom left, and ending at top right.
        /// <para>-1 represents a cell that takes no space; 0 takes space but is empty; 1
        /// takes space and is filled with something else as well.</para>
        /// </summary>
        public int[] grid = new int[25];

        public delegate void CellDelegate(Vector3 pos, int x, int y, int index);

        int[] oldGrid;


        /// <summary>
        /// Performs the given cell function on each cell.
        /// </summary>
        /// <param name="cellFunction">A function with a position, x, and y value of the cell.</param>
        public void DoForEachCell(CellDelegate cellFunction, int newWidth, int newHeight)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = IndexOfCoords(x, y, newWidth);
                    if (index >= grid.Length) continue;

                    Vector3 center = new Vector3(x, y, 0) * cellSize + (Vector3)offset;
                    cellFunction(center, x, y, index);
                }
            }
        }

        public void SnapOffset()
        {
            Vector2 newOffset = offset * 8;
            newOffset = new Vector2(Mathf.Round(newOffset.x)/8, Mathf.Round(newOffset.y)/8);

            offset = newOffset;
        }

        /// <summary>
        /// Performs the given cell function on each cell.
        /// </summary>
        /// <param name="cellFunction">A function with a position, x, and y value of the cell.</param>
        public void DoForEachCell(CellDelegate cellFunction)
        {
            DoForEachCell(cellFunction, width, height);
        }

        /// <summary>
        /// Empties all the cell that are occupied.
        /// </summary>
        public void EmptyCells()
        {
            for (int i = 0; i < grid.Length; i++) grid[i] = Mathf.Clamp(grid[i], -1, 0);
        }


        /// <summary>
        /// Sets the grid to the given width and height. Memorizes previously set cell values.
        /// </summary>
        public void ResizeGrid(int newWidth, int newHeight)
        {
            // Clamp the new dimensions
            newWidth = Mathf.Clamp(newWidth, 0, 99);
            newHeight = Mathf.Clamp(newHeight, 0, 99);

            // Copy the grid into a duplicate 'old grid'
            oldGrid = new int[grid.Length];
            for (int i = 0; i < oldGrid.Length; i++) oldGrid[i] = grid[i];

            // Reset the grid array. 
            grid = new int[newWidth * newHeight];

            // Duplicate the old cells into the new ones
            DoForEachCell(DuplicateCell, newWidth, newHeight);

            width = newWidth;
            height = newHeight;
        }

        /// <summary>
        /// Duplicates the cell of the given coordinates from grid[] to oldgrid[]. 
        /// Used to memorize values when resetting the grid.
        /// </summary>
        void DuplicateCell(Vector3 pos, int x, int y, int i)
        {
            int oldIndex = IndexOfCoords(x, y, width);
            if (oldIndex >= oldGrid.Length)
            {
                grid[i] = 0;
                return;
            }

            grid[i] = oldGrid[oldIndex];
        }

        #region grid placement
        /// <summary>
        /// Attempts to insert this grid into the other grid 'parent grid'
        /// </summary>
        public bool TryInsertGrid(InteriorGrid parentGrid, bool actuallyPlace = false)
        {
            if (parentGrid.width < width || parentGrid.height < height)
                return false;

            int tryIndex = 0;
            while (tryIndex < parentGrid.grid.Length)
            {
                // Try to place this grid at the given index of the parent grid
                if (!AttemptGridPlacement(tryIndex, parentGrid)) tryIndex++;

                else
                {
                    if (actuallyPlace)
                    {
                        // If placement was successful, tell the parent grid to mark those grid cells as 'stored'
                        AttemptGridPlacement(tryIndex, parentGrid, true);

                        Vector2 coords = CoordsOfIndex(tryIndex, parentGrid.width);
                        PlaceAtCoords((int)coords.x, (int)coords.y, parentGrid);
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Places this grid such that the lower left cell will be placed over the cell of the given coordinates.
        /// </summary>
        /// <param name="parentGrid">The grid to place this into.</param>
        public void PlaceAtCoords(int x, int y, InteriorGrid parentGrid)
        {
            transform.SetParent(parentGrid.transform.parent, true);
            transform.localEulerAngles = Vector3.zero;
            Vector3 myOffset = CoordsToLocalSpace(0, 0);

            transform.localPosition = parentGrid.CoordsToLocalSpace(x, y) + parentGrid.transform.localPosition - myOffset;
        }

        /// <summary>
        /// Attempts to place this grid into the parentGrid with the lower left corner (index 0) being at the 
        /// given index of the other grid.
        /// </summary>
        /// <param name="finalize">Sets the parent grid blocks that are being used as 'full'</param>
        bool AttemptGridPlacement(int atIndex, InteriorGrid parentGrid, bool finalize = false)
        {
            // Represents the index of my own cells to check. This eventually iterates through all the cells
            int myIndex = 0;

            // The coordinates of the cell in the parent grid that we're starting the check at.
            Vector2 originCoords = CoordsOfIndex(atIndex, parentGrid.width);

            // Check through every one of my cells!
            while (myIndex < grid.Length)
            {
                // Only check for cells that occupy space
                if (grid[myIndex] >= 0)
                {
                    // Check if there's room in the parent grid
                    Vector2 myCoords = CoordsOfIndex(myIndex, width);

                    Vector2 totalCoords = myCoords + originCoords;

                    if (!parentGrid.ValidCoords((int)totalCoords.x, (int)totalCoords.y)) return false;

                    int parentIndex = IndexOfCoords((int)totalCoords.x, (int)totalCoords.y, parentGrid.width);

                    // Check for empty or non-existent grid cells
                    if (parentIndex >= parentGrid.grid.Length) return false;
                    if (parentGrid.grid[parentIndex] != 0) return false;

                    // Mark the occupied grid cell as full (only if finalizing)
                    if (finalize) parentGrid.grid[parentIndex] = 1;
                }

                myIndex++;
            }

            return true;
        }
        #endregion


        #region coordinates and positions

        /// <summary>
        /// Returns the local space coordinate of the grid cell at the given coordinates.
        /// </summary>
        public Vector3 CoordsToLocalSpace(int x, int y)
        {
            int i = IndexOfCoords(x, y, width);

            if (i >= grid.Length) return Vector3.zero;

            return (Vector3)offset + new Vector3(x, y, 0) * cellSize;
        }


        /// <summary>
        /// Returns the index in grid[] of the cell at the given coordinates.
        /// </summary>
        /// <param name="w">The width of the grid.</param>
        public int IndexOfCoords(int x, int y, int w)
        {
            return y * w + x;
        }

        /// <summary>
        /// Check if the coordinates are within my shape
        /// </summary>
        public bool ValidCoords(int x, int y)
        {
            if (x >= width || x < 0) return false;
            if (y >= height || y < 0) return false;

            if (IndexOfCoords(x, y, width) >= grid.Length) return false;
            return true;
        }

        /// <summary>
        /// Returns the coordinates of the cell at the given index in grid[]
        /// </summary>
        /// <param name="w">The width of the grid.</param>
        public Vector2 CoordsOfIndex(int index, int w)
        {
            int y = Mathf.FloorToInt(index / w);
            int x = index % w;
            return new Vector2(x, y);
        }

        #endregion
    }
}