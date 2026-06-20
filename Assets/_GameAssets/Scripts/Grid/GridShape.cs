using System.Collections.Generic;
using UnityEngine;

namespace Deckbuilder.Grid
{
    public enum GridShape
    {
        Single,
        Cross,
        Diamond,
        Square
    }

    public static class GridShapeUtility
    {
        public static IEnumerable<Vector2Int> GetCoordinates(Vector2Int origin, GridShape shape, int size)
        {
            foreach (var offset in GetOffsets(shape, size))
                yield return origin + offset;
        }

        public static IEnumerable<Vector2Int> GetOffsets(GridShape shape, int size)
        {
            switch (shape)
            {
                case GridShape.Single:
                    yield return Vector2Int.zero;
                    break;

                case GridShape.Cross:
                    foreach (var offset in GetCross(size))
                        yield return offset;
                    break;

                case GridShape.Diamond:
                    foreach (var offset in GetDiamond(size))
                        yield return offset;
                    break;

                case GridShape.Square:
                    foreach (var offset in GetSquare(size))
                        yield return offset;
                    break;
            }
        }

        public static int ManhattanDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        private static IEnumerable<Vector2Int> GetCross(int size)
        {
            for (int offset = -size; offset <= size; offset++)
            {
                yield return new Vector2Int(offset, 0);
                yield return new Vector2Int(0, offset);
            }
        }

        private static IEnumerable<Vector2Int> GetDiamond(int size)
        {
            for (int x = -size; x <= size; x++)
            {
                int remaining = size - Mathf.Abs(x);
                for (int y = -remaining; y <= remaining; y++)
                    yield return new Vector2Int(x, y);
            }
        }

        private static IEnumerable<Vector2Int> GetSquare(int size)
        {
            for (int x = -size; x <= size; x++)
                for (int y = -size; y <= size; y++)
                    yield return new Vector2Int(x, y);
        }
    }
}
