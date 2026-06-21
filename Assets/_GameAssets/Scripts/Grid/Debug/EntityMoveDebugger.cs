using UnityEngine;
using UnityEngine.InputSystem;

namespace Deckbuilder.Grid.Debugging
{
    public class EntityMoveDebugger : MonoBehaviour
    {
        [Header("Manual Test")]
        [SerializeField] private Entity m_entity;

        [Header("Click To Move")]
        [SerializeField] private bool m_enableClickToMove = true;
        [SerializeField] private LayerMask m_cellLayerMask = ~0;

        [Header("Gizmos")]
        [SerializeField] private Color m_currentCellColor = Color.green;
        [SerializeField] private Color m_destinationCellColor = Color.yellow;
        [SerializeField] private float m_markerHeight = 0.05f;
        [SerializeField] private float m_markerRadius = 0.4f;

        [ContextMenu("Cancel Move")]
        private void CancelMove()
        {
            if (m_entity == null)
                return;

            m_entity.CancelMove();
        }

        private void Update()
        {
            if (!m_enableClickToMove || m_entity == null)
                return;

            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
                return;

            if (CameraManager.Instance == null)
                return;

            Camera _camera = CameraManager.Instance.MainCam;
            if (_camera == null)
                return;

            Ray _ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(_ray, out RaycastHit _hit, Mathf.Infinity, m_cellLayerMask))
            {
                GridCell _cell = _hit.collider.GetComponentInParent<GridCell>();
                if (_cell != null)
                    m_entity.MoveTo(_cell);
            }
        }

        private void OnDrawGizmos()
        {
            if (m_entity == null)
                return;

            if (m_entity.CurrentCell != null)
            {
                Gizmos.color = m_currentCellColor;
                Gizmos.DrawSphere(m_entity.CurrentCell.transform.position + Vector3.up * m_markerHeight, m_markerRadius);
            }

            if (m_entity.DestinationCell != null)
            {
                Gizmos.color = m_destinationCellColor;
                Gizmos.DrawSphere(m_entity.DestinationCell.transform.position + Vector3.up * m_markerHeight, m_markerRadius);
            }
        }
    }
}
