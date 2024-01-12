using System;
using UnityEngine;

namespace Game.Data
{
    public class BubbleData : MonoBehaviour, IComparable<BubbleData>
    {
        // Data
        [Header("Data")]
        [SerializeField] private int m_Value = 0;
        public int Value
        {
            get => m_Value;
            set
            {
                m_Value = value;
                if (value == 0)
                {
                    // Empty Bubble Slot
                    m_spriteRenderer.color = new Color(1, 1, 1, 0);
                    gameObject.layer = LayerMask.NameToLayer("Bubble");
                }
                else
                {
                    // Filled Bubble Slot
                    m_spriteRenderer.color = bubbleTypeSO.GetBubbleColor(value);
                    gameObject.layer = LayerMask.NameToLayer("FilledBubble");
                }
            }
        }

        public Vector2Int index;
        
        public bool isOnScreen
        {
            get
            {
                if (transform.position.y <= 9.5f)
                    return true;
                return false;
            }
        }

        public BubbleData[] parents;
        public BubbleData[] children;

        public bool hasEmptyParents
        {
            get
            {
                for (int i = 0; i < parents.Length; i++)
                {
                    if (parents[i] && parents[i].Value != 0)
                        return false;
                }

                return true;
            }
        }

        // Components
        private SpriteRenderer m_spriteRenderer;
        
        // References
        [SerializeField] private BubbleTypeSO bubbleTypeSO;
        
        private void Awake()
        {
            // Components
            m_spriteRenderer = GetComponent<SpriteRenderer>();
            
            // Data Init
            Value = 0;
            children = new BubbleData[2] { null, null };
        }

        public int CompareTo(BubbleData other)
        {
            return index.x.CompareTo(other.index.x);
        }
    }
}
