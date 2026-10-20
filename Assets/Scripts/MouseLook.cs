
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public Transform playerBody;
    public float mouseSensitivity = 200f;

    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X")
                     * mouseSensitivity * Time.deltaTime;

        float mouseY = Input.GetAxis("Mouse Y")
                     * mouseSensitivity * Time.deltaTime;

        // หมุนกล้องขึ้นและลง
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);

        transform.localRotation =
            Quaternion.Euler(xRotation, 0f, 0f);

        // หมุนตัวละครซ้ายและขวา
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
