using System;
using Deckbuilder.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Deckbuilder.Cards
{
    [Serializable]
    public class ZoneDefinition
    {
        [SerializeField] private GridShape m_shape;
        [SerializeField, HideIf("@m_shape == GridShape.Single")]
        private int m_size;

        public GridShape Shape => m_shape;
        public int Size => m_size;
    }
}
