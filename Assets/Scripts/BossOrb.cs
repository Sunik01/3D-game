using UnityEngine;

public class BossOrb : MonoBehaviour
{
    private bool collected;

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player") && !other.transform.root.CompareTag("Player")) return;
        collected = true;
        if (BossFightManager.Instance != null) BossFightManager.Instance.CollectRedOrb();
        Destroy(gameObject);
    }
}
