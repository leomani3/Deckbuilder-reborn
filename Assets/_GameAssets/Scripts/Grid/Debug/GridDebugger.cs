using System.Collections.Generic;
using UnityEngine;

namespace Deckbuilder.Grid.Debugging
{
    public class GridDebugger : MonoBehaviour
    {
        [Header("Zone")]
        [SerializeField] private GridCell origin;
        [SerializeField] private GridShape shape = GridShape.Diamond;
        [SerializeField] private int size = 2;
        [SerializeField] private bool showZone = true;
        [SerializeField] private Color zoneColor = new(1f, 0.5f, 0f, 0.6f);
        [SerializeField] private Color originColor = Color.red;

        [Header("Neighbors")]
        [SerializeField] private bool showNeighborLinks;
        [SerializeField] private GridCell neighborReference;
        [SerializeField] private Color neighborLinkColor = Color.cyan;

        [Header("Pathfinding")]
        [SerializeField] private bool showPath;
        [SerializeField] private GridCell pathStart;
        [SerializeField] private GridCell pathEnd;
        [SerializeField] private bool pathIgnoreOccupants;
        [SerializeField] private Color pathColor = Color.green;
        [SerializeField] private Color noPathColor = Color.red;

        [Header("Line Of Sight")]
        [SerializeField] private bool showLineOfSight;
        [SerializeField] private GridCell losFrom;
        [SerializeField] private GridCell losTo;
        [SerializeField] private Color losClearColor = Color.green;
        [SerializeField] private Color losBlockedColor = Color.red;
        [SerializeField] private Color losBlockingCellColor = new(1f, 0f, 0f, 0.9f);

        [Header("Labels")]
        [SerializeField] private bool showCoordinateLabels;

        [Header("Gizmo Sizing")]
        [SerializeField] private float markerHeight = 0.05f;
        [SerializeField] private float markerRadius = 0.4f;

        private void OnDrawGizmos()
        {
            if (showZone)
                DrawZone();

            if (showNeighborLinks)
                DrawNeighborLinks();

            if (showPath)
                DrawPath();

            if (showLineOfSight)
                DrawLineOfSight();

            if (showCoordinateLabels)
                DrawCoordinateLabels();
        }

        private void DrawZone()
        {
            if (origin == null)
                return;

            var cellsByCoordinate = BuildCoordinateLookup();
            var originCoordinate = origin.Coordinate;

            Gizmos.color = zoneColor;
            foreach (var coordinate in GridShapeUtility.GetCoordinates(originCoordinate, shape, size))
            {
                if (coordinate == originCoordinate)
                    continue;

                if (cellsByCoordinate.TryGetValue(coordinate, out var cell))
                    DrawMarker(cell.transform.position);
            }

            Gizmos.color = originColor;
            DrawMarker(origin.transform.position);
        }

        private void DrawNeighborLinks()
        {
            var cell = neighborReference != null ? neighborReference : origin;
            if (cell == null)
                return;

            var cellsByCoordinate = BuildCoordinateLookup();
            var coordinate = cell.Coordinate;

            Gizmos.color = neighborLinkColor;
            var from = cell.transform.position + Vector3.up * markerHeight;
            foreach (var direction in GridDirectionUtility.All)
            {
                var neighborCoordinate = coordinate + GridDirectionUtility.GetOffset(direction);
                if (!cellsByCoordinate.TryGetValue(neighborCoordinate, out var neighbor))
                    continue;

                var to = neighbor.transform.position + Vector3.up * markerHeight;
                Gizmos.DrawLine(from, to);
                Gizmos.DrawSphere(to, markerRadius * 0.5f);
            }
        }

        private void DrawPath()
        {
            if (pathStart == null || pathEnd == null)
                return;

            var cellsByCoordinate = BuildCoordinateLookup();
            var path = GridPathfinder.FindPath(pathStart, pathEnd, coordinate => cellsByCoordinate.GetValueOrDefault(coordinate), pathIgnoreOccupants);

            if (path == null)
            {
                Gizmos.color = noPathColor;
                Gizmos.DrawLine(
                    pathStart.transform.position + Vector3.up * markerHeight,
                    pathEnd.transform.position + Vector3.up * markerHeight);
                return;
            }

            Gizmos.color = pathColor;
            for (int i = 0; i < path.Count; i++)
            {
                var position = path[i].transform.position + Vector3.up * markerHeight;
                Gizmos.DrawSphere(position, markerRadius * 0.6f);

                if (i > 0)
                {
                    var previousPosition = path[i - 1].transform.position + Vector3.up * markerHeight;
                    Gizmos.DrawLine(previousPosition, position);
                }
            }
        }

        private void DrawLineOfSight()
        {
            if (losFrom == null || losTo == null)
                return;

            var cellsByCoordinate = BuildCoordinateLookup();
            var hasLineOfSight = GridLineOfSight.HasLineOfSight(losFrom, losTo, coordinate => cellsByCoordinate.GetValueOrDefault(coordinate), out var blockingCell);

            var from = losFrom.transform.position + Vector3.up * markerHeight;
            var to = losTo.transform.position + Vector3.up * markerHeight;

            Gizmos.color = hasLineOfSight ? losClearColor : losBlockedColor;
            Gizmos.DrawLine(from, to);

            if (blockingCell != null)
            {
                Gizmos.color = losBlockingCellColor;
                Gizmos.DrawCube(blockingCell.transform.position + Vector3.up * markerHeight, Vector3.one * markerRadius);
            }
        }

        private void DrawCoordinateLabels()
        {
#if UNITY_EDITOR
            var cells = FindObjectsByType<GridCell>(FindObjectsSortMode.None);
            foreach (var cell in cells)
            {
                var position = cell.transform.position + Vector3.up * (markerHeight + 0.1f);
                UnityEditor.Handles.Label(position, cell.Coordinate.ToString());
            }
#endif
        }

        private void DrawMarker(Vector3 position)
        {
            Gizmos.DrawSphere(position + Vector3.up * markerHeight, markerRadius);
        }

        private static Dictionary<Vector2Int, GridCell> BuildCoordinateLookup()
        {
            var lookup = new Dictionary<Vector2Int, GridCell>();
            var cells = FindObjectsByType<GridCell>(FindObjectsSortMode.None);
            foreach (var cell in cells)
                lookup[cell.Coordinate] = cell;

            return lookup;
        }
    }
}
