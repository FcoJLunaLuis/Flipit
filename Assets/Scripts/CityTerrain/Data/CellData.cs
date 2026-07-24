using UnityEngine;

namespace Flipit.CityTerrain
{
    public struct CellData
    {
        public int Row { get; }
        public int Col { get; }
        public CellType Type { get; set; }
        public bool IsOccupied { get; set; }
        public bool IsIntersection { get; }
        public string OccupantTag { get; set; }

        public CellData(int row, int col, CellType type, bool isIntersection)
        {
            Row = row;
            Col = col;
            Type = type;
            IsOccupied = false;
            IsIntersection = isIntersection;
            OccupantTag = null;
        }

        /// <summary>
        /// Computes the world position of this cell.
        /// x = col * cellSize, z = row * cellSize.
        /// y = 0 for Street, 0.05 for Sidewalk, 0 otherwise.
        /// </summary>
        public Vector3 WorldPosition(float cellSize)
        {
            float x = Col * cellSize;
            float z = Row * cellSize;
            float y = Type == CellType.Sidewalk ? 0.05f : 0f;
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Computes the Manhattan distance between this cell and another.
        /// </summary>
        public int ManhattanDistance(CellData other)
        {
            return Mathf.Abs(Row - other.Row) + Mathf.Abs(Col - other.Col);
        }
    }
}
