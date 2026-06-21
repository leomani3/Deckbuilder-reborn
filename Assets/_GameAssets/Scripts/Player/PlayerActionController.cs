using System.Collections.Generic;
using Deckbuilder.Actions;
using Deckbuilder.Cards;
using Deckbuilder.Grid;
using Deckbuilder.Grid.Highlighting;
using Stats;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Deckbuilder.Player
{
    public class PlayerActionController : MonoBehaviour
    {
        private enum Mode
        {
            Move,
            CardPreview
        }

        [Header("Entity")]
        [SerializeField] private Entity m_entity;

        [Header("Card (temporary test binding)")]
        [SerializeField] private CardConfig m_testCard;
        [SerializeField] private Key m_moveModeKey = Key.Digit1;
        [SerializeField] private Key m_selectCardKey = Key.Digit2;

        [Header("Input")]
        [SerializeField] private LayerMask m_cellLayerMask = ~0;

        [Header("Highlight Colors")]
        [SerializeField] private Color m_moveRangeColor = new(0f, 1f, 0f, 0.4f);
        [SerializeField] private Color m_movePathColor = new(0f, 1f, 0f, 0.8f);
        [SerializeField] private Color m_targetZoneColor = new(0f, 0.5f, 1f, 0.5f);
        [SerializeField] private Color m_targetZoneBlockedColor = new(0.4f, 0.4f, 0.4f, 0.4f);
        [SerializeField] private Color m_effectZoneColor = new(1f, 0.5f, 0f, 0.6f);

        private Mode m_mode = Mode.Move;
        private GridCell m_hoveredCell;

        private Entity ResolveEntity()
        {
            if (m_entity != null)
                return m_entity;

            return CombatManager.Instance != null ? CombatManager.Instance.Player : null;
        }

        private void Update()
        {
            UpdateModeSwitch();
            UpdateHover();

            if (CellHighlightManager.Instance == null || GridManager.Instance == null)
                return;

            Entity _entity = ResolveEntity();

            if (m_mode == Mode.Move)
                UpdateMoveMode(_entity);
            else
                UpdateCardPreviewMode(_entity);
        }

        private void UpdateModeSwitch()
        {
            if (Keyboard.current == null)
                return;

            if (Keyboard.current[m_moveModeKey].wasPressedThisFrame)
                m_mode = Mode.Move;
            else if (m_mode == Mode.Move && m_testCard != null && Keyboard.current[m_selectCardKey].wasPressedThisFrame)
                m_mode = Mode.CardPreview;
        }

        private void UpdateHover()
        {
            m_hoveredCell = null;

            if (CameraManager.Instance == null)
                return;

            Camera _camera = CameraManager.Instance.MainCam;
            if (_camera == null || Mouse.current == null)
                return;

            Ray _ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(_ray, out RaycastHit _hit, Mathf.Infinity, m_cellLayerMask))
                m_hoveredCell = _hit.collider.GetComponentInParent<GridCell>();
        }

        private void UpdateMoveMode(Entity _entity)
        {
            ClearCardHighlights();

            if (_entity == null || _entity.EffectiveCell == null)
            {
                ClearMoveHighlights();
                return;
            }

            int _movementPoints = GetMovementPoints(_entity);
            List<GridCell> _reachable = GridManager.Instance.GetReachableCells(_entity.EffectiveCell, _movementPoints);

            CellHighlightManager.Instance.ClearLayer(HighlightLayer.MovementRange);
            CellHighlightManager.Instance.ClearLayer(HighlightLayer.MovementPath);

            foreach (GridCell _cell in _reachable)
                CellHighlightManager.Instance.Highlight(_cell, HighlightLayer.MovementRange, m_moveRangeColor);

            bool _hoveredReachable = m_hoveredCell != null && _reachable.Contains(m_hoveredCell);
            if (_hoveredReachable)
            {
                List<GridCell> _path = GridManager.Instance.FindPath(_entity.EffectiveCell, m_hoveredCell);
                if (_path != null)
                {
                    foreach (GridCell _cell in _path)
                        CellHighlightManager.Instance.Highlight(_cell, HighlightLayer.MovementPath, m_movePathColor);
                }
            }

            if (_hoveredReachable && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                EnqueueMove(_entity, m_hoveredCell);
        }

        private void UpdateCardPreviewMode(Entity _entity)
        {
            ClearMoveHighlights();

            CellHighlightManager.Instance.ClearLayer(HighlightLayer.TargetZone);
            CellHighlightManager.Instance.ClearLayer(HighlightLayer.EffectZone);

            if (_entity == null || _entity.EffectiveCell == null || m_testCard == null)
            {
                m_mode = Mode.Move;
                return;
            }

            foreach (GridCell _cell in GetCellsInZone(_entity.EffectiveCell.Coordinate, m_testCard.TargetZone.Shape, m_testCard.TargetZone.Size))
            {
                bool _hasLineOfSight = !m_testCard.RequiresLineOfSight
                    || GridManager.Instance.HasLineOfSight(_entity.EffectiveCell, _cell, out GridCell _blockingCell);

                Color _cellColor = _hasLineOfSight ? m_targetZoneColor : m_targetZoneBlockedColor;
                CellHighlightManager.Instance.Highlight(_cell, HighlightLayer.TargetZone, _cellColor);
            }

            if (m_hoveredCell == null || !CardExecutor.CanTarget(m_testCard, _entity, m_hoveredCell))
                return;

            foreach (GridCell _cell in GetCellsInZone(m_hoveredCell.Coordinate, m_testCard.EffectZone.Shape, m_testCard.EffectZone.Size))
                CellHighlightManager.Instance.Highlight(_cell, HighlightLayer.EffectZone, m_effectZoneColor);

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                EnqueueCard(_entity, m_hoveredCell);
                m_mode = Mode.Move;
            }
        }

        private void EnqueueMove(Entity _entity, GridCell _destination)
        {
            if (_entity.TryGetModule(out EntityActionQueueModule _queue))
                _queue.Enqueue(new MoveAction(_entity, _destination));
        }

        private void EnqueueCard(Entity _entity, GridCell _targetCell)
        {
            if (_entity.TryGetModule(out EntityActionQueueModule _queue))
                _queue.Enqueue(new PlayCardAction(m_testCard, _entity, _targetCell));
        }

        private int GetMovementPoints(Entity _entity)
        {
            if (_entity.TryGetModule(out EntityStatModule _statModule))
                return Mathf.RoundToInt(_statModule.GetValue(StatType.MovementPoints));

            return 3;
        }

        private void ClearMoveHighlights()
        {
            if (CellHighlightManager.Instance == null)
                return;

            CellHighlightManager.Instance.ClearLayer(HighlightLayer.MovementRange);
            CellHighlightManager.Instance.ClearLayer(HighlightLayer.MovementPath);
        }

        private void ClearCardHighlights()
        {
            if (CellHighlightManager.Instance == null)
                return;

            CellHighlightManager.Instance.ClearLayer(HighlightLayer.TargetZone);
            CellHighlightManager.Instance.ClearLayer(HighlightLayer.EffectZone);
        }

        private static IEnumerable<GridCell> GetCellsInZone(Vector2Int _origin, GridShape _shape, int _size)
        {
            return GridManager.Instance != null
                ? GridManager.Instance.GetCellsInZone(_origin, _shape, _size)
                : System.Array.Empty<GridCell>();
        }
    }
}
