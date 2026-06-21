using System.Collections.Generic;
using Deckbuilder.Cards.Actions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Deckbuilder.Cards
{
    [CreateAssetMenu(fileName = "NewCard", menuName = "Deckbuilder/Card Config")]
    public class CardConfig : ScriptableObject
    {
        [Title("Identity")]
        [SerializeField] private string m_id;
        [SerializeField] private string m_title;
        [SerializeField] private Sprite m_illustration;

        [Title("Target Zone")]
        [SerializeField] private ZoneDefinition m_targetZone;
        [SerializeField] private bool m_requiresLineOfSight;

        [Title("Effect Zone")]
        [SerializeField] private ZoneDefinition m_effectZone;

        [Title("Actions")]
        [SerializeReference, ListDrawerSettings(ListElementLabelName = "ActionName")]
        private List<CardAction> m_actions = new();

        public string Id => m_id;
        public string Title => m_title;
        public Sprite Illustration => m_illustration;

        public ZoneDefinition TargetZone => m_targetZone;
        public bool RequiresLineOfSight => m_requiresLineOfSight;

        public ZoneDefinition EffectZone => m_effectZone;

        public IReadOnlyList<CardAction> Actions => m_actions;
    }
}
