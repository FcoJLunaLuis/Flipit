using System.Collections.Generic;
using UnityEngine;

namespace Flipit.CityTerrain
{
    /// <summary>
    /// Places School and House landmarks on opposite edges of the grid.
    /// Pure algorithm operating on Grid_Layout.
    /// </summary>
    public static class LandmarkPlacer
    {
        public struct LandmarkResult
        {
            public CellData SchoolCell;
            public CellData HouseCell;
            public CellData SchoolSpawnCell;   // adjacent sidewalk
            public CellData HouseTriggerCell;  // adjacent sidewalk
            public bool Success;
            public string ErrorMessage;
        }

        /// <summary>
        /// Finds valid positions for School and House landmarks.
        /// School on one edge, House on opposite edge.
        /// Minimum distance: 60% of grid diagonal.
        /// Both must be Building_Cells adjacent to a Sidewalk_Cell.
        /// </summary>
        public static LandmarkResult PlaceLandmarks(Grid_Layout grid)
        {
            List<CellData> edgeBuildingCells = grid.GetEdgeBuildingCells();

            if (edgeBuildingCells.Count == 0)
            {
                return new LandmarkResult
                {
                    Success = false,
                    ErrorMessage = "No Building_Cells found on grid edges adjacent to Sidewalk_Cells."
                };
            }

            // Group edge cells by edge
            var topCells = new List<CellData>();
            var bottomCells = new List<CellData>();
            var leftCells = new List<CellData>();
            var rightCells = new List<CellData>();

            foreach (var cell in edgeBuildingCells)
            {
                if (cell.Row == 0)
                    topCells.Add(cell);
                else if (cell.Row == grid.Depth - 1)
                    bottomCells.Add(cell);

                if (cell.Col == 0)
                    leftCells.Add(cell);
                else if (cell.Col == grid.Width - 1)
                    rightCells.Add(cell);
            }

            // Calculate grid diagonal distance
            float gridWorldWidth = grid.Width * grid.CellSize;
            float gridWorldDepth = grid.Depth * grid.CellSize;
            float diagonal = Mathf.Sqrt(gridWorldWidth * gridWorldWidth + gridWorldDepth * gridWorldDepth);
            float minDistance = 0.3f * diagonal;

            // Define edge pairs to check — include all combinations since
            // edges at row/col 0 are often all-street (0 % interval == 0)
            var edgePairs = new (List<CellData> edgeA, List<CellData> edgeB)[]
            {
                (topCells, bottomCells),
                (leftCells, rightCells),
                (bottomCells, topCells),
                (rightCells, leftCells),
                // Cross-edge pairs for when one axis has no buildings
                (bottomCells, rightCells),
                (bottomCells, leftCells),
                (topCells, rightCells),
                (topCells, leftCells),
                (leftCells, bottomCells),
                (rightCells, bottomCells)
            };

            // Search for a valid pair
            foreach (var (edgeA, edgeB) in edgePairs)
            {
                foreach (var schoolCandidate in edgeA)
                {
                    foreach (var houseCandidate in edgeB)
                    {
                        float distance = Vector3.Distance(
                            schoolCandidate.WorldPosition(grid.CellSize),
                            houseCandidate.WorldPosition(grid.CellSize));

                        if (distance >= minDistance)
                        {
                            // Find adjacent sidewalk for school
                            CellData? schoolSidewalk = FindAdjacentSidewalk(grid, schoolCandidate);
                            if (!schoolSidewalk.HasValue)
                                continue;

                            // Find adjacent sidewalk for house
                            CellData? houseSidewalk = FindAdjacentSidewalk(grid, houseCandidate);
                            if (!houseSidewalk.HasValue)
                                continue;

                            return new LandmarkResult
                            {
                                SchoolCell = schoolCandidate,
                                HouseCell = houseCandidate,
                                SchoolSpawnCell = schoolSidewalk.Value,
                                HouseTriggerCell = houseSidewalk.Value,
                                Success = true,
                                ErrorMessage = null
                            };
                        }
                    }
                }
            }

            return new LandmarkResult
            {
                Success = false,
                ErrorMessage = "No valid landmark pair found satisfying opposite-edge placement " +
                               $"with minimum distance of 60% diagonal ({minDistance:F2} units)."
            };
        }

        /// <summary>
        /// Finds the first adjacent Sidewalk_Cell for a given cell.
        /// </summary>
        private static CellData? FindAdjacentSidewalk(Grid_Layout grid, CellData cell)
        {
            List<CellData> neighbors = grid.GetAdjacentCells(cell.Row, cell.Col);
            foreach (var neighbor in neighbors)
            {
                if (neighbor.Type == CellType.Sidewalk)
                {
                    return neighbor;
                }
            }
            return null;
        }
    }
}
