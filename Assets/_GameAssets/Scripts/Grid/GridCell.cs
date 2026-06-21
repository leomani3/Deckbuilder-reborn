using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Deckbuilder.Grid
{
    public class GridCell : MonoBehaviour
    {
        [FormerlySerializedAs("isObstacle")] [SerializeField] private bool m_isObstacle;
        [FormerlySerializedAs("obstacleVisual")] [SerializeField] private GameObject m_obstacleVisual;

        public Vector2Int Coordinate => GridManager.PositionToCoordinate(transform.position);
        public Entity Occupant { get; private set; }
        public bool IsOccupied => Occupant != null;
        public bool IsObstacle => m_isObstacle;
        public bool IsWalkable => !IsObstacle && !IsOccupied;

        private readonly Dictionary<GridDirection, GridCell> m_neighbors = new();

        private void Awake()
        {
            UpdateObstacleVisual();
        }

        private void OnValidate()
        {
            UpdateObstacleVisual();
        }

        public void SetObstacle(bool _value)
        {
            m_isObstacle = _value;
            UpdateObstacleVisual();
        }

        private void UpdateObstacleVisual()
        {
            if (m_obstacleVisual != null)
                m_obstacleVisual.SetActive(m_isObstacle);
        }

        public void SetNeighbor(GridDirection _direction, GridCell _cell)
        {
            m_neighbors[_direction] = _cell;
        }

        public void ClearNeighbors()
        {
            m_neighbors.Clear();
        }

        public GridCell GetNeighbor(GridDirection _direction)
        {
            return m_neighbors.TryGetValue(_direction, out GridCell _cell) ? _cell : null;
        }

        public IEnumerable<GridCell> GetNeighbors(bool _includeDiagonals = true)
        {
            GridDirection[] _directions = _includeDiagonals ? GridDirectionUtility.All : GridDirectionUtility.Orthogonal;
            foreach (GridDirection _direction in _directions)
            {
                GridCell _cell = GetNeighbor(_direction);
                if (_cell != null)
                    yield return _cell;
            }
        }

        public bool TrySetOccupant(Entity _entity)
        {
            if (IsOccupied)
                return false;

            Occupant = _entity;
            _entity.SetCurrentCell(this);
            return true;
        }

        public void ClearOccupant()
        {
            Occupant = null;
        }
    }
}
