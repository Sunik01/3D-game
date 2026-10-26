using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BossArenaTrigger : MonoBehaviour
{
    private void Reset() { GetComponent<Collider>().isTrigger = true; }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") && !other.transform.root.CompareTag("Player")) return;
        if (BossFightManager.Instance != null) BossFightManager.Instance.PlayerEnteredArena();
    }
}
