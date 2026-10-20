
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    public int damage = 20;
    public float damageCooldown = 1f;

    private float nextDamageTime = 0f;

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (Time.time < nextDamageTime) return;

        PlayerHealth health =
            other.GetComponent<PlayerHealth>();

        if (health == null)
        {
            health = other.GetComponentInParent<PlayerHealth>();
        }

        if (health != null)
        {
            health.TakeDamage(damage);
            nextDamageTime = Time.time + damageCooldown;
        }
    }
}
