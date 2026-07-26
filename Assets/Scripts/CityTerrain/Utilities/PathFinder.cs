using System.Collections.Generic;

namespace Flipit.CityTerrain
{
    /// <summary>
    /// BFS pathfinding on the grid. Finds navigable paths along
    /// street and sidewalk cells between two points.
    /// </summary>
    public static class PathFinder
    {
        /// <summary>
        /// Returns an ordered list of cells forming the shortest path
        /// from start to end, traversing only Street and Sidewalk cells.
        /// Returns empty list if no path exists.
        /// </summary>
        public static List<CellData> FindPath(Grid_Layout grid, CellData start, CellData end)
        {
            if (grid == null)
                return new List<CellData>();

            // Early exit if start or end are not navigable
            if (!IsNavigable(start) || !IsNavigable(end))
                return new List<CellData>();

            // Early exit if start equals end
            if (start.Row == end.Row && start.Col == end.Col)
                return new List<CellData> { start };

            // BFS data structures using (row, col) tuples as keys
            var queue = new Queue<(int row, int col)>();
            var visited = new HashSet<(int row, int col)>();
            var parent = new Dictionary<(int row, int col), (int row, int col)>();

            var startKey = (start.Row, start.Col);
            var endKey = (end.Row, end.Col);

            queue.Enqueue(startKey);
            visited.Add(startKey);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (current == endKey)
                {
                    return ReconstructPath(grid, parent, startKey, endKey);
                }

                // Get orthogonal neighbors
                List<CellData> neighbors = grid.GetAdjacentCells(current.row, current.col);

                foreach (CellData neighbor in neighbors)
                {
                    var neighborKey = (neighbor.Row, neighbor.Col);

                    if (visited.Contains(neighborKey))
                        continue;

                    if (!IsNavigable(neighbor))
                        continue;

                    visited.Add(neighborKey);
                    parent[neighborKey] = current;
                    queue.Enqueue(neighborKey);
                }
            }

            // No path found
            return new List<CellData>();
        }

        private static bool IsNavigable(CellData cell)
        {
            return cell.Type == CellType.Street || cell.Type == CellType.Sidewalk;
        }

        private static List<CellData> ReconstructPath(
            Grid_Layout grid,
            Dictionary<(int row, int col), (int row, int col)> parent,
            (int row, int col) startKey,
            (int row, int col) endKey)
        {
            var path = new List<CellData>();
            var current = endKey;

            while (current != startKey)
            {
                path.Add(grid.GetCell(current.row, current.col));
                current = parent[current];
            }

            path.Add(grid.GetCell(startKey.row, startKey.col));
            path.Reverse();

            return path;
        }
    }
}
