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
        [HorizontalGroup("Range", LabelWidth = 30), LabelText("Min"), MinValue(0), MaxValue("@m_size - 1")]
        private int m_minSize;

        [SerializeField, HideIf("@m_shape == GridShape.Single")]
        [HorizontalGroup("Range", LabelWidth = 30), LabelText("Max"), MinValue(1)]
        private int m_size = 1;

        public GridShape Shape => m_shape;
        public int Size => m_size;
        public int MinSize => Mathf.Min(m_minSize, Mathf.Max(m_size - 1, 0));
    }
}
