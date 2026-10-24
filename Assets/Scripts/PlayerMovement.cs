
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -20f;

    private CharacterController controller;
    private Vector3 velocity;
    private string selectedClass;

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        selectedClass = PlayerPrefs.GetString(
            "SelectedClass", "Normal");

        switch (selectedClass)
        {
            case "Runner":
                moveSpeed = 8f;
                break;

            case "Normal":
                moveSpeed = 6f;
                break;

            case "Tank":
                moveSpeed = 4f;
                break;

            case "Soft":
                moveSpeed = 9f;
                jumpHeight = 5f;
                gravity = -10f;
                break;

            case "HealthEmpire":
                moveSpeed = 5f;
                break;

            case "Elite":
                moveSpeed = 7f;
                break;

            case "Vampire":
                moveSpeed = 6f;
                break;

            case "GodSword":
                moveSpeed = 6.5f;
                break;

            case "Richman":
                moveSpeed = 8f;
                break;

            case "MazeRunner":
                moveSpeed = 10f;
                break;

            case "Reaper":
                moveSpeed = 6f;
                break;

            case "Magician":
                moveSpeed = 7.5f;
                break;
        }

        if (PlayerPrefs.GetInt("SpeedJumpUnlocked", 0) == 1)
        {
            moveSpeed = 20f;
            jumpHeight = 8f;
        }
    }

    private void Update()
    {
        if (controller == null)
            return;

        PlayerStun stun = GetComponent<PlayerStun>();

        if (stun != null && stun.IsStunned)
            return;

        bool grounded = controller.isGrounded;

        if (grounded && velocity.y < 0f)
            velocity.y = -2f;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move =
            transform.right * x +
            transform.forward * z;

        move = Vector3.ClampMagnitude(move, 1f);

        if (grounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = Mathf.Sqrt(
                jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        // โบนัสความเร็วจากน้ำยา
        float potionMultiplier = 1f;

        if (PlayerPotionEffects.Instance != null)
        {
            potionMultiplier =
                PlayerPotionEffects.Instance.SpeedMultiplier;
        }

        // ค้นหาระบบคลาสของผู้เล่น
        PlayerClassSystem classSystem =
            GetComponent<PlayerClassSystem>();

        if (classSystem == null)
            classSystem = GetComponentInParent<PlayerClassSystem>();

        if (classSystem == null)
            classSystem = GetComponentInChildren<PlayerClassSystem>();

        if (classSystem == null)
            classSystem = PlayerClassSystem.Instance;

        // โบนัสความเร็ว Reaper ตามจำนวน Kill
        float reaperBonus = 0f;

        if (classSystem != null)
            reaperBonus = classSystem.SpeedBonus;

        // โบนัสความเร็วจาก Magician
        MagicianSpellController magician =
            GetComponent<MagicianSpellController>();

        if (magician == null)
            magician = GetComponentInChildren<MagicianSpellController>();

        float magicianSpeedMultiplier = 1f;

        if (magician != null)
            magicianSpeedMultiplier = magician.SpeedMultiplier;

        // คำนวณการเคลื่อนที่
        float finalSpeed =
            (moveSpeed + reaperBonus) *
            potionMultiplier *
            magicianSpeedMultiplier;

        Vector3 finalMove =
            move * finalSpeed + velocity;

        controller.Move(finalMove * Time.deltaTime);
    }
}
