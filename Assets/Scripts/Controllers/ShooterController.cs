using System.Collections.Generic;
using Game.Data;
using UnityEngine;

namespace Game
{
    public class ShooterController : MonoBehaviour
    {
        [SerializeField] private Transform m_headAnchor;
        [SerializeField] private LineRenderer m_aimingLine;
        [SerializeField] private float m_shootSpeed = 1f;

        private Camera m_mainCam;
        private Vector3 m_mouseWorldPos = Vector3.zero;
        private Vector3 m_direction = Vector3.up;
        private float m_angle = 0f;

        private GameData m_gameData;

        private List<Vector3> m_turningPoints = new List<Vector3>();
        private bool m_isShootBubbleMoving = false;
        private GameObject m_shootBubble;
        private BubbleData m_shootTargetBubbleSlot;
        private int m_currentTargetIndex = 0;
        
        private void Start()
        {
            m_mainCam = Camera.main;
            m_gameData = GameObject.Find("GameManager").GetComponent<GameData>();
        }

        private void Update()
        {
            if (!m_shootBubble)
                m_shootBubble = m_gameData.RandomANewBubble(m_headAnchor.position);
            
            // Move the bubble to target slot
            if (m_isShootBubbleMoving)
            {
                MoveBubbleToTargetSlot();
            }
            else
            {
                // Hold the left mouse button => aiming
                if (Input.GetMouseButton(0))
                {
                    CalculateAimingAngle();
                    CalulateAimingLineTurningPoints();
                }
                else  // Cancel holding => reset
                {
                    m_aimingLine.positionCount = 0;
                    m_headAnchor.eulerAngles = Vector3.zero;
                }
            
                // Left mouse button up => shoot
                if (Input.GetMouseButtonUp(0))
                {
                    m_isShootBubbleMoving = true;
                    m_currentTargetIndex = 1;
                }
            }
        }

        private void CalculateAimingAngle()
        {
            // Get mouse position
            m_mouseWorldPos = m_mainCam.ScreenToWorldPoint(Input.mousePosition);
            // Calculate direction and constrain the angle between -60 and 60
            m_direction = new Vector3((m_mouseWorldPos - m_headAnchor.position).x, (m_mouseWorldPos - m_headAnchor.position).y, 0);
            m_angle = Vector3.Angle(Vector3.up, m_direction);
            m_angle = m_direction.x > 0 ? -m_angle : m_angle;
            if (m_angle < -60f)
            {
                m_angle = -60f;
                m_direction = new Vector3(2, 1, 0);
            }
            else if (m_angle > 60f)
            {
                m_angle = 60f;
                m_direction = new Vector3(-2, 1, 0);
            }
            // Turn the shooter head the specific angle
            m_headAnchor.eulerAngles = Vector3.forward * m_angle;
        }

        private void CalulateAimingLineTurningPoints()
        {
            // Calculate aiming line turning points
            m_turningPoints = new List<Vector3>();
            // Add the init point
            m_turningPoints.Add(m_headAnchor.position);
            
            // Find all the turning points
            while (FindTurningPoints(m_turningPoints[m_turningPoints.Count - 1], out bool hasReachedEnd))
            {
                if (hasReachedEnd)
                    break;
            }

            // Draw aiming line
            m_aimingLine.positionCount = m_turningPoints.Count;
            for (int i = 0; i < m_turningPoints.Count; i++)
                m_aimingLine.SetPosition(i, m_turningPoints[i]);
        }

        private bool FindTurningPoints(Vector3 startPoint, out bool hasReachedEnd)
        {
            hasReachedEnd = false;
            
            // Filled bubble
            RaycastHit2D nearestFilledBubbleHitInfo =  Physics2D.Raycast(startPoint, m_direction.normalized, 50, LayerMask.GetMask("FilledBubble"));
            if (nearestFilledBubbleHitInfo.collider)
            {
                m_turningPoints.Add(nearestFilledBubbleHitInfo.point);

                // Find the farthest empty bubble slot as target
                RaycastHit2D emptyBubbleSlotHitInfo = Physics2D.Raycast(nearestFilledBubbleHitInfo.point, -m_direction.normalized, 10, LayerMask.GetMask("Bubble"));
                if (emptyBubbleSlotHitInfo.collider)
                    m_shootTargetBubbleSlot = emptyBubbleSlotHitInfo.collider.gameObject.GetComponent<BubbleData>();
                
                hasReachedEnd = true;
                return true;
            }
            
            // Top
            RaycastHit2D topHitInfo = Physics2D.Raycast(m_turningPoints[m_turningPoints.Count - 1], m_direction.normalized, 50, LayerMask.GetMask("TopBoundary"));
            if (topHitInfo.collider)
            {
                m_turningPoints.Add(topHitInfo.point);
                
                // Find the farthest empty bubble slot as target
                RaycastHit2D emptyBubbleSlotHitInfo = Physics2D.Raycast(topHitInfo.point, -m_direction.normalized, 10, LayerMask.GetMask("Bubble"));
                if (emptyBubbleSlotHitInfo.collider)
                    m_shootTargetBubbleSlot = emptyBubbleSlotHitInfo.collider.gameObject.GetComponent<BubbleData>();
                
                hasReachedEnd = true;
                return true;
            }
            
            // Vertical
            RaycastHit2D hitInfo = Physics2D.Raycast(startPoint, m_direction.normalized, 50, LayerMask.GetMask("VerticalBoundary"));
            if (hitInfo.collider)
            {
                m_turningPoints.Add(hitInfo.point.x > 0 ? hitInfo.point - Vector2.one * 0.01f : hitInfo.point + Vector2.one * 0.01f);
                m_direction = Vector2.Reflect(m_direction, hitInfo.normal);
                return true;
            }
            
            return false;
        }

        private void MoveBubbleToTargetSlot()
        {
            if (Vector3.Distance(m_shootBubble.transform.position, m_shootTargetBubbleSlot.transform.position) >= m_gameData.bubbleRadius)
            {
                if (Vector3.Distance(m_shootBubble.transform.position, m_turningPoints[m_currentTargetIndex]) >= 0.1f)
                {
                    // Move towards turning point
                    m_shootBubble.transform.Translate((m_turningPoints[m_currentTargetIndex] - m_shootBubble.transform.position).normalized * Time.deltaTime * m_shootSpeed);
                }
                else
                {
                    // Update index
                    m_shootBubble.transform.position = m_turningPoints[m_currentTargetIndex];
                    m_currentTargetIndex++;
                }
            }
            else
            {
                // Has reach
                m_isShootBubbleMoving = false;
                m_shootTargetBubbleSlot.Value = m_shootBubble.GetComponent<BubbleData>().Value;
                Destroy(m_shootBubble);
                m_shootBubble = null;

                m_gameData.CheckBubbleCombo(m_shootTargetBubbleSlot);
            }
        }
    }
}
