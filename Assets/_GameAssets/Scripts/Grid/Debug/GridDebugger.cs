using System.Collections.Generic;
using UnityEngine;

namespace Deckbuilder.Grid.Debugging
{
    public class GridDebugger : MonoBehaviour
    {
        [Header("Zone")]
        [SerializeField] private GridCell m_origin;
        [SerializeField] private GridShape m_shape = GridShape.Diamond;
        [SerializeField] private int m_size = 2;
        [SerializeField] private bool m_showZone = true;
        [SerializeField] private Color m_zoneColor = new(1f, 0.5f, 0f, 0.6f);
        [SerializeField] private Color m_originColor = Color.red;

        [Header("Neighbors")]
        [SerializeField] private bool m_showNeighborLinks;
        [SerializeField] private GridCell m_neighborReference;
        [SerializeField] private Color m_neighborLinkColor = Color.cyan;

        [Header("Pathfinding")]
        [SerializeField] private bool m_showPath;
        [SerializeField] private GridCell m_pathStart;
        [SerializeField] private GridCell m_pathEnd;
        [SerializeField] private bool m_pathIgnoreOccupants;
        [SerializeField] private Color m_pathColor = Color.green;
        [SerializeField] private Color m_noPathColor = Color.red;

        [Header("Line Of Sight")]
        [SerializeField] private bool m_showLineOfSight;
        [SerializeField] private GridCell m_losFrom;
        [SerializeField] private GridCell m_losTo;
        [SerializeField] private Color m_losClearColor = Color.green;
        [SerializeField] private Color m_losBlockedColor = Color.red;
        [SerializeField] private Color m_losBlockingCellColor = new(1f, 0f, 0f, 0.9f);

        [Header("Labels")]
        [SerializeField] private bool m_showCoordinateLabels;

        [Header("Gizmo Sizing")]
        [SerializeField] private float m_markerHeight = 0.05f;
        [SerializeField] private float m_markerRadius = 0.4f;

        private void OnDrawGizmos()
        {
            if (m_showZone)
                DrawZone();

            if (m_showNeighborLinks)
                DrawNeighborLinks();

            if (m_showPath)
                DrawPath();

            if (m_showLineOfSight)
                DrawLineOfSight();

            if (m_showCoordinateLabels)
                DrawCoordinateLabels();
        }

        private void DrawZone()
        {
            if (m_origin == null)
                return;

            Dictionary<Vector2Int, GridCell> _cellsByCoordinate = BuildCoordinateLookup();
            Vector2Int _originCoordinate = m_origin.Coordinate;

            Gizmos.color = m_zoneColor;
            foreach (Vector2Int _coordinate in GridShapeUtility.GetCoordinates(_originCoordinate, m_shape, m_size))
            {
                if (_coordinate == _originCoordinate)
                    continue;

                if (_cellsByCoordinate.TryGetValue(_coordinate, out GridCell _cell))
                    DrawMarker(_cell.transform.position);
            }

            Gizmos.color = m_originColor;
            DrawMarker(m_origin.transform.position);
        }

        private void DrawNeighborLinks()
        {
            GridCell _cell = m_neighborReference != null ? m_neighborReference : m_origin;
            if (_cell == null)
                return;

            Dictionary<Vector2Int, GridCell> _cellsByCoordinate = BuildCoordinateLookup();
            Vector2Int _coordinate = _cell.Coordinate;

            Gizmos.color = m_neighborLinkColor;
            Vector3 _from = _cell.transform.position + Vector3.up * m_markerHeight;
            foreach (GridDirection _direction in GridDirectionUtility.All)
            {
                Vector2Int _neighborCoordinate = _coordinate + GridDirectionUtility.GetOffset(_direction);
                if (!_cellsByCoordinate.TryGetValue(_neighborCoordinate, out GridCell _neighbor))
                    continue;

                Vector3 _to = _neighbor.transform.position + Vector3.up * m_markerHeight;
                Gizmos.DrawLine(_from, _to);
                Gizmos.DrawSphere(_to, m_markerRadius * 0.5f);
            }
        }

        private void DrawPath()
        {
            if (m_pathStart == null || m_pathEnd == null)
                return;

            Dictionary<Vector2Int, GridCell> _cellsByCoordinate = BuildCoordinateLookup();
            List<GridCell> _path = GridPathfinder.FindPath(m_pathStart, m_pathEnd, _coordinate => _cellsByCoordinate.GetValueOrDefault(_coordinate), m_pathIgnoreOccupants);

            if (_path == null)
            {
                Gizmos.color = m_noPathColor;
                Gizmos.DrawLine(
                    m_pathStart.transform.position + Vector3.up * m_markerHeight,
                    m_pathEnd.transform.position + Vector3.up * m_markerHeight);
                return;
            }

            Gizmos.color = m_pathColor;
            for (int _i = 0; _i < _path.Count; _i++)
            {
                Vector3 _position = _path[_i].transform.position + Vector3.up * m_markerHeight;
                Gizmos.DrawSphere(_position, m_markerRadius * 0.6f);

                if (_i > 0)
                {
                    Vector3 _previousPosition = _path[_i - 1].transform.position + Vector3.up * m_markerHeight;
                    Gizmos.DrawLine(_previousPosition, _position);
                }
            }
        }

        private void DrawLineOfSight()
        {
            if (m_losFrom == null || m_losTo == null)
                return;

            Dictionary<Vector2Int, GridCell> _cellsByCoordinate = BuildCoordinateLookup();
            bool _hasLineOfSight = GridLineOfSight.HasLineOfSight(m_losFrom, m_losTo, _coordinate => _cellsByCoordinate.GetValueOrDefault(_coordinate), out GridCell _blockingCell);

            Vector3 _from = m_losFrom.transform.position + Vector3.up * m_markerHeight;
            Vector3 _to = m_losTo.transform.position + Vector3.up * m_markerHeight;

            Gizmos.color = _hasLineOfSight ? m_losClearColor : m_losBlockedColor;
            Gizmos.DrawLine(_from, _to);

            if (_blockingCell != null)
            {
                Gizmos.color = m_losBlockingCellColor;
                Gizmos.DrawCube(_blockingCell.transform.position + Vector3.up * m_markerHeight, Vector3.one * m_markerRadius);
            }
        }

        private void DrawCoordinateLabels()
        {
#if UNITY_EDITOR
            GridCell[] _cells = FindObjectsByType<GridCell>(FindObjectsSortMode.None);
            foreach (GridCell _cell in _cells)
            {
                Vector3 _position = _cell.transform.position + Vector3.up * (m_markerHeight + 0.1f);
                UnityEditor.Handles.Label(_position, _cell.Coordinate.ToString());
            }
#endif
        }

        private void DrawMarker(Vector3 _position)
        {
            Gizmos.DrawSphere(_position + Vector3.up * m_markerHeight, m_markerRadius);
        }

        private static Dictionary<Vector2Int, GridCell> BuildCoordinateLookup()
        {
            Dictionary<Vector2Int, GridCell> _lookup = new Dictionary<Vector2Int, GridCell>();
            GridCell[] _cells = FindObjectsByType<GridCell>(FindObjectsSortMode.None);
            foreach (GridCell _cell in _cells)
                _lookup[_cell.Coordinate] = _cell;

            return _lookup;
        }
    }
}
