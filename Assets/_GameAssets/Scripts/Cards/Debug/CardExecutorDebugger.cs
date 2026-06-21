using System.Collections.Generic;
using Deckbuilder.Debugging;
using Deckbuilder.Grid;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Deckbuilder.Cards.Debugging
{
    public class CardExecutorDebugger : MonoBehaviour
    {
        [Header("Card To Test")]
        [SerializeField] private CardConfig m_card;
        [SerializeField] private Entity m_caster;

        [Header("Click To Play")]
        [SerializeField] private bool m_enableClickToPlay = true;
        [SerializeField] private LayerMask m_cellLayerMask = ~0;

        [Header("Gizmos")]
        [SerializeField] private Color m_targetZoneColor = new(0f, 0.5f, 1f, 0.5f);
        [SerializeField] private Color m_effectZoneValidColor = new(1f, 0.5f, 0f, 0.6f);
        [SerializeField] private Color m_effectZoneInvalidColor = new(0.5f, 0.5f, 0.5f, 0.4f);
        [SerializeField] private float m_markerHeight = 0.05f;
        [SerializeField] private float m_markerRadius = 0.4f;

        private GridCell m_hoveredCell;

        private Entity ResolveCaster()
        {
            if (m_caster != null)
                return m_caster;

            return CombatManager.Instance != null ? CombatManager.Instance.Player : null;
        }

        private void Update()
        {
            m_hoveredCell = null;

            if (m_card == null || DebugClickRouter.Mode != DebugClickMode.PlayCard)
                return;

            Entity _caster = ResolveCaster();
            if (_caster == null)
                return;

            if (CameraManager.Instance == null)
                return;

            Camera _camera = CameraManager.Instance.MainCam;
            if (_camera == null || Mouse.current == null)
                return;

            Ray _ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(_ray, out RaycastHit _hit, Mathf.Infinity, m_cellLayerMask))
                m_hoveredCell = _hit.collider.GetComponentInParent<GridCell>();

            if (!m_enableClickToPlay || !Mouse.current.leftButton.wasPressedThisFrame || m_hoveredCell == null)
                return;

            if (CardExecutor.Execute(m_card, _caster, m_hoveredCell))
                Debug.Log($"[CardExecutorDebugger] Played '{m_card.Title}' on {m_hoveredCell.name}.");
            else
                Debug.LogWarning($"[CardExecutorDebugger] Cannot play '{m_card.Title}' on {m_hoveredCell.name} (out of target zone, blocked LOS, or no caster cell).");
        }

        private void OnDrawGizmos()
        {
            if (m_card == null || DebugClickRouter.Mode != DebugClickMode.PlayCard)
                return;

            Entity _caster = ResolveCaster();
            if (_caster == null || _caster.EffectiveCell == null)
                return;

            Gizmos.color = m_targetZoneColor;
            foreach (GridCell _cell in GetCellsInZone(_caster.EffectiveCell.Coordinate, m_card.TargetZone.Shape, m_card.TargetZone.Size))
                Gizmos.DrawSphere(_cell.transform.position + Vector3.up * m_markerHeight, m_markerRadius * 0.5f);

            if (m_hoveredCell == null || !CardExecutor.IsWithinTargetZone(m_card, _caster, m_hoveredCell))
                return;

            bool _isValidTarget = CardExecutor.CanTarget(m_card, _caster, m_hoveredCell);
            Gizmos.color = _isValidTarget ? m_effectZoneValidColor : m_effectZoneInvalidColor;
            foreach (GridCell _cell in GetCellsInZone(m_hoveredCell.Coordinate, m_card.EffectZone.Shape, m_card.EffectZone.Size))
                Gizmos.DrawCube(_cell.transform.position + Vector3.up * m_markerHeight, Vector3.one * m_markerRadius);
        }

        private static IEnumerable<GridCell> GetCellsInZone(Vector2Int _origin, GridShape _shape, int _size)
        {
            return GridManager.Instance != null
                ? GridManager.Instance.GetCellsInZone(_origin, _shape, _size)
                : System.Array.Empty<GridCell>();
        }
    }
}
