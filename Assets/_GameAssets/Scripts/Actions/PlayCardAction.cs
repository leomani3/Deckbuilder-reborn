using System.Collections;
using Deckbuilder.Cards;
using Deckbuilder.Grid;
using UnityEngine;

namespace Deckbuilder.Actions
{
    public class PlayCardAction : IEntityAction
    {
        private readonly CardConfig m_card;
        private readonly Entity m_caster;
        private readonly GridCell m_targetCell;

        public PlayCardAction(CardConfig _card, Entity _caster, GridCell _targetCell)
        {
            m_card = _card;
            m_caster = _caster;
            m_targetCell = _targetCell;
        }

        public IEnumerator Execute()
        {
            m_caster.FaceTowards(m_targetCell.transform.position);

            if (m_card.CastAnimation != null && m_caster.TryGetModule(out EntityAnimationModule _animationModule))
            {
                yield return _animationModule.PlayCast(m_card.CastAnimation, ApplyCard);
            }
            else
            {
                ApplyCard();
            }
        }

        private void ApplyCard()
        {
            if (!CardExecutor.Execute(m_card, m_caster, m_targetCell))
                Debug.LogWarning($"[PlayCardAction] Failed to play '{m_card.Title}' on {m_targetCell.name}.");
        }
    }
}
