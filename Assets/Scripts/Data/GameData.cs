using System.Collections.Generic;
using Game.Tool;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Data
{
    public class GameData : MonoBehaviour
    {
        [Header("--------------------Parameters--------------------")] 
        [Header("Bubble Slots Related")]
        [SerializeField] private Vector2 topLeftPos;
        [SerializeField] private int maxColumnCount;
        [SerializeField] private int totalRowCount;
        [SerializeField] private int shownRowCount;
        public float bubbleRadius = 0.5f;

        [Header("Function Related")] 
        [SerializeField] private float fallInterval;
        [SerializeField] private float fallDuration;
        
        [Header("--------------------References--------------------")] 
        [SerializeField] private GameObject bubblePrefab;
        [SerializeField] private Transform bubbleParent;
        [SerializeField] private BubbleTypeSO bubbleTypeSO;
        
        // Data
        private Dictionary<int, List<BubbleData>> m_allBubbleSlots;
        private List<BubbleData> m_filledBubbleSlots;

        private float m_rowHeight;
        
        private BubbleParentMoveController m_bubbleParentMoveController;
        private bool m_canFall = true;
        private float m_fallIntervalTimer = 0f;
        private int m_fallCount;

        private int m_comboCount = 0;
        private List<BubbleData> m_usedBubbleList;
        private List<BubbleData> m_childrenList;
        private Dictionary<BubbleData, int> m_originValue;

        private int m_failureCount = 0;

        private void Start()
        {
            // File
            JsonManager.Instance.Load(Application.streamingAssetsPath + "/JsonFiles/BubbleGenerate.json");
            
            // References Init
            m_bubbleParentMoveController = bubbleParent.GetComponent<BubbleParentMoveController>();
            
            // Data Init
            m_allBubbleSlots = new Dictionary<int, List<BubbleData>>();
            m_filledBubbleSlots = new List<BubbleData>();
            m_rowHeight = Mathf.Sqrt(2 * bubbleRadius - bubbleRadius * bubbleRadius);
            m_canFall = true;
            m_fallIntervalTimer = 0f;
            m_fallCount = totalRowCount - shownRowCount;

            // Bubbles Init
            InitBubbleSlots();
            GenerateBubbles();
        }

        private void Update()
        {
            if (m_failureCount >= 3)
            {
                FallBubbleSlots(1);
                m_failureCount = 0;
            }
        }
        
        private void InitBubbleSlots()
        {
            for (int i = 0; i < totalRowCount; i++)
            {
                int columnCount = i % 2 == 0 ? maxColumnCount : maxColumnCount - 1;
                Vector2 rowBasicPos = new Vector2(i % 2 == 0 ? topLeftPos.x : topLeftPos.x + bubbleRadius, topLeftPos.y + (totalRowCount - shownRowCount - i) * m_rowHeight);
                List<BubbleData> rowBubbles = new List<BubbleData>();
                
                for (int j = 0; j < columnCount; j++)
                {
                    GameObject bubble = Instantiate(bubblePrefab, bubbleParent);
                    bubble.transform.position = new Vector3(rowBasicPos.x + j * bubbleRadius * 2, rowBasicPos.y, 0); 
                    bubble.name = $"{i},{j}";
                    
                    BubbleData data = bubble.GetComponent<BubbleData>();
                    data.index = new Vector2Int(i, j);
                    
                    // assign parents
                    if (i - 1 >= 0)  // has parent line
                    {
                        data.parents = new BubbleData[2];

                        if (columnCount != maxColumnCount)  // not full bubble line
                        {
                            // left
                            data.parents[0] = m_allBubbleSlots[i - 1][j];
                            m_allBubbleSlots[i - 1][j].children[1] = data;
                            
                            // right
                            if (j + 1 < maxColumnCount)
                            {
                                data.parents[1] = m_allBubbleSlots[i - 1][j + 1];
                                m_allBubbleSlots[i - 1][j + 1].children[0] = data;
                            }
                        }
                        else  // full bubble line
                        {
                            // left
                            if (j - 1 >= 0)
                            {
                                data.parents[0] = m_allBubbleSlots[i - 1][j - 1];
                                m_allBubbleSlots[i - 1][j - 1].children[1] = data;
                            }
                            
                            // right
                            if (j < maxColumnCount - 1)
                            {
                                data.parents[1] = m_allBubbleSlots[i - 1][j];
                                m_allBubbleSlots[i - 1][j].children[0] = data;
                            }
                        }
                    }
           
                    rowBubbles.Add(data);
                }
                
                m_allBubbleSlots.Add(i, rowBubbles);
            }
        }

        private void FallBubbleSlots(int moveStep = 1)
        {
            m_bubbleParentMoveController.LerpToTargetPos(new Vector3(bubbleParent.position.x, bubbleParent.position.y - moveStep * m_rowHeight, 0), fallDuration);
            m_fallCount -= moveStep;

            // Cannot fall when all shown
            if (m_fallCount <= 0)
                m_canFall = false;
        }
        
        private void GenerateBubbles()
        {
            // Deal with json data
            for (int i = 0; i < JsonManager.Instance.bubbleGeneratorData.BubbleGenerateList.Count; i++)
            {
                BubbleGeneratorLine bubbleLine = JsonManager.Instance.bubbleGeneratorData.BubbleGenerateList[i];
                
                if (!m_allBubbleSlots.ContainsKey(bubbleLine.rowIndex))
                    return;
                
                string[] lineData = bubbleLine.bubbles.Split(',');
                
                for (int j = 0; j < lineData.Length; j++)
                {
                    if (j >= m_allBubbleSlots[bubbleLine.rowIndex].Count)
                        break;
                    
                    if (int.Parse(lineData[j]) != 0)
                    {
                        m_allBubbleSlots[bubbleLine.rowIndex][j].Value = int.Parse(lineData[j]);
                        m_filledBubbleSlots.Add(m_allBubbleSlots[bubbleLine.rowIndex][j]);
                    }
                }
            }
        }
        
        public GameObject RandomANewBubble(Vector3 pos)
        {
            GameObject bubble = Instantiate(bubblePrefab, pos, Quaternion.identity);
            BubbleData data = bubble.GetComponent<BubbleData>();
            data.Value = Random.Range(bubbleTypeSO.bubbleTypes[0].index, bubbleTypeSO.bubbleTypes[bubbleTypeSO.bubbleTypes.Length - 1].index + 1);
            data.index = new Vector2Int(-1, -1);
            bubble.gameObject.layer = LayerMask.NameToLayer("Default");
            return bubble;
        }

        public void CheckBubbleCombo(BubbleData bubble)
        {
            m_comboCount = 0;
            m_usedBubbleList = new List<BubbleData>();
            RecursiveFindNeighbourBubble(bubble);
            
            // Debug.Log($"Neighbour Bubbles: {m_comboCount}");

            if (m_comboCount < 3)
            {
                m_failureCount++;
                return;
            }

            m_failureCount = 0;
            
            // Combo
            for (int i = 0; i < m_usedBubbleList.Count; i++)
            {
                m_usedBubbleList[i].Value = 0;
                m_filledBubbleSlots.Remove(m_usedBubbleList[i]);
            }
            
            // Cancel all the bubbles that has no connection around
            m_usedBubbleList.Sort();
            m_childrenList = new List<BubbleData>();
            m_originValue = new Dictionary<BubbleData, int>();
            for (int i = m_usedBubbleList[0].index.x; i < totalRowCount; i++)
            {
                for (int j = 0; j < m_allBubbleSlots[i].Count; j++)
                {
                    if (m_allBubbleSlots[i][j].hasEmptyParents && m_allBubbleSlots[i][j].Value != 0)
                    {
                        m_childrenList.Add(m_allBubbleSlots[i][j]);
                        m_originValue.Add(m_allBubbleSlots[i][j], m_allBubbleSlots[i][j].Value);
                        m_allBubbleSlots[i][j].Value = 0;
                    }
                }
            }
            
            // Check if included irrelevant bubbles
            for (int i = 0; i < m_childrenList.Count; i++)
            {
                bool needRecover = false;
                
                // left
                if (m_childrenList[i].index.y - 1 >= 0)
                {
                    BubbleData leftBubble = m_allBubbleSlots[m_childrenList[i].index.x][m_childrenList[i].index.y - 1];
                    if (leftBubble.Value != 0 && !m_childrenList.Contains(leftBubble))
                        needRecover = true;
                }
                
                // right
                if (!needRecover)
                {
                    if (m_childrenList[i].index.y + 1 < m_allBubbleSlots[m_childrenList[i].index.x].Count)
                    {
                        BubbleData rightBubble = m_allBubbleSlots[m_childrenList[i].index.x][m_childrenList[i].index.y + 1];
                        if (rightBubble.Value != 0 && !m_childrenList.Contains(rightBubble))
                            needRecover = true;
                    }
                }

                if (needRecover)
                    m_childrenList[i].Value = m_originValue[m_childrenList[i]];
                else
                    m_filledBubbleSlots.Remove(m_childrenList[i]);
            }
        }

        private void RecursiveFindNeighbourBubble(BubbleData centerBubble)
        {
            m_filledBubbleSlots.Add(centerBubble);
            m_usedBubbleList.Add(centerBubble);
            m_comboCount++;

            // parents
            if (centerBubble.parents != null)
            {
                for (int i = 0; i < centerBubble.parents.Length; i++)
                {
                    if (centerBubble.parents[i])
                    {
                        BubbleData currentBubble = centerBubble.parents[i];
                        if (currentBubble && currentBubble.Value != 0 && currentBubble.Value == centerBubble.Value && currentBubble.isOnScreen && !m_usedBubbleList.Contains(currentBubble))
                            RecursiveFindNeighbourBubble(currentBubble);
                    }
                }
            }

            // children
            if (centerBubble.children != null)
            {
                for (int i = 0; i < centerBubble.children.Length; i++)
                {
                    if (centerBubble.children[i])
                    {
                        BubbleData currentBubble = centerBubble.children[i];
                        if (currentBubble && currentBubble.Value != 0 && currentBubble.Value == centerBubble.Value && currentBubble.isOnScreen && !m_usedBubbleList.Contains(currentBubble))
                            RecursiveFindNeighbourBubble(currentBubble);
                    }
                }
            }
 
            // left
            if (centerBubble.index.y - 1 >= 0)
            {
                BubbleData currentBubble = m_allBubbleSlots[centerBubble.index.x][centerBubble.index.y - 1];
                if (currentBubble && currentBubble.Value != 0 && currentBubble.Value == centerBubble.Value && currentBubble.isOnScreen && !m_usedBubbleList.Contains(currentBubble))
                    RecursiveFindNeighbourBubble(currentBubble);
            }

            // right
            if (centerBubble.index.y + 1 < m_allBubbleSlots[centerBubble.index.x].Count)
            {
                BubbleData currentBubble = m_allBubbleSlots[centerBubble.index.x][centerBubble.index.y + 1];
                if (currentBubble && currentBubble.Value != 0 && currentBubble.Value == centerBubble.Value && currentBubble.isOnScreen && !m_usedBubbleList.Contains(currentBubble))
                    RecursiveFindNeighbourBubble(currentBubble);
            }
        }
    }
}
