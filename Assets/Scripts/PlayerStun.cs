
using UnityEngine;
using System.Collections;

public class PlayerStun : MonoBehaviour
{
    public float stunDuration = 1f;

    private bool isStunned = false;
    private Coroutine stunCoroutine;

    public bool IsStunned => isStunned;

    public void Stun()
    {
        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);

        stunCoroutine = StartCoroutine(StunRoutine());
    }

    private IEnumerator StunRoutine()
    {
        isStunned = true;

        yield return new WaitForSeconds(stunDuration);

        isStunned = false;
        stunCoroutine = null;
    }
}
