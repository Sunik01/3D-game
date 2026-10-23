
using UnityEngine;

public class PushZombieStun : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerStun stun = other.GetComponent<PlayerStun>();

        if (stun == null)
            stun = other.GetComponentInParent<PlayerStun>();

        if (stun != null)
        {
            stun.Stun();
            Debug.Log("Push Zombie: Player stunned for 1 second!");
        }
        else
        {
            Debug.LogWarning("Player does not have PlayerStun!");
        }
    }
}
