using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deckbuilder.Grid
{
    public static class GridPathfinder
    {
        public static List<GridCell> FindPath(GridCell start, GridCell goal, Func<Vector2Int, GridCell> cellResolver, bool ignoreOccupants = false, bool randomize = false)
        {
            if (start == null || goal == null)
                return null;

            var startCoordinate = start.Coordinate;
            var goalCoordinate = goal.Coordinate;

            var openSet = new List<Vector2Int> { startCoordinate };
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var gScore = new Dictionary<Vector2Int, int> { [startCoordinate] = 0 };
            var fScore = new Dictionary<Vector2Int, int> { [startCoordinate] = ManhattanDistance(startCoordinate, goalCoordinate) };

            while (openSet.Count > 0)
            {
                var current = GetLowestFScore(openSet, fScore, randomize);

                if (current == goalCoordinate)
                    return ReconstructPath(cameFrom, current, cellResolver);

                openSet.Remove(current);

                foreach (var direction in GetExplorationOrder(randomize))
                {
                    var neighborCoordinate = current + GridDirectionUtility.GetOffset(direction);
                    var neighborCell = cellResolver(neighborCoordinate);

                    if (neighborCell == null)
                        continue;

                    if (!IsWalkable(neighborCell, ignoreOccupants))
                        continue;

                    var tentativeGScore = gScore[current] + 1;
                    if (gScore.TryGetValue(neighborCoordinate, out var existingGScore) && tentativeGScore >= existingGScore)
                        continue;

                    cameFrom[neighborCoordinate] = current;
                    gScore[neighborCoordinate] = tentativeGScore;
                    fScore[neighborCoordinate] = tentativeGScore + ManhattanDistance(neighborCoordinate, goalCoordinate);

                    if (!openSet.Contains(neighborCoordinate))
                        openSet.Add(neighborCoordinate);
                }
            }

            return null;
        }

        private static bool IsWalkable(GridCell cell, bool ignoreOccupants)
        {
            if (cell.IsObstacle)
                return false;

            return ignoreOccupants || !cell.IsOccupied;
        }

        private static GridDirection[] GetExplorationOrder(bool randomize)
        {
            if (!randomize)
                return GridDirectionUtility.Orthogonal;

            var shuffled = (GridDirection[])GridDirectionUtility.Orthogonal.Clone();
            for (int i = shuffled.Length - 1; i > 0; i--)
            {
                var swapIndex = UnityEngine.Random.Range(0, i + 1);
                (shuffled[i], shuffled[swapIndex]) = (shuffled[swapIndex], shuffled[i]);
            }

            return shuffled;
        }

        private static Vector2Int GetLowestFScore(List<Vector2Int> openSet, Dictionary<Vector2Int, int> fScore, bool randomize)
        {
            var lowestScore = fScore[openSet[0]];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (fScore.TryGetValue(openSet[i], out var score) && score < lowestScore)
                    lowestScore = score;
            }

            if (!randomize)
            {
                foreach (var coordinate in openSet)
                {
                    if (fScore.TryGetValue(coordinate, out var score) && score == lowestScore)
                        return coordinate;
                }
            }

            var candidates = new List<Vector2Int>();
            foreach (var coordinate in openSet)
            {
                if (fScore.TryGetValue(coordinate, out var score) && score == lowestScore)
                    candidates.Add(coordinate);
            }

            return candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }

        private static List<GridCell> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current, Func<Vector2Int, GridCell> cellResolver)
        {
            var path = new List<GridCell> { cellResolver(current) };
            while (cameFrom.TryGetValue(current, out var previous))
            {
                current = previous;
                path.Add(cellResolver(current));
            }

            path.Reverse();
            return path;
        }

        private static int ManhattanDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }
    }
}
