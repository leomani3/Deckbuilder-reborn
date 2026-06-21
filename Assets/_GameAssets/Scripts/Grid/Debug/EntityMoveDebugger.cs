using Deckbuilder.Debugging;
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

        private Entity ResolveEntity()
        {
            if (m_entity != null)
                return m_entity;

            return CombatManager.Instance != null ? CombatManager.Instance.Player : null;
        }

        [ContextMenu("Cancel Move")]
        private void CancelMove()
        {
            Entity _entity = ResolveEntity();
            if (_entity == null)
                return;

            _entity.CancelMove();
        }

        private void Update()
        {
            if (!m_enableClickToMove || DebugClickRouter.Mode != DebugClickMode.Move)
                return;

            Entity _entity = ResolveEntity();
            if (_entity == null)
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
                    _entity.MoveTo(_cell);
            }
        }

        private void OnDrawGizmos()
        {
            Entity _entity = ResolveEntity();
            if (_entity == null)
                return;

            if (_entity.CurrentCell != null)
            {
                Gizmos.color = m_currentCellColor;
                Gizmos.DrawSphere(_entity.CurrentCell.transform.position + Vector3.up * m_markerHeight, m_markerRadius);
            }

            if (_entity.DestinationCell != null)
            {
                Gizmos.color = m_destinationCellColor;
                Gizmos.DrawSphere(_entity.DestinationCell.transform.position + Vector3.up * m_markerHeight, m_markerRadius);
            }
        }
    }
}
