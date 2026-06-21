using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Deckbuilder.Grid;
using Lean.Pool;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

public class Entity : MonoBehaviour, IPoolable
{
    [TitleGroup("Settings")]
    [SerializeField] private EntityType m_entityType;

    private Dictionary<Type, EntityModule> m_modules = new Dictionary<Type, EntityModule>();
    private bool m_isAlive = true;
    private bool m_isSpawning;

    public EntityType EntityType => m_entityType;
    public bool IsAlive => m_isAlive;
    public bool IsSpawning => m_isSpawning;

    public GridCell CurrentCell { get; private set; }
    public GridCell DestinationCell { get; private set; }
    public bool IsMoving { get; private set; }

    private Coroutine m_moveRoutine;
    private bool m_cancelMoveRequested;

    internal void SetSpawning(bool _value) => m_isSpawning = _value;

    private void RegisterModules()
    {
        EntityModule[] _modules = GetComponents<EntityModule>();
        foreach (EntityModule _module in _modules)
        {
            Type _type = _module.GetType();
            while (_type != null && typeof(EntityModule).IsAssignableFrom(_type))
            {
                m_modules.TryAdd(_type, _module);
                _type = _type.BaseType;
            }

            _module.Initialize(this);
        }

        foreach (EntityModule _module in m_modules.Values.Distinct())
        {
            _module.OnAllModuleInitialized();
        }

        if (TryGetModule(out EntityHealthModule _healthModule))
        {
            _healthModule.OnDeath += OnDeath;
        }
    }


    public bool TryGetModule<T>(out T _module) where T : EntityModule
    {
        if (m_modules.TryGetValue(typeof(T), out EntityModule _raw))
        {
            _module = (T)_raw;
            return true;
        }

        _module = null;
        return false;
    }

    public void Despawn()
    {
        if (TryGetModule(out EntityHealthModule _healthModule))
        {
            _healthModule.OnDeath -= OnDeath;
        }

        LeanPool.Despawn(this);
    }

    private void Register()
    {
        EntityManager.Instance?.Register(this);
    }

    private void OnDeath()
    {
        m_isAlive = false;
        EntityManager.Instance?.Unregister(this);
        Despawn();
    }

    public void OnSpawn()
    {
        m_isAlive = true;

        RegisterModules();
        Register();
    }

    public void OnDespawn()
    {
        foreach (EntityModule _module in m_modules.Values.Distinct())
            _module.Cleanup();

        if (m_moveRoutine != null)
        {
            StopCoroutine(m_moveRoutine);
            m_moveRoutine = null;
        }

        if (CurrentCell != null && CurrentCell.Occupant == this)
            CurrentCell.ClearOccupant();

        CurrentCell = null;
        DestinationCell = null;
        IsMoving = false;
        m_cancelMoveRequested = false;
    }

    public void SetCurrentCell(GridCell _cell)
    {
        CurrentCell = _cell;
    }

    public bool MoveTo(GridCell _destination, bool _ignoreOccupants = false, bool _randomizePath = false)
    {
        if (IsMoving || _destination == null || GridManager.Instance == null || CurrentCell == null)
            return false;

        List<GridCell> _path = GridManager.Instance.FindPath(CurrentCell, _destination, _ignoreOccupants, _randomizePath);
        if (_path == null || _path.Count < 2)
            return false;

        DestinationCell = _destination;
        m_cancelMoveRequested = false;
        m_moveRoutine = StartCoroutine(FollowPath(_path));
        return true;
    }

    public void CancelMove()
    {
        if (IsMoving)
            m_cancelMoveRequested = true;
    }

    private IEnumerator FollowPath(List<GridCell> _path)
    {
        IsMoving = true;

        for (int _i = 1; _i < _path.Count; _i++)
        {
            GridCell _targetCell = _path[_i];
            yield return MoveToCell(_targetCell);
            GridManager.Instance.MoveEntity(this, _targetCell);

            if (m_cancelMoveRequested)
                break;
        }

        IsMoving = false;
        DestinationCell = null;
        m_cancelMoveRequested = false;
        m_moveRoutine = null;
    }

    private IEnumerator MoveToCell(GridCell _targetCell)
    {
        Vector3 _startPosition = transform.position;
        Vector3 _endPosition = _targetCell.transform.position;

        Vector3 _direction = _endPosition - _startPosition;
        _direction.y = 0f;
        Quaternion _targetRotation = _direction.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(_direction) : transform.rotation;

        float _moveSpeed = GameConfig.Instance != null ? GameConfig.Instance.entitySettings.moveSpeed : 1f;
        float _rotationSpeed = GameConfig.Instance != null ? GameConfig.Instance.entitySettings.rotationSpeed : 1f;

        float _distance = Vector3.Distance(_startPosition, _endPosition);
        float _duration = _distance / Mathf.Max(_moveSpeed, 0.01f);
        float _elapsed = 0f;

        while (_elapsed < _duration)
        {
            _elapsed += Time.deltaTime;
            float _t = Mathf.Clamp01(_elapsed / _duration);

            transform.position = Vector3.Lerp(_startPosition, _endPosition, _t);
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime * _rotationSpeed);

            yield return null;
        }

        transform.position = _endPosition;
    }
}
