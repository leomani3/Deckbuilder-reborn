using Unity.VisualScripting;
using UnityEngine;
using Utils.Playable;

public class EntityAE : MonoBehaviour
{
    [SerializeField] private VFXPlayable[] m_footStepsPlayables;

    private Entity m_entity;

    private void Awake()
    {
        m_entity = GetComponentInParent<Entity>();
    }
}