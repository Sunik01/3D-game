
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    public int damage = 20;
    public float damageCooldown = 1f;

    private float nextDamageTime;

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player") &&
            !other.transform.root.CompareTag("Player"))
            return;

        if (Time.time < nextDamageTime)
            return;

        PlayerHealth health =
            other.GetComponentInParent<PlayerHealth>();

        if (health == null)
            health = other.GetComponent<PlayerHealth>();

        if (health != null)
        {
            health.TakeTrapDamage(damage);
            nextDamageTime = Time.time + damageCooldown;
        }
    }
}
