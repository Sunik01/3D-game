
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -20f;

    private CharacterController controller;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // ตรวจสอบพื้นก่อน
        bool grounded = controller.isGrounded;

        if (grounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        // เดินด้วย WASD
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move =
            transform.right * x +
            transform.forward * z;

        move = Vector3.ClampMagnitude(move, 1f);

        // กระโดดด้วย Spacebar
        if (grounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = Mathf.Sqrt(
                jumpHeight * -2f * gravity
            );
        }

        // แรงโน้มถ่วง
        velocity.y += gravity * Time.deltaTime;

        // รวมการเดินและกระโดด
        Vector3 finalMove =
            move * moveSpeed + velocity;

        controller.Move(finalMove * Time.deltaTime);
    }
}
