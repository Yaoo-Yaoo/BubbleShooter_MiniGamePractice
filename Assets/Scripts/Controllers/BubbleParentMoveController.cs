using Game.Tool;
using UnityEngine;

namespace Game
{
    public class BubbleParentMoveController : MonoBehaviour
    {
        private Vector3 m_startPos;
        private Vector3 m_targetPos;
        private bool m_isMoving = false;
        private float m_moveDuration = 0f;
        private float m_moveTimer = 0f;
        
        private void Update()
        {
            // Move Bubble parent
            if (m_isMoving)
            {
                if (Vector3.Distance(transform.position, m_targetPos) > 0.01f)
                {
                    m_moveTimer += Time.deltaTime;
                    transform.position = m_startPos + (m_targetPos - m_startPos) / m_moveDuration * LerpCurveTool.GetLerpValue(m_moveTimer, CurveType.EaseInOut);
                }
                else
                {
                    transform.position = m_targetPos;
                    m_isMoving = false;
                    m_moveTimer = 0f;
                }
            }
        }

        public void LerpToTargetPos(Vector3 targetPos, float lerpTime)
        {
            m_isMoving = true;
            m_targetPos = targetPos;
            m_startPos = transform.position;
            m_moveDuration = lerpTime;
        }
    }
}
