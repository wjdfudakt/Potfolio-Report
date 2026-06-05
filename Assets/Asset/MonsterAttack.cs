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

    [Header("Cooldown")]
    [Tooltip("공격 쿨타임")]
    [SerializeField] private float attackCooldown = 5f;

    private float lastAttackTime = -999f;

    private void Update()
    {
        if (player == null)
            return;

        Vector3 offset = player.position - transform.position;
        float sqrDistance = offset.sqrMagnitude;

        float tryRangeSqr = attackTryRange * attackTryRange;
        float hitRangeSqr = attackHitRange * attackHitRange;

        float time = Time.time;

        if (sqrDistance > tryRangeSqr)
        {
            return;
        }

        Debug.Log("공격 시도 범위 안");

        bool isCooldown = time < lastAttackTime + attackCooldown;

        if (isCooldown)
        {
            Debug.Log("쿨타임 중");
            return;
        }

        if (sqrDistance <= hitRangeSqr)
        {
            Debug.Log("공격 성공!");

            lastAttackTime = time;
        }
        else
        {
            Debug.Log("공격 시도했지만 거리 부족");
        }
    }
}