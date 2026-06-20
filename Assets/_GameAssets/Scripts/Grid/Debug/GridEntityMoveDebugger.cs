using UnityEngine;
using UnityEngine.InputSystem;

namespace Deckbuilder.Grid.Debugging
{
    public class GridEntityMoveDebugger : MonoBehaviour
    {
        [Header("Manual Test")]
        [SerializeField] private GridEntity entity;

        [Header("Click To Move")]
        [SerializeField] private bool enableClickToMove = true;
        [SerializeField] private LayerMask cellLayerMask = ~0;

        [Header("Gizmos")]
        [SerializeField] private Color currentCellColor = Color.green;
        [SerializeField] private Color destinationCellColor = Color.yellow;
        [SerializeField] private float markerHeight = 0.05f;
        [SerializeField] private float markerRadius = 0.4f;

        [ContextMenu("Cancel Move")]
        private void CancelMove()
        {
            if (entity == null)
                return;

            entity.CancelMove();
        }

        private void Update()
        {
            if (!enableClickToMove || entity == null)
                return;

            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
                return;

            if (CameraManager.Instance == null)
                return;

            var camera = CameraManager.Instance.MainCam;
            if (camera == null)
                return;

            var ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, cellLayerMask))
            {
                var cell = hit.collider.GetComponentInParent<GridCell>();
                if (cell != null)
                    entity.MoveTo(cell);
            }
        }

        private void OnDrawGizmos()
        {
            if (entity == null)
                return;

            if (entity.CurrentCell != null)
            {
                Gizmos.color = currentCellColor;
                Gizmos.DrawSphere(entity.CurrentCell.transform.position + Vector3.up * markerHeight, markerRadius);
            }

            if (entity.DestinationCell != null)
            {
                Gizmos.color = destinationCellColor;
                Gizmos.DrawSphere(entity.DestinationCell.transform.position + Vector3.up * markerHeight, markerRadius);
            }
        }
    }
}
