
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour

{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Health Regeneration")]
    public float healInterval = 2f;

    private float healTimer = 0f;
    private bool isDead = false;

    void Start()
    {
        Time.timeScale = 1f;

        string selectedClass =
            PlayerPrefs.GetString("SelectedClass", "Normal");

        switch (selectedClass)
        {
            case "Runner":
                maxHealth = 75;
                healInterval = 2f;
                break;

            case "Normal":
                maxHealth = 100;
                healInterval = 2f;
                break;

            case "Tank":
                maxHealth = 160;
                healInterval = 2f;
                break;

            case "Soft":
                maxHealth = 50;
                healInterval = 2f;
                break;

            case "HealthEmpire":
                maxHealth = 400;
                healInterval = 10f;
                break;

            case "Elite":
                maxHealth = 125;
                healInterval = 2f;
                break;
            case "Vampire":
                maxHealth = 120;
                break;

            case "GodSword":
                maxHealth = 140;
                break;

            case "Richman":
                maxHealth = 180;
                break;  
        }

        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        // GODMODE: ป้องกันความเสียหายทั่วไป
        if (PlayerPrefs.GetInt("GodModeUnlocked", 0) == 1)
            return;

        if (PlayerPotionEffects.Instance != null)
        {
            damage = Mathf.CeilToInt(
                damage *
                PlayerPotionEffects.Instance.DamageTakenMultiplier
            );
        }

        currentHealth = Mathf.Clamp(
            currentHealth - damage, 0, maxHealth
        );

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Kill()
    {
        if (isDead) return;

        currentHealth = 0;
        Die();
    }
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }


    void Die()
    {
        isDead = true;
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        // ฟื้นฟูเลือด 1% ทุก 2 วินาที
        if (!isDead && currentHealth < maxHealth)
        {
            healTimer += Time.deltaTime;

            if (healTimer >= healInterval)
            {
                int healAmount = Mathf.Max(
                    1,
                    Mathf.CeilToInt(maxHealth * 0.01f)
                );

                currentHealth = Mathf.Min(
                    currentHealth + healAmount,
                    maxHealth
                );

                healTimer = 0f;
            }
        }

        // กด R เพื่อเริ่มเกมใหม่
        if (isDead && Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1f;

            SceneManager.LoadScene(
                SceneManager.GetActiveScene().buildIndex
            );
        }
    }

    void OnGUI()
    {
        // ขอบจอสีแดงเมื่อเลือดต่ำกว่า 25
        if (currentHealth < 25)
        {
            float intensity = 0.35f +
                Mathf.PingPong(
                    Time.unscaledTime * 0.5f, 0.3f
                );

            float border = 55f;

            GUI.color = new Color(
                1f, 0f, 0f, intensity
            );

            GUI.DrawTexture(
                new Rect(0, 0, Screen.width, border),
                Texture2D.whiteTexture
            );

            GUI.DrawTexture(
                new Rect(0, Screen.height - border,
                Screen.width, border),
                Texture2D.whiteTexture
            );

            GUI.DrawTexture(
                new Rect(0, 0, border, Screen.height),
                Texture2D.whiteTexture
            );

            GUI.DrawTexture(
                new Rect(Screen.width - border, 0,
                border, Screen.height),
                Texture2D.whiteTexture
            );

            GUI.color = Color.white;
        }

        // แสดงแถบเลือด
        GUI.Box(new Rect(20, 20, 220, 65), "");

        GUI.Label(
            new Rect(30, 25, 200, 25),
            "HEALTH: " + currentHealth +
            " / " + maxHealth
        );

        float healthPercent =
            (float)currentHealth / maxHealth;

        // พื้นหลังหลอดเลือด
        GUI.color = Color.gray;

        GUI.DrawTexture(
            new Rect(30, 53, 190, 12),
            Texture2D.whiteTexture
        );

        // สีหลอดเลือด
        GUI.color = healthPercent < 0.25f
            ? Color.red : Color.green;

        GUI.DrawTexture(
            new Rect(30, 53,
            190 * healthPercent, 12),
            Texture2D.whiteTexture
        );

        GUI.color = Color.white;

        // หน้าจอ GAME OVER
        if (isDead)
        {
            GUI.color = new Color(
                0f, 0f, 0f, 0.75f
            );

            GUI.DrawTexture(
                new Rect(0, 0,
                Screen.width, Screen.height),
                Texture2D.whiteTexture
            );

            GUI.color = Color.white;

            GUIStyle titleStyle = new GUIStyle();
            titleStyle.fontSize = 60;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment =
                TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = Color.red;

            GUI.Label(
                new Rect(0, Screen.height / 2 - 100,
                Screen.width, 90),
                "GAME OVER", titleStyle
            );

            GUIStyle restartStyle = new GUIStyle();
            restartStyle.fontSize = 32;
            restartStyle.fontStyle = FontStyle.Bold;
            restartStyle.alignment =
                TextAnchor.MiddleCenter;
            restartStyle.normal.textColor = Color.white;

            GUI.Label(
                new Rect(0, Screen.height / 2 + 10,
                Screen.width, 60),
                "Press R to Restart", restartStyle
            );
        }
    }
}
