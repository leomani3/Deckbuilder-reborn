using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Stats
{
    
    public enum ModifierType
    {
        Flat,
        Percentage,
        Multiplier
    }
    
    [Serializable]
    public abstract class AStatModifier
    {
        public string id;
        public EntityType entityType;
        public StatType statType;
        public ModifierType type;

        protected AStatModifier(EntityType _entityType, StatType _statType, ModifierType _type, string _id = null)
        {
            id         = _id;
            entityType = _entityType;
            statType   = _statType;
            type       = _type;
        }
    }
}
