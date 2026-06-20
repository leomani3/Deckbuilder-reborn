using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Deckbuilder.Grid
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [SerializeField] private float cellSize = 1f;
        [SerializeField, ReadOnly] private SerializableDictionary<Vector2Int, GridCell> cellsByCoordinate = new();

        private void Awake()
        {
            Instance = this;
            BuildGrid();
        }

        public static Vector2Int PositionToCoordinate(Vector3 position)
        {
            float size = Instance != null ? Instance.cellSize : 1f;
            return new Vector2Int(
                Mathf.RoundToInt(position.x / size),
                Mathf.RoundToInt(position.z / size));
        }

        private void BuildGrid()
        {
            cellsByCoordinate.Clear();

            var cells = FindObjectsByType<GridCell>(FindObjectsSortMode.None);
            foreach (var cell in cells)
            {
                if (!cellsByCoordinate.TryAdd(cell.Coordinate, cell))
                    Debug.LogError($"Duplicate grid coordinate {cell.Coordinate} found on {cell.name}.", cell);
            }

            foreach (var cell in cells)
            {
                cell.ClearNeighbors();
                foreach (var direction in GridDirectionUtility.All)
                {
                    var neighborCoordinate = cell.Coordinate + GridDirectionUtility.GetOffset(direction);
                    if (cellsByCoordinate.TryGetValue(neighborCoordinate, out var neighbor))
                        cell.SetNeighbor(direction, neighbor);
                }
            }
        }

        public GridCell GetCell(Vector2Int coordinate)
        {
            return cellsByCoordinate.TryGetValue(coordinate, out var cell) ? cell : null;
        }

        public IEnumerable<GridCell> GetCellsInZone(Vector2Int origin, GridShape shape, int size)
        {
            foreach (var coordinate in GridShapeUtility.GetCoordinates(origin, shape, size))
            {
                if (cellsByCoordinate.TryGetValue(coordinate, out var cell))
                    yield return cell;
            }
        }

        public int GetManhattanDistance(GridCell a, GridCell b)
        {
            return GridShapeUtility.ManhattanDistance(a.Coordinate, b.Coordinate);
        }

        public List<GridCell> FindPath(GridCell start, GridCell goal, bool ignoreOccupants = false, bool randomize = false)
        {
            return GridPathfinder.FindPath(start, goal, GetCell, ignoreOccupants, randomize);
        }

        public bool HasLineOfSight(GridCell from, GridCell to, out GridCell blockingCell)
        {
            return GridLineOfSight.HasLineOfSight(from, to, GetCell, out blockingCell);
        }

        public bool MoveEntity(GridEntity entity, GridCell targetCell)
        {
            if (targetCell == null || targetCell.IsOccupied)
                return false;

            entity.CurrentCell?.ClearOccupant();
            return targetCell.TrySetOccupant(entity);
        }
    }
}
