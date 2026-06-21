using Deckbuilder.Grid;
using Lean.Pool;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

public class CombatManager : MyBox.Singleton<CombatManager>
{
    [TitleGroup("Player")]
    [SerializeField] private Entity m_playerPrefab;

    private void Awake()
    {
        GridManager.Instance.Init();
        
        SpawnEntity(m_playerPrefab, GridManager.Instance.GetCell(0, 0));
    }

    public Entity SpawnEntity(Entity _entityPrefab, GridCell _cell)
    {
        if (_cell == null)
        {
            Debug.LogError("[CombatManager] No cell provided to spawn entity on.", this);
            return null;
        }

        Entity _entity = LeanPool.Spawn(_entityPrefab, _cell.transform.position, _cell.transform.rotation);

        if (!_cell.TrySetOccupant(_entity))
            Debug.LogWarning($"[CombatManager] Spawn cell {_cell.name} is already occupied.", this);

        return _entity;
    }
}
