using System.Collections.Generic;

namespace Flipit.CityTerrain
{
    /// <summary>
    /// Pure data model representing the city grid. No Unity dependencies except Vector2Int.
    /// All placement logic operates on this grid before any GameObjects are created.
    /// Grid indexed by [row, col] where row = depth dimension, col = width dimension.
    /// </summary>
    public class Grid_Layout
    {
        public int Width { get; }
        public int Depth { get; }
        public float CellSize { get; }
        public CellData[,] Cells { get; }

        /// <summary>
        /// Creates a new grid with the given dimensions and cell size.
        /// All cells are initialized with their row/col positions and Empty type.
        /// </summary>
        public Grid_Layout(int width, int depth, float cellSize)
        {
            Width = width;
            Depth = depth;
            CellSize = cellSize;
            Cells = new CellData[depth, width];

            for (int row = 0; row < depth; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    Cells[row, col] = new CellData(row, col, CellType.Empty, false);
                }
            }
        }

        /// <summary>
        /// Returns the cell data at the specified row and column.
        /// </summary>
        public CellData GetCell(int row, int col)
        {
            return Cells[row, col];
        }

        /// <summary>
        /// Sets the cell type at the specified row and column.
        /// </summary>
        public void SetCellType(int row, int col, CellType type)
        {
            CellData cell = Cells[row, col];
            cell.Type = type;
            Cells[row, col] = cell;
        }

        /// <summary>
        /// Marks the cell at the specified position as occupied with the given tag.
        /// </summary>
        public void MarkOccupied(int row, int col, string tag)
        {
            CellData cell = Cells[row, col];
            cell.IsOccupied = true;
            cell.OccupantTag = tag;
            Cells[row, col] = cell;
        }

        /// <summary>
        /// Returns true if the cell at the specified position is not occupied.
        /// </summary>
        public bool IsCellAvailable(int row, int col)
        {
            return !Cells[row, col].IsOccupied;
        }

        /// <summary>
        /// Returns all cells matching the specified type.
        /// </summary>
        public List<CellData> GetCellsOfType(CellType type)
        {
            var result = new List<CellData>();
            for (int row = 0; row < Depth; row++)
            {
                for (int col = 0; col < Width; col++)
                {
                    if (Cells[row, col].Type == type)
                    {
                        result.Add(Cells[row, col]);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns all cells matching the specified type that are not occupied.
        /// </summary>
        public List<CellData> GetUnoccupiedCellsOfType(CellType type)
        {
            var result = new List<CellData>();
            for (int row = 0; row < Depth; row++)
            {
                for (int col = 0; col < Width; col++)
                {
                    if (Cells[row, col].Type == type && !Cells[row, col].IsOccupied)
                    {
                        result.Add(Cells[row, col]);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns orthogonal neighbors (up, down, left, right) of the specified cell
        /// that are within grid bounds.
        /// </summary>
        public List<CellData> GetAdjacentCells(int row, int col)
        {
            var result = new List<CellData>(4);

            // Up (row - 1)
            if (row > 0)
                result.Add(Cells[row - 1, col]);
            // Down (row + 1)
            if (row < Depth - 1)
                result.Add(Cells[row + 1, col]);
            // Left (col - 1)
            if (col > 0)
                result.Add(Cells[row, col - 1]);
            // Right (col + 1)
            if (col < Width - 1)
                result.Add(Cells[row, col + 1]);

            return result;
        }

        /// <summary>
        /// Returns Building cells on grid edges (row==0, row==Depth-1, col==0, col==Width-1)
        /// that are adjacent to at least one Sidewalk cell.
        /// </summary>
        public List<CellData> GetEdgeBuildingCells()
        {
            var result = new List<CellData>();

            for (int row = 0; row < Depth; row++)
            {
                for (int col = 0; col < Width; col++)
                {
                    if (!IsOnEdge(row, col))
                        continue;

                    if (Cells[row, col].Type != CellType.Building)
                        continue;

                    // Check if adjacent to at least one Sidewalk cell
                    var neighbors = GetAdjacentCells(row, col);
                    bool adjacentToSidewalk = false;
                    foreach (var neighbor in neighbors)
                    {
                        if (neighbor.Type == CellType.Sidewalk)
                        {
                            adjacentToSidewalk = true;
                            break;
                        }
                    }

                    if (adjacentToSidewalk)
                    {
                        result.Add(Cells[row, col]);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns true if the cell is on the edge of the grid
        /// (row==0, row==Depth-1, col==0, or col==Width-1).
        /// </summary>
        public bool IsOnEdge(int row, int col)
        {
            return row == 0 || row == Depth - 1 || col == 0 || col == Width - 1;
        }
    }
}
