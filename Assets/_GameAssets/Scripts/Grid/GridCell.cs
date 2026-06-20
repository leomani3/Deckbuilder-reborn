using System.Collections.Generic;
using UnityEngine;

namespace Deckbuilder.Grid
{
    public class GridCell : MonoBehaviour
    {
        [SerializeField] private bool isObstacle;
        [SerializeField] private GameObject obstacleVisual;

        public Vector2Int Coordinate => GridManager.PositionToCoordinate(transform.position);
        public GridEntity Occupant { get; private set; }
        public bool IsOccupied => Occupant != null;
        public bool IsObstacle => isObstacle;
        public bool IsWalkable => !IsObstacle && !IsOccupied;

        private readonly Dictionary<GridDirection, GridCell> neighbors = new();

        private void Awake()
        {
            UpdateObstacleVisual();
        }

        private void OnValidate()
        {
            UpdateObstacleVisual();
        }

        public void SetObstacle(bool value)
        {
            isObstacle = value;
            UpdateObstacleVisual();
        }

        private void UpdateObstacleVisual()
        {
            if (obstacleVisual != null)
                obstacleVisual.SetActive(isObstacle);
        }

        public void SetNeighbor(GridDirection direction, GridCell cell)
        {
            neighbors[direction] = cell;
        }

        public void ClearNeighbors()
        {
            neighbors.Clear();
        }

        public GridCell GetNeighbor(GridDirection direction)
        {
            return neighbors.TryGetValue(direction, out var cell) ? cell : null;
        }

        public IEnumerable<GridCell> GetNeighbors(bool includeDiagonals = true)
        {
            var directions = includeDiagonals ? GridDirectionUtility.All : GridDirectionUtility.Orthogonal;
            foreach (var direction in directions)
            {
                var cell = GetNeighbor(direction);
                if (cell != null)
                    yield return cell;
            }
        }

        public bool TrySetOccupant(GridEntity entity)
        {
            if (IsOccupied)
                return false;

            Occupant = entity;
            entity.SetCurrentCell(this);
            return true;
        }

        public void ClearOccupant()
        {
            Occupant = null;
        }
    }
}
