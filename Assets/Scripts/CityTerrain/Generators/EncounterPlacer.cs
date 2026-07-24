using System.Collections.Generic;

namespace Flipit.CityTerrain
{
    /// <summary>
    /// Places invisible trigger zones on navigable path cells.
    /// Encounters are placed with minimum spacing constraints and
    /// minimum distance from landmark cells.
    /// </summary>
    public static class EncounterPlacer
    {
        public struct EncounterPlacementResult
        {
            public List<CellData> PlacedCells;
            public int RequestedCount;
            public int ActualCount;
        }

        /// <summary>
        /// Places encounters on path cells with:
        /// - Minimum 3-cell Manhattan distance spacing between encounters
        /// - Minimum 5-cell Manhattan distance from landmarks (school and house)
        /// - Not on occupied cells
        /// </summary>
        /// <param name="grid">The city grid layout.</param>
        /// <param name="rng">Seeded random number generator for deterministic placement.</param>
        /// <param name="count">Number of encounters to place.</param>
        /// <param name="path">The navigable path between school and house.</param>
        /// <param name="schoolCell">The school landmark cell.</param>
        /// <param name="houseCell">The house landmark cell.</param>
        /// <returns>Result containing placed cells and count information.</returns>
        public static EncounterPlacementResult PlaceEncounters(
            Grid_Layout grid, System.Random rng,
            int count, List<CellData> path,
            CellData schoolCell, CellData houseCell)
        {
            var result = new EncounterPlacementResult
            {
                PlacedCells = new List<CellData>(),
                RequestedCount = count,
                ActualCount = 0
            };

            if (grid == null || path == null || path.Count == 0 || count <= 0)
            {
                return result;
            }

            // Step 1: Filter path cells to find eligible candidates
            var eligible = FilterEligibleCells(grid, path, schoolCell, houseCell);

            if (eligible.Count == 0)
            {
                return result;
            }

            // Step 2: Shuffle eligible cells for random selection
            Shuffle(eligible, rng);

            // Step 3: Place encounters with minimum 3-cell spacing between each other
            var placed = new List<CellData>();

            foreach (var cell in eligible)
            {
                if (placed.Count >= count)
                    break;

                if (!HasMinimumSpacing(cell, placed, minSpacing: 3))
                    continue;

                // Mark the cell as occupied with "encounter" tag
                grid.MarkOccupied(cell.Row, cell.Col, "encounter");
                placed.Add(grid.GetCell(cell.Row, cell.Col));
            }

            result.PlacedCells = placed;
            result.ActualCount = placed.Count;

            return result;
        }

        /// <summary>
        /// Filters path cells to only include those that:
        /// - Are Street or Sidewalk type (on the navigable path)
        /// - Are not occupied
        /// - Are at least 5 Manhattan distance from school and house landmarks
        /// </summary>
        private static List<CellData> FilterEligibleCells(
            Grid_Layout grid, List<CellData> path,
            CellData schoolCell, CellData houseCell)
        {
            var eligible = new List<CellData>();

            foreach (var cell in path)
            {
                // Must be a navigable cell type (Street or Sidewalk)
                if (cell.Type != CellType.Street && cell.Type != CellType.Sidewalk)
                    continue;

                // Must not be occupied
                CellData current = grid.GetCell(cell.Row, cell.Col);
                if (current.IsOccupied)
                    continue;

                // Must be at least 5 Manhattan distance from school
                if (current.ManhattanDistance(schoolCell) < 5)
                    continue;

                // Must be at least 5 Manhattan distance from house
                if (current.ManhattanDistance(houseCell) < 5)
                    continue;

                eligible.Add(current);
            }

            return eligible;
        }

        /// <summary>
        /// Checks if a candidate cell has at least the minimum Manhattan distance
        /// from all already-placed encounter cells.
        /// </summary>
        private static bool HasMinimumSpacing(CellData candidate, List<CellData> placed, int minSpacing)
        {
            foreach (var existing in placed)
            {
                if (candidate.ManhattanDistance(existing) < minSpacing)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Fisher-Yates shuffle for random ordering.
        /// </summary>
        private static void Shuffle(List<CellData> list, System.Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                CellData temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}
