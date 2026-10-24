
using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Detection")]
    public float detectionRange = 20f;
    public float attackDistance = 1.8f;

    [Header("Movement")]
    public float moveSpeed = 3.5f;

    [Header("Attack")]
    public int attackDamage = 10;
    public float attackCooldown = 1.5f;

    private PlayerHealth playerHealth;
    private NavMeshAgent agent;
    private float nextAttackTime;
    private bool isDead;
    private bool hasStunnedPlayer = false;
    public bool stunOnly = false;



    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("Zombie ?????? NavMeshAgent!", this);
            enabled = false;
            return;
        }

        agent.speed = moveSpeed;
        agent.angularSpeed = 360f;
        agent.stoppingDistance = attackDistance * 0.8f;

        if (player == null)
        {
            GameObject foundPlayer = GameObject.Find("Player");

            if (foundPlayer != null)
                player = foundPlayer.transform;
        }

        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();

            if (playerHealth == null)
                playerHealth =
                    player.GetComponentInChildren<PlayerHealth>();
        }
    }

    private void Update()
    {
        PlayerStun stun = GetComponent<PlayerStun>();

        if (stun != null && stun.IsStunned)
            return;

        if (isDead || player == null || agent == null)
            return;

        if (!agent.isOnNavMesh)
            return;
        // Push Zombie ?????????????????????? ???????????????
        if (stunOnly)
        {
            if (agent.isOnNavMesh)
            {
                float distanceToPlayer =
                    Vector3.Distance(transform.position, player.position);

                if (distanceToPlayer <= detectionRange)
                {
                    agent.isStopped = false;
                    agent.SetDestination(player.position);
                }
                else
                {
                    agent.isStopped = true;
                    agent.ResetPath();
                }
            }

            return;
        }
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        // ?????????????????????????: ??????????
        if (distance > detectionRange)
        {
            agent.isStopped = true;
            agent.ResetPath();
            return;
        }

        // ?????????????????: ?????????????
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                agent.angularSpeed * Time.deltaTime
            );
        }

        // ???????????????????: ??????
        if (distance > attackDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        // ????????????: ????????????????
        else
        {
            agent.isStopped = true;
            AttackPlayer();
        }
    }

    private void AttackPlayer()
    {
        if (Time.time < nextAttackTime)
            return;

        nextAttackTime = Time.time + attackCooldown;

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
            Debug.Log("Zombie attacked player!");
        }
    }

    public void StopAI()
    {
        isDead = true;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!stunOnly)
            return;

        // ?????????? Collider ???????????? Player
        if (!other.CompareTag("Player") &&
            other.transform.root.CompareTag("Player") == false)
            return;

        PlayerStun stun = other.GetComponentInParent<PlayerStun>();

        if (stun != null)
        {
            stun.Stun();
            Debug.Log("Push Zombie stunned Player for 1 second!");
        }
        else
        {
            Debug.LogWarning("PlayerStun component not found on Player!");
        }
    }
}