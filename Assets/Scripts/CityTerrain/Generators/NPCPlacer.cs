using System.Collections.Generic;
using UnityEngine;

namespace Flipit.CityTerrain
{
    /// <summary>
    /// Places Vendor and Challenger NPCs on valid sidewalk cells with clearance rules.
    /// </summary>
    public static class NPCPlacer
    {
        public struct NPCPlacementResult
        {
            public List<CellData> PlacedCells;
            public int RequestedCount;
            public int ActualCount;
            public string NPCType;
        }

        /// <summary>
        /// Places vendors on sidewalk cells adjacent to buildings.
        /// Max 1 per city block. Minimum 2-cell clearance from other NPCs.
        /// A block is defined by integer division of (row / streetInterval, col / streetInterval).
        /// </summary>
        public static NPCPlacementResult PlaceVendors(Grid_Layout grid, System.Random rng, int count, int streetInterval)
        {
            var result = new NPCPlacementResult
            {
                PlacedCells = new List<CellData>(),
                RequestedCount = count,
                ActualCount = 0,
                NPCType = "vendor"
            };

            if (count <= 0 || streetInterval < 3)
                return result;

            const int minClearance = 2;

            // Collect all existing NPC positions (cells already occupied by NPCs)
            var existingNPCs = GetExistingNPCCells(grid);

            // Find eligible cells: unoccupied Sidewalk cells adjacent to at least one Building cell
            var eligibleByBlock = new Dictionary<(int, int), List<CellData>>();

            for (int row = 0; row < grid.Depth; row++)
            {
                for (int col = 0; col < grid.Width; col++)
                {
                    CellData cell = grid.Cells[row, col];
                    if (cell.Type != CellType.Sidewalk || cell.IsOccupied)
                        continue;

                    if (!HasAdjacentBuilding(grid, row, col))
                        continue;

                    // Determine which block this cell belongs to
                    var blockKey = (row / streetInterval, col / streetInterval);

                    if (!eligibleByBlock.ContainsKey(blockKey))
                        eligibleByBlock[blockKey] = new List<CellData>();

                    eligibleByBlock[blockKey].Add(cell);
                }
            }

            // Get list of blocks and shuffle them
            var blockKeys = new List<(int, int)>(eligibleByBlock.Keys);
            Shuffle(blockKeys, rng);

            // Place one vendor per block until count is reached
            foreach (var blockKey in blockKeys)
            {
                if (result.ActualCount >= count)
                    break;

                var candidates = eligibleByBlock[blockKey];
                Shuffle(candidates, rng);

                bool placed = false;
                foreach (var candidate in candidates)
                {
                    if (HasClearanceFromAll(candidate, existingNPCs, result.PlacedCells, minClearance))
                    {
                        // Place vendor here
                        grid.MarkOccupied(candidate.Row, candidate.Col, "vendor");
                        result.PlacedCells.Add(grid.Cells[candidate.Row, candidate.Col]);
                        result.ActualCount++;
                        placed = true;
                        break;
                    }
                }

                if (!placed)
                {
                    // Could not place in this block, try next
                    continue;
                }
            }

            if (result.ActualCount < count)
            {
                Debug.LogWarning($"[NPCPlacer] Could only place {result.ActualCount}/{count} vendors. " +
                    $"Insufficient valid blocks or clearance constraints.");
            }

            return result;
        }

        /// <summary>
        /// Places challengers evenly along the path from school to house.
        /// Minimum 3-cell clearance from other NPCs.
        /// Uses the NPC Placement Along Path Algorithm from the design.
        /// </summary>
        public static NPCPlacementResult PlaceChallengers(Grid_Layout grid, System.Random rng, int count, List<CellData> path)
        {
            var result = new NPCPlacementResult
            {
                PlacedCells = new List<CellData>(),
                RequestedCount = count,
                ActualCount = 0,
                NPCType = "challenger"
            };

            if (count <= 0 || path == null || path.Count == 0)
                return result;

            const int minClearance = 3;

            // Collect all existing NPC positions
            var existingNPCs = GetExistingNPCCells(grid);

            // Calculate segment length and target indices
            int segmentLength = path.Count / (count + 1);
            if (segmentLength <= 0)
                segmentLength = 1;

            var targetIndices = new List<int>();
            for (int i = 1; i <= count; i++)
            {
                int idx = i * segmentLength;
                if (idx >= path.Count)
                    idx = path.Count - 1;
                targetIndices.Add(idx);
            }

            // For each target index, find nearest unoccupied Sidewalk cell with proper clearance
            foreach (int targetIdx in targetIndices)
            {
                CellData targetCell = path[targetIdx];
                CellData? foundCell = FindNearestValidSidewalk(grid, targetCell, existingNPCs, result.PlacedCells, minClearance);

                if (foundCell.HasValue)
                {
                    CellData cell = foundCell.Value;
                    grid.MarkOccupied(cell.Row, cell.Col, "challenger");
                    result.PlacedCells.Add(grid.Cells[cell.Row, cell.Col]);
                    result.ActualCount++;
                }
                else
                {
                    Debug.LogWarning($"[NPCPlacer] Could not place challenger near path index {targetIdx} " +
                        $"(row={targetCell.Row}, col={targetCell.Col}). No valid sidewalk with clearance found.");
                }
            }

            if (result.ActualCount < count)
            {
                Debug.LogWarning($"[NPCPlacer] Could only place {result.ActualCount}/{count} challengers.");
            }

            return result;
        }

        /// <summary>
        /// Finds the nearest unoccupied Sidewalk cell to the given target cell
        /// that has the required Manhattan distance clearance from all existing and placed NPCs.
        /// Searches outward using BFS from the target cell position.
        /// </summary>
        private static CellData? FindNearestValidSidewalk(
            Grid_Layout grid, CellData target,
            List<CellData> existingNPCs, List<CellData> placedNPCs,
            int minClearance)
        {
            // BFS outward from target to find nearest valid sidewalk
            var visited = new HashSet<(int, int)>();
            var queue = new Queue<(int row, int col)>();

            queue.Enqueue((target.Row, target.Col));
            visited.Add((target.Row, target.Col));

            while (queue.Count > 0)
            {
                var (row, col) = queue.Dequeue();
                CellData cell = grid.Cells[row, col];

                // Check if this is a valid placement cell
                if (cell.Type == CellType.Sidewalk && !cell.IsOccupied)
                {
                    if (HasClearanceFromAll(cell, existingNPCs, placedNPCs, minClearance))
                    {
                        return cell;
                    }
                }

                // Expand to orthogonal neighbors
                int[] dRows = { -1, 1, 0, 0 };
                int[] dCols = { 0, 0, -1, 1 };

                for (int i = 0; i < 4; i++)
                {
                    int nRow = row + dRows[i];
                    int nCol = col + dCols[i];

                    if (nRow >= 0 && nRow < grid.Depth && nCol >= 0 && nCol < grid.Width)
                    {
                        if (!visited.Contains((nRow, nCol)))
                        {
                            visited.Add((nRow, nCol));
                            queue.Enqueue((nRow, nCol));
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if a candidate cell has at least minClearance Manhattan distance
        /// from all cells in both the existingNPCs and placedNPCs lists.
        /// </summary>
        private static bool HasClearanceFromAll(CellData candidate, List<CellData> existingNPCs, List<CellData> placedNPCs, int minClearance)
        {
            foreach (var npc in existingNPCs)
            {
                if (candidate.ManhattanDistance(npc) < minClearance)
                    return false;
            }

            foreach (var npc in placedNPCs)
            {
                if (candidate.ManhattanDistance(npc) < minClearance)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if the cell at (row, col) has at least one adjacent Building cell.
        /// </summary>
        private static bool HasAdjacentBuilding(Grid_Layout grid, int row, int col)
        {
            // Up
            if (row > 0 && grid.Cells[row - 1, col].Type == CellType.Building)
                return true;
            // Down
            if (row < grid.Depth - 1 && grid.Cells[row + 1, col].Type == CellType.Building)
                return true;
            // Left
            if (col > 0 && grid.Cells[row, col - 1].Type == CellType.Building)
                return true;
            // Right
            if (col < grid.Width - 1 && grid.Cells[row, col + 1].Type == CellType.Building)
                return true;

            return false;
        }

        /// <summary>
        /// Collects all cells currently occupied by NPCs (vendor, challenger, or other NPC tags).
        /// </summary>
        private static List<CellData> GetExistingNPCCells(Grid_Layout grid)
        {
            var npcs = new List<CellData>();
            for (int row = 0; row < grid.Depth; row++)
            {
                for (int col = 0; col < grid.Width; col++)
                {
                    CellData cell = grid.Cells[row, col];
                    if (cell.IsOccupied && IsNPCTag(cell.OccupantTag))
                    {
                        npcs.Add(cell);
                    }
                }
            }
            return npcs;
        }

        /// <summary>
        /// Returns true if the occupant tag represents an NPC type.
        /// </summary>
        private static bool IsNPCTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                return false;

            return tag == "vendor" || tag == "challenger" || tag == "npc";
        }

        /// <summary>
        /// Fisher-Yates shuffle using the provided random instance.
        /// </summary>
        private static void Shuffle<T>(List<T> list, System.Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}
