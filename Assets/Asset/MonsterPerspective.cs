using System.Collections.Generic;
using UnityEngine;

public class Monsterperspective : MonoBehaviour
{
    private enum MonsterState
    {
        Patrol,//대기
        Detect,//발견
        Chase,//추적
        Attack//공격
    }

    [Header("Target")]
    [Tooltip("해당 몬스터가 쫓을 타겟")]
    [SerializeField] private Transform player;

    [Header("Distance")]
    [Tooltip("몬스터가 타겟을 감지할수 있는 범위")]
    [SerializeField] private float PatrolDistance = 0f;
    [SerializeField] private float DetectDistance = 10f;
    [SerializeField] private float ChaseDistance = 6f;
    [SerializeField] private float AttackDistance = 3f;

    [Header("View")]
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private float rotateSpeed = 5f;

    [Header("Time")]
    [Tooltip("인식 범위에 들어간 뒤 몬스터가 추적으로 바뀌기까지 걸리는 시간")]
    [SerializeField] private float detectStayTime = 5f;

    private float detectTimer = 0f;

    [Header("Movement")]
    [Tooltip("몬스터의 이동 속도")]
    [SerializeField] private float moveSpeed = 2f;

    [SerializeField] private LayerMask obstacleMask;

    private MonsterState currentState = MonsterState.Patrol;

    [SerializeField] private List<Vector3> patrolPoints = new();

    [SerializeField] private float arriveDistance = 0.2f;

    private int currentIndex = 0;

    private void Update()
    {
        if (player == null)//플레이어 없으면
        {
            currentState = MonsterState.Patrol;
            Patrol();
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        bool canSeePlayer = CanSeePlayer();

        if (distance <= AttackDistance && canSeePlayer)//공격 상태일 때
        {
            currentState = MonsterState.Attack;

            RotateToPlayer();

            return;
        }        

        else if (distance <= ChaseDistance && canSeePlayer)//추적 상태일 때
        {
            currentState = MonsterState.Chase;

            detectTimer = detectStayTime;//바로 시간 초과

            RotateToPlayer();

            transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

            return;
        }

        else if (distance <= DetectDistance && canSeePlayer)//발견 상태일 때
        {
            RotateToPlayer();

            detectTimer += Time.deltaTime;//실시간으로 발견 시간 증가

            if (detectTimer >= detectStayTime)//발견 시간이 설정 시간보다 길 경우
            {
                currentState = MonsterState.Chase;//추적 상태로 변경

                transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            }
            else//설정 시간보다 짧으면
            {
                currentState = MonsterState.Detect;//발견 상태 유지
            }
        }

        else//그 외
        {
            detectTimer = 0f;//발견 시간을 0으로 초기화
            currentState = MonsterState.Patrol;
            Patrol();
        }
    }

    private void Patrol()
    {
        if (patrolPoints.Count == 0)
            return;

        Vector3 targetPos = patrolPoints[currentIndex];

        // 이동
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        // 회전
        Vector3 direction = targetPos - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }

        // 도착 판정
        if ((transform.position - targetPos).sqrMagnitude <= arriveDistance * arriveDistance)
        {
            currentIndex++;

            if (currentIndex >= patrolPoints.Count)
                currentIndex = 0;
        }
    }

    private bool CanSeePlayer()
    {
        if (player == null)
            return false;

        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 target = player.position + Vector3.up * 0.5f;

        Vector3 direction = (target - origin);
        float distance = direction.magnitude;
        direction /= distance;

        if (distance > DetectDistance)
            return false;

        float dot = Vector3.Dot(transform.forward, direction);
        float limit = Mathf.Cos(viewAngle * 0.5f * Mathf.Deg2Rad);

        if (dot < limit)
            return false;

        if (Physics.Raycast(origin, direction, distance, obstacleMask))
        {
            return false;
        }

        return true;
    }

    private void RotateToPlayer()
    {
        if (player == null)
            return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = GetStateColor();
        Gizmos.DrawSphere(transform.position, 0.35f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, DetectDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, ChaseDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AttackDistance);

        if (player == null)
            return;

        Gizmos.color = Color.blue;

        float halfAngle = viewAngle * 0.5f;
        int segments = 20;

        Vector3 previousPoint = transform.position + Quaternion.Euler(0, -halfAngle, 0) * transform.forward * DetectDistance;

        for (int i = 1; i <= segments; i++)
        {
            float angle = Mathf.Lerp(-halfAngle, halfAngle, i / (float)segments);

            Vector3 currentPoint = transform.position + Quaternion.Euler(0, angle, 0) * transform.forward * DetectDistance;

            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }

        Vector3 leftBoundary = Quaternion.Euler(0, -halfAngle, 0) * transform.forward;

        Vector3 rightBoundary = Quaternion.Euler(0, halfAngle, 0) * transform.forward;

        Gizmos.DrawRay(transform.position, leftBoundary * DetectDistance);

        Gizmos.DrawRay(transform.position, rightBoundary * DetectDistance);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, player.position);
    }

    private Color GetStateColor()
    {
        switch (currentState)
        {
            case MonsterState.Detect://발견
                return Color.green;
            case MonsterState.Chase://추적
                return Color.yellow;
            case MonsterState.Attack://공격
                return Color.red;
            default:
                return Color.gray;//그 외
        }
    }
}