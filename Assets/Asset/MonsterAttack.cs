using UnityEngine;

public class MonsterAttackSimple : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Attack Range")]
    [Tooltip("공격을 시도하는 거리")]
    [SerializeField] private float attackTryRange = 3f;

    [Tooltip("실제로 공격이 성공하는 거리")]
    [SerializeField] private float attackHitRange = 1.5f;

    [Header("Attack Delay")]
    [Tooltip("공격 시전 시간")]
    [SerializeField] private float attackDelay = 2.0f;

    [Header("Cooldown")]
    [Tooltip("공격 쿨타임")]
    [SerializeField] private float attackCooldown = 5f;

    private bool isAttacking = false;
    private float attackStartTime = 0f;
    private float lastAttackTime = -999f;

    private void Update()
    {
        if (player == null)
            return;

        float time = Time.time;
        
        if (isAttacking)
        {
            if (time >= attackStartTime + attackDelay)
            {
                FinishAttack();
            }

            return;
        }

        Vector3 offset = player.position - transform.position;
        float sqrDistance = offset.sqrMagnitude;
        float tryRangeSqr = attackTryRange * attackTryRange;
        float hitRangeSqr = attackHitRange * attackHitRange;

        if (sqrDistance > tryRangeSqr)
        {
            return;
        }

        bool isCooldown = time < lastAttackTime + attackCooldown;

        if (isCooldown)
        {
            Debug.Log("공격 쿨타임");
            return;
        }

        StartAttack();
    }

    private void StartAttack()
    {
        isAttacking = true;
        attackStartTime = Time.time;

        Debug.Log("공격 중");
    }

    private void FinishAttack()
    {
        isAttacking = false;

        Vector3 offset = player.position - transform.position;
        float sqrDistance = offset.sqrMagnitude;

        float hitRangeSqr = attackHitRange * attackHitRange;

        if (sqrDistance <= hitRangeSqr)
        {
            Debug.Log("공격 성공!");
        }
        else
        {
            Debug.Log("공격 실패");
        }

        lastAttackTime = Time.time;
    }
}