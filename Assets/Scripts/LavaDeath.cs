
using UnityEngine;

public class LavaDeath : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerHealth health =
            other.GetComponent<PlayerHealth>();

        if (health == null)
        {
            health = other.GetComponentInParent<PlayerHealth>();
        }

        if (health != null)
        {
            health.Kill();
        }
    }
}
