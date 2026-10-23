
using UnityEngine;
using UnityEngine.AI;

public class HyperZombieRangedAttack : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Ranged Attack")]
    public float attackRange = 15f;
    public float attackCooldown = 6f;
    public float spawnDistance = 4f;

    [Header("Zone")]
    public float zoneDuration = 4.5f;
    public float zoneRadius = 3f;
    public float damagePerTick = 5f;
    public float damageInterval = 0.5f;

    private float nextAttackTime;
    private NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            GameObject found = GameObject.Find("Player");
            if (found != null)
                player = found.transform;
        }
    }

    private void Update()
    {
        if (player == null || Time.time < nextAttackTime)
            return;

        float distance = Vector3.Distance(
            transform.position, player.position
        );

        if (distance > attackRange)
            return;

        nextAttackTime = Time.time + attackCooldown;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        CastAttack();
    }

    private void CastAttack()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction);

        // สร้างวงตรงตำแหน่งผู้เล่น ณ เวลาที่เริ่มโจมตี
        Vector3 targetPosition = player.position;
        targetPosition.y = transform.position.y;

        GameObject zone = new GameObject("Hyper Zombie Damage Zone");
        zone.transform.position = targetPosition;

        var visual = zone.AddComponent<HyperZombieZoneVisual>();
        visual.radius = zoneRadius;

        var damageZone = zone.AddComponent<HyperZombieBlackWhiteZone>();
        damageZone.duration = zoneDuration;
        damageZone.radius = zoneRadius;
        damageZone.damage = damagePerTick;
        damageZone.damageInterval = damageInterval;
        damageZone.Setup(player);

        Debug.Log("Hyper Zombie used ranged zone attack!");
    }
}
