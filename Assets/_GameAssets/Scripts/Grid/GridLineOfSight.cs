using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deckbuilder.Grid
{
    public static class GridLineOfSight
    {
        public static bool HasLineOfSight(GridCell from, GridCell to, Func<Vector2Int, GridCell> cellResolver, out GridCell blockingCell)
        {
            blockingCell = null;

            if (from == null || to == null)
                return false;

            foreach (var coordinate in GetLineCoordinates(from.Coordinate, to.Coordinate))
            {
                if (coordinate == from.Coordinate || coordinate == to.Coordinate)
                    continue;

                var cell = cellResolver(coordinate);
                if (cell != null && (cell.IsObstacle || cell.IsOccupied))
                {
                    blockingCell = cell;
                    return false;
                }
            }

            return true;
        }

        public static IEnumerable<Vector2Int> GetLineCoordinates(Vector2Int from, Vector2Int to)
        {
            int x0 = from.x, y0 = from.y;
            int x1 = to.x, y1 = to.y;

            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int signX = x1 > x0 ? 1 : -1;
            int signY = y1 > y0 ? 1 : -1;

            int x = x0, y = y0;
            int error = dx - dy;

            while (true)
            {
                yield return new Vector2Int(x, y);

                if (x == x1 && y == y1)
                    break;

                int doubledError = error * 2;
                if (doubledError > -dy)
                {
                    error -= dy;
                    x += signX;
                }

                if (doubledError < dx)
                {
                    error += dx;
                    y += signY;
                }
            }
        }
    }
}
