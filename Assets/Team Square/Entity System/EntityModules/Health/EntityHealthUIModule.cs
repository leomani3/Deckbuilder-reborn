using Lean.Pool;
using Sirenix.OdinInspector;
using UnityEngine;

public class EntityHealthUIModule : EntityModule
{
    [SerializeField, Required] private FloatingTextConfig m_floatingTextConfig;
    [SerializeField, Required] private FloatingTextConfig m_criticalFloatingTextConfig;
    [SerializeField, Required] private Transform m_healthBarTarget;
    [SerializeField, Required] private GenericGauge m_genericGaugePrefab;

    protected GenericGauge m_spawnedGenericGauge;

    public override void OnAllModuleInitialized()
    {
        if (!Owner.TryGetModule(out EntityHealthModule healthModule))
        {
            Debug.LogWarning($"[EntityHealthUIModule] No EntityHealthModule found on {Owner.name}. Health bar will not function.");
            return;
        }

        healthModule.OnDamageTaken += OnTakeDamage;
        healthModule.OnHealthChanged += HandleHealthChanged;
        healthModule.OnDeath += OnDeath;

        SpawnHealthBar(healthModule.MaxHealth);
    }

    protected virtual void SpawnHealthBar(double maxHealth)
    {
        m_spawnedGenericGauge = LeanPool.Spawn(m_genericGaugePrefab, UIManager.Instance.GetCanvas<GameCanvas>().transform);
        m_spawnedGenericGauge.Setup(m_healthBarTarget, maxHealth, maxHealth);
    }

    protected virtual void DespawnHealthBar() { }


    protected void HandleHealthChanged(double currentHealth, double maxHealth, bool suppressFeedback)
    {
        if (m_spawnedGenericGauge == null) return;
        m_spawnedGenericGauge.SetValue(currentHealth, maxHealth, instant: false, showChunks: !suppressFeedback);
    }

    protected void OnTakeDamage(double amount, bool isCrit)
    {
        OnFloatingTextRequested((float)amount, isCrit);
    }

    protected void OnFloatingTextRequested(float amount, bool isCrit)
    {
        Vector3 spawnPos = m_healthBarTarget != null ? m_healthBarTarget.position : Owner.transform.position;
        
        if (isCrit)
            FloatingTextManager.Instance.SpawnWorldText(spawnPos, $"{amount:N0}", m_criticalFloatingTextConfig);
        else
            FloatingTextManager.Instance.SpawnWorldText(spawnPos, $"{amount:N0}", m_floatingTextConfig);
    }

    protected virtual void OnDeath()
    {
        if (Owner.TryGetModule(out EntityHealthModule healthModule))
        {
            healthModule.OnDeath -= OnDeath;
        }

        DespawnHealthBar();
    }
}
