using System;
using System.Collections;
using UnityEngine;

public class EntityAnimationModule : EntityModule
{
    private const string CastTriggerName = "Cast";
    private const string CastPlaceholderClipName = "CastPlaceholder";
    private const float CompletionSafetyMargin = 1f;

    [SerializeField] private Animator m_animator;

    private AnimatorOverrideController m_overrideController;
    private Action m_pendingTriggerCallback;
    private bool m_pendingCompletion;

    protected override void OnInitialize()
    {
        base.OnInitialize();

        if (m_animator == null)
            m_animator = GetComponentInChildren<Animator>();

        if (m_animator != null && m_overrideController == null && m_animator.runtimeAnimatorController != null)
        {
            m_overrideController = new AnimatorOverrideController(m_animator.runtimeAnimatorController);
            m_animator.runtimeAnimatorController = m_overrideController;
        }
    }

    public IEnumerator PlayCast(AnimationClip _clip, Action _onTrigger)
    {
        if (m_animator == null || m_overrideController == null || _clip == null)
        {
            _onTrigger?.Invoke();
            yield break;
        }

        m_overrideController[CastPlaceholderClipName] = _clip;
        m_pendingTriggerCallback = _onTrigger;
        m_pendingCompletion = true;

        m_animator.SetTrigger(CastTriggerName);

        // Event-driven happy path: OnComplete() clears m_pendingCompletion. The timer below is purely
        // a deadlock guard in case a clip is missing its 'OnComplete' event, not a timing mechanism.
        float _safetyTimeout = _clip.length + CompletionSafetyMargin;
        float _elapsed = 0f;

        while (m_pendingCompletion && _elapsed < _safetyTimeout)
        {
            _elapsed += Time.deltaTime;
            yield return null;
        }

        if (m_pendingCompletion)
        {
            Debug.LogError($"[EntityAnimationModule] Clip '{_clip.name}' never fired an 'OnComplete' animation event. Forcing completion so the action queue doesn't stall.");
            m_pendingCompletion = false;
        }

        if (m_pendingTriggerCallback != null)
        {
            Debug.LogError($"[EntityAnimationModule] Clip '{_clip.name}' finished without firing an 'OnTrigger' animation event. Card effects were never applied.");
            m_pendingTriggerCallback = null;
        }
    }

    // Invoked by EntityAE, which relays the Animation Event up from the Animator's GameObject.
    public void OnTrigger()
    {
        m_pendingTriggerCallback?.Invoke();
        m_pendingTriggerCallback = null;
    }

    // Invoked by EntityAE, which relays the Animation Event up from the Animator's GameObject.
    public void OnComplete()
    {
        m_pendingCompletion = false;
    }
}
