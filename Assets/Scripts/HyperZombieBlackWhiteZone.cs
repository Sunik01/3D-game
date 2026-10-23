
using UnityEngine;
using System.Collections;

public class HyperZombieBlackWhiteZone : MonoBehaviour
{
    public float duration = 4.5f;
    public float radius = 3f;
    public float damage = 5f;
    public float damageInterval = 0.5f;

    private Transform player;
    private PlayerHealth playerHealth;
    private bool running;
    private Vector3 fixedPosition;

    public void Setup(Transform target)
    {
        player = target;
        fixedPosition = transform.position;

        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();

            if (playerHealth == null)
                playerHealth = player.GetComponentInChildren<PlayerHealth>();
        }

        StartCoroutine(ZoneRoutine());
    }

    private IEnumerator ZoneRoutine()
    {
        running = true;

        float elapsed = 0f;
        float damageTimer = 0f;

        while (elapsed < duration)
        {
            transform.position = fixedPosition;

            if (player != null && playerHealth != null)
            {
                Vector3 closest = player.position;
                closest.y = transform.position.y;

                float distance = Vector3.Distance(
                    closest, transform.position
                );

                if (distance <= radius)
                {
                    damageTimer += Time.deltaTime;

                    if (damageTimer >= damageInterval)
                    {
                        playerHealth.TakeDamage(Mathf.RoundToInt(damage));
                        damageTimer = 0f;
                    }
                }
                else
                {
                    damageTimer = 0f;
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
