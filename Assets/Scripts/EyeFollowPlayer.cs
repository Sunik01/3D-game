
using UnityEngine;

public class EyeFollowPlayer : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Eye Settings")]
    public float rotationSpeed = 5f;
    public bool onlyRotateHorizontally = false;

    private void Start()
    {
        // ค้นหาผู้เล่นอัตโนมัติ หากยังไม่ได้กำหนด
        if (player == null)
        {
            GameObject foundPlayer = GameObject.Find("Player");

            if (foundPlayer != null)
                player = foundPlayer.transform;
        }
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        Vector3 direction = player.position - transform.position;

        // ถ้าต้องการให้หมุนเฉพาะซ้าย-ขวา
        if (onlyRotateHorizontally)
            direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}
