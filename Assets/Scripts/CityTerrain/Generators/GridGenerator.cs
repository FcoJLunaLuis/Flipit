namespace Flipit.CityTerrain
{
    /// <summary>
    /// Generates the grid cell type assignments. Pure logic, no GameObjects.
    /// </summary>
    public static class GridGenerator
    {
        /// <summary>
        /// Assigns Street, Sidewalk, and Building types to all cells.
        /// Streets at every 'streetInterval' row and column.
        /// Sidewalks on cells adjacent to streets (that aren't streets themselves).
        /// Remaining cells become Building.
        /// Also marks intersection cells where both row and column are street positions.
        /// </summary>
        public static void AssignCellTypes(Grid_Layout grid, int streetInterval)
        {
            // Clamp streetInterval to minimum of 3
            if (streetInterval < 3)
                streetInterval = 3;

            int depth = grid.Depth;
            int width = grid.Width;

            // Pass 1: Assign Street or Building (provisional)
            for (int row = 0; row < depth; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    bool isStreetRow = row % streetInterval == 0;
                    bool isStreetCol = col % streetInterval == 0;

                    if (isStreetRow || isStreetCol)
                    {
                        bool isIntersection = isStreetRow && isStreetCol;
                        grid.Cells[row, col] = new CellData(row, col, CellType.Street, isIntersection);
                    }
                    else
                    {
                        grid.Cells[row, col] = new CellData(row, col, CellType.Building, false);
                    }
                }
            }

            // Pass 2: Promote Building cells adjacent to Street cells to Sidewalk
            for (int row = 0; row < depth; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    if (grid.Cells[row, col].Type != CellType.Building)
                        continue;

                    if (HasAdjacentStreet(grid, row, col))
                    {
                        grid.Cells[row, col] = new CellData(row, col, CellType.Sidewalk, false);
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the given cell is at a street intersection
        /// (row is a street row AND col is a street column).
        /// </summary>
        public static bool IsIntersection(int row, int col, int streetInterval)
        {
            if (streetInterval < 3)
                streetInterval = 3;

            return (row % streetInterval == 0) && (col % streetInterval == 0);
        }

        /// <summary>
        /// Checks if any orthogonal neighbor of the given cell is a Street cell.
        /// </summary>
        private static bool HasAdjacentStreet(Grid_Layout grid, int row, int col)
        {
            // Up
            if (row > 0 && grid.Cells[row - 1, col].Type == CellType.Street)
                return true;
            // Down
            if (row < grid.Depth - 1 && grid.Cells[row + 1, col].Type == CellType.Street)
                return true;
            // Left
            if (col > 0 && grid.Cells[row, col - 1].Type == CellType.Street)
                return true;
            // Right
            if (col < grid.Width - 1 && grid.Cells[row, col + 1].Type == CellType.Street)
                return true;

            return false;
        }
    }
}
