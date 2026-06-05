using UnityEngine;

public class MonsterStateGizmo : MonoBehaviour
{
    private enum MonsterState
    {
        Idle,//대기
        detect,//발견
        Chase,//추적
        Attack//공격
    }

    [Header("Target")]
    [Tooltip("해당 몬스터가 쫓을 타겟")]
    [SerializeField] private Transform player;

    [Header("Distance")]
    [Tooltip("몬스터가 타겟을 감지할수 있는 범위")]
    [SerializeField] private float idleDistance = 0f;
    [SerializeField] private float detectDistance = 10f;
    [SerializeField] private float chaseDistance = 6f;
    [SerializeField] private float attackDistance = 1.8f;

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

    private MonsterState currentState = MonsterState.Idle;

    private void Update()
    {
        if (player == null)//플레이어 없으면
        {
            currentState = MonsterState.Idle;
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        bool canSeePlayer = CanSeePlayer();

        if (!canSeePlayer)
        {
            currentState = MonsterState.Idle;
            return;
        }
        if (distance <= attackDistance)//공격 상태일 때
        {
            currentState = MonsterState.Attack;

            RotateToPlayer();
        }
        else if (distance <= chaseDistance)//추적 상태일 때
        {
            currentState = MonsterState.Chase;

            detectTimer = detectStayTime;//바로 시간 초과

            RotateToPlayer();

            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                moveSpeed * Time.deltaTime
            );
        }
        else if (distance <= detectDistance)//발견 상태일 때
        {
            RotateToPlayer();

            detectTimer += Time.deltaTime;//실시간으로 발견 시간 증가

            if (detectTimer >= detectStayTime)//발견 시간이 설정 시간보다 길 경우
            {
                currentState = MonsterState.Chase;//추적 상태로 변경

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    player.position,
                    moveSpeed * Time.deltaTime
                );
            }
            else//설정 시간보다 짧으면
            {
                currentState = MonsterState.detect;//발견 상태 유지
            }
        }
        else//그 외
        {
            detectTimer = 0f;//발견 시간을 0으로 초기화
            currentState = MonsterState.Idle;
        }
    }

    private bool CanSeePlayer()
    {
        if (player == null)
            return false;

        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 target = player.position + Vector3.up * 0.5f;

        Vector3 directionToPlayer =
            (target - origin).normalized;

        float distance =
            Vector3.Distance(origin, target);

        // 내적 계산
        float dot = Vector3.Dot(
            transform.forward,
            directionToPlayer
        );

        // 시야각 절반 기준
        float limit = Mathf.Cos(viewAngle * 0.5f * Mathf.Deg2Rad);

        if (dot < limit)
            return false;

        // 벽에 가려졌는지 확인
        if (Physics.Raycast(origin, directionToPlayer,
            out RaycastHit hit, distance))
        {
            return hit.transform == player;
        }

        return false;
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
        Gizmos.DrawWireSphere(transform.position, detectDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        if (player == null)
            return;

        Gizmos.color = Color.blue;

        float halfAngle = viewAngle * 0.5f;
        int segments = 20;

        Vector3 previousPoint = transform.position + Quaternion.Euler(0, -halfAngle, 0) * transform.forward * detectDistance;

        for (int i = 1; i <= segments; i++)
        {
            float angle = Mathf.Lerp(-halfAngle, halfAngle, i / (float)segments);

            Vector3 currentPoint = transform.position + Quaternion.Euler(0, angle, 0) * transform.forward * detectDistance;

            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }

        Vector3 leftBoundary = Quaternion.Euler(0, -halfAngle, 0) * transform.forward;

        Vector3 rightBoundary = Quaternion.Euler(0, halfAngle, 0) * transform.forward;

        Gizmos.DrawRay(transform.position, leftBoundary * detectDistance);

        Gizmos.DrawRay(transform.position, rightBoundary * detectDistance);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, player.position);
    }

    private Color GetStateColor()
    {
        switch (currentState)
        {
            case MonsterState.detect://발견
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