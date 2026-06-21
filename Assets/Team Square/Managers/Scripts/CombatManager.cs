using Deckbuilder.Grid;
using Lean.Pool;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

public class CombatManager : MyBox.Singleton<CombatManager>
{
    [TitleGroup("Player")]
    [SerializeField] private Entity m_playerPrefab;
    [SerializeField] private Vector2Int m_playerSpawnCoordinate = new(0, 0);

    [TitleGroup("Enemy")]
    [SerializeField] private Entity m_enemyPrefab;
    [SerializeField] private Vector2Int m_enemySpawnCoordinate = new(3, 0);

    public Entity Player { get; private set; }
    public Entity Enemy { get; private set; }

    private void Awake()
    {
        GridManager.Instance.Init();

        Player = SpawnEntity(m_playerPrefab, GridManager.Instance.GetCell(m_playerSpawnCoordinate.x, m_playerSpawnCoordinate.y));
        Enemy = SpawnEntity(m_enemyPrefab, GridManager.Instance.GetCell(m_enemySpawnCoordinate.x, m_enemySpawnCoordinate.y));
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
