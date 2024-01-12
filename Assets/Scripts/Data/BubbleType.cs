using System;
using UnityEngine;

namespace Game.Data
{
    [Serializable]
    public class BubbleType
    {
        public int index;
        public Color color;
    }
    
    [CreateAssetMenu(fileName = "BubbleTypeSO", menuName = "Create/Data SO/Bubble Type SO")]
    public class BubbleTypeSO : ScriptableObject
    {
        public BubbleType[] bubbleTypes;

        public Color GetBubbleColor(int value)
        {
            return bubbleTypes[value - 1].color;
        }
    }
}
