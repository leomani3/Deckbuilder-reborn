using System;
using Sirenix.OdinInspector;
using UnityEngine;
using DG.Tweening;
using Stats;

public class EntityHealthModule : EntityModule
{
    public Action<double, double, bool> OnHealthChanged;
    public Action<double, bool> OnDamageTaken;
    public Action OnDeath;
    
    [FoldoutGroup("Feedback settings"), SerializeField] private Vector3 punchScale = new Vector3(0.3f, -0.2f, 0f);
    [FoldoutGroup("Feedback settings"), SerializeField, Min(0f)] private float punchDuration = 0.35f;
    [FoldoutGroup("Feedback settings"), SerializeField, Min(1)] private int punchVibrato = 6;
    [FoldoutGroup("Feedback settings"), SerializeField, Range(0f, 1f)] private float punchElasticity = 0.5f;

    protected double m_currentHealth;
    protected bool m_isDead;
    private Tween m_punchTween;
    protected EntityStatModule m_statModule;

    public bool IsDead => m_isDead;
    public double MaxHealth
    {
        get
        {
            if (Owner.TryGetModule(out EntityStatModule statModule))
            {
                return statModule.GetValue(StatType.MaxHealth);
            }
            else
            {
                return 100;
            }
        }
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();

        m_isDead = false;
    }

    public override void OnAllModuleInitialized()
    {
        base.OnAllModuleInitialized();
        Owner.TryGetModule(out m_statModule);
        m_currentHealth = MaxHealth;
    }

    protected virtual void PlayDamageFeedback(float damagePercentage)
    {
        PlayPunchScale();

        if (Owner.TryGetModule(out EntitySheenModule sheenModule))
        {
            sheenModule.PlayWhiteSheen();
        }
    }

    private void PlayPunchScale()
    {
        m_punchTween?.Kill(complete: true);
        Owner.transform.localScale = Vector3.one;

        m_punchTween = Owner.transform
            .DOPunchScale(punchScale, punchDuration, punchVibrato, punchElasticity)
            .SetUpdate(UpdateType.Normal)
            .SetLink(Owner.gameObject);
    }

    public void UpdateCurrentHealth()
    {
        m_currentHealth = MaxHealth;
        OnHealthChanged?.Invoke(m_currentHealth, MaxHealth, true);
    }

    [Button]
    public double TakeDamage(double amount, bool isCrit, bool suppressFeedback = false)
    {
        if (m_isDead) return 0d;

        if (amount <= 0f)
        {
            OnDamageTaken?.Invoke(0, false);
            return 0d;
        }

        double previous = m_currentHealth;
        m_currentHealth = Math.Max(0d, m_currentHealth - amount);
        double delta = m_currentHealth - previous;

        if (!suppressFeedback)
        {            
            PlayDamageFeedback((float)(amount / MaxHealth));
            OnDamageTaken?.Invoke(amount, isCrit);
        }

        OnHealthChanged?.Invoke(m_currentHealth, MaxHealth, suppressFeedback);

        if (m_currentHealth <= 0f)
        {
            Die();
        }
        return amount;
    }

    #region Heal
    protected virtual void Heal(double amount, bool suppressFeedback = false)
    {
        if (m_isDead || amount <= 0f) return;
        
        m_currentHealth = Math.Min(MaxHealth, m_currentHealth + amount);
        OnHealthChanged?.Invoke(m_currentHealth, MaxHealth, suppressFeedback);
    }
    #endregion

    public override void Cleanup()
    {
        OnHealthChanged = null;
        OnDamageTaken = null;
        OnDeath = null;
    }

    public virtual void Die()
    {
        m_isDead = true;
        
        OnDeath?.Invoke();
    }
}