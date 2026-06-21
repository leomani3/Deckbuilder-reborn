using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum EntityType
{
    Player = 1 << 0,
    Enemy = 1 << 1,
}

public static class EntityTypeExtensions
{
    public static List<EntityType> GetFlags(this EntityType entityType)
    {
        List<EntityType> flags = new List<EntityType>();
        foreach (EntityType value in Enum.GetValues(typeof(EntityType)))
        {
            if ((entityType & value) == value)
            {
                flags.Add(value);
            }
        }
        return flags;
    }
}

