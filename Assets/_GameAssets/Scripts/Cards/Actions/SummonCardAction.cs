using System;
using Deckbuilder.Cards;
using UnityEngine;

namespace Deckbuilder.Cards.Actions
{
    [Serializable]
    public class SummonCardAction : CardAction
    {
        [SerializeField] private Entity m_entityPrefab;

        public Entity EntityPrefab => m_entityPrefab;

        public override Type DefinitionType => typeof(SummonActionDefinition);

        // Summoning has no natural per-unit scalar yet; weight is currently flat (1 x WeightPerUnit).
        protected override float Magnitude => 1f;
    }
}
