using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Deckbuilder.Grid
{
    public class GridEntity : MonoBehaviour
    {
        [SerializeField] private GridCell startingCell;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float rotationSpeed = 10f;

        public GridCell CurrentCell { get; private set; }
        public GridCell DestinationCell { get; private set; }
        public bool IsMoving { get; private set; }

        private Coroutine moveRoutine;
        private bool cancelRequested;

        private void Start()
        {
            if (startingCell != null && CurrentCell == null)
                startingCell.TrySetOccupant(this);
        }

        public void SetCurrentCell(GridCell cell)
        {
            CurrentCell = cell;
        }

        public bool MoveTo(GridCell destination, bool ignoreOccupants = false, bool randomizePath = false)
        {
            if (IsMoving || destination == null || GridManager.Instance == null || CurrentCell == null)
                return false;

            var path = GridManager.Instance.FindPath(CurrentCell, destination, ignoreOccupants, randomizePath);
            if (path == null || path.Count < 2)
                return false;

            DestinationCell = destination;
            cancelRequested = false;
            moveRoutine = StartCoroutine(FollowPath(path));
            return true;
        }

        public void CancelMove()
        {
            if (IsMoving)
                cancelRequested = true;
        }

        private IEnumerator FollowPath(List<GridCell> path)
        {
            IsMoving = true;

            for (int i = 1; i < path.Count; i++)
            {
                var targetCell = path[i];
                yield return MoveToCell(targetCell);
                GridManager.Instance.MoveEntity(this, targetCell);

                if (cancelRequested)
                    break;
            }

            IsMoving = false;
            DestinationCell = null;
            cancelRequested = false;
            moveRoutine = null;
        }

        private IEnumerator MoveToCell(GridCell targetCell)
        {
            var startPosition = transform.position;
            var endPosition = targetCell.transform.position;

            var direction = endPosition - startPosition;
            direction.y = 0f;
            var targetRotation = direction.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(direction) : transform.rotation;

            var distance = Vector3.Distance(startPosition, endPosition);
            var duration = distance / Mathf.Max(moveSpeed, 0.01f);
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);

                transform.position = Vector3.Lerp(startPosition, endPosition, t);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

                yield return null;
            }

            transform.position = endPosition;
        }
    }
}
