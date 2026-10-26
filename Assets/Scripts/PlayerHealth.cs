
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Health Regeneration")]
    public float healInterval = 2f;

    private float healTimer;
    private bool isDead;

    private string selectedClass = "Normal";

    private void Start()
    {
        Time.timeScale = 1f;

        selectedClass =
            PlayerPrefs.GetString("SelectedClass", "Normal");

        switch (selectedClass)
        {
            case "Runner": maxHealth = 75; break;
            case "Normal": maxHealth = 100; break;
            case "Tank": maxHealth = 160; break;
            case "Soft": maxHealth = 50; break;
            case "HealthEmpire":
                maxHealth = 400;
                healInterval = 10f;
                break;
            case "Elite": maxHealth = 125; break;
            case "Vampire": maxHealth = 120; break;
            case "GodSword": maxHealth = 140; break;
            case "Richman": maxHealth = 180; break;

            case "MazeRunner":
                maxHealth = 80;
                break;

            case "Reaper":
                maxHealth = 100;
                break;

            case "Magician":
                maxHealth = 150;
                break;
        }

        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("[PlayerHealth] TakeDamage called: " + damage);
        ApplyDamage(damage, false, false);
    }

    // ใช้สำหรับกับดักโดยเฉพาะ
    public void TakeTrapDamage(int damage)
    {
        ApplyDamage(damage, true, false);
    }

    // ใช้สำหรับวงโจมตีของ Hyper Zombie
    public void TakeHyperZoneDamage(int damage)
    {
        if (selectedClass == "Magician")
            return;

        ApplyDamage(damage, false, true);

    }

    private void ApplyDamage(
        int damage, bool isTrap, bool isHyperZone)
    {
        if (isDead || damage <= 0)
            return;

        if (PlayerPrefs.GetInt("GodModeUnlocked", 0) == 1)
        {
            Debug.LogWarning("[PlayerHealth] God Mode is blocking damage!");
            return;
        }

        // Maze Runner ลดดาเมจจากกับดัก 80%
        // ไม่คูณซ้ำกับน้ำยาต้านความเสียหาย
        if (isTrap && selectedClass == "MazeRunner")
        {
            damage = Mathf.CeilToInt(damage * 0.2f);
        }
        else if (PlayerPotionEffects.Instance != null)
        {
            damage = Mathf.CeilToInt(
                damage *
                PlayerPotionEffects.Instance.DamageTakenMultiplier);

            MagicianSpellController magician =
                GetComponent<MagicianSpellController>();

            if (magician != null)
            {
                damage = Mathf.CeilToInt(
                    damage * magician.DamageTakenMultiplier);
            }
        }

        currentHealth = Mathf.Clamp(
            currentHealth - damage, 0, maxHealth);

        if (currentHealth <= 0)
            Die();
    }

    public void AddMaxHealth(int amount)
    {
        if (amount <= 0)
            return;

        maxHealth += amount;
        currentHealth = Mathf.Min(
            currentHealth + amount, maxHealth);
    }

    public void Heal(int amount)
    {
        if (isDead || amount <= 0)
            return;

        currentHealth = Mathf.Min(
            currentHealth + amount, maxHealth);
    }

    public void Kill()
    {
        if (isDead)
            return;

        currentHealth = 0;
        Die();
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    private void Update()
    {
        if (!isDead && currentHealth < maxHealth)
        {
            healTimer += Time.deltaTime;

            if (healTimer >= healInterval)
            {
                float regenPercent = 0.01f;

                // Reaper ฟื้นฟู 1% Max HP เท่านั้น
                // ไม่รวม RegenBonus จาก Kill Stack
                int healAmount = Mathf.Max(
                    1,
                    Mathf.CeilToInt(maxHealth * regenPercent)
                );

                // Magician ใช้ระบบฟื้นฟูเดิมของตัวเอง
                MagicianSpellController magician =
                    GetComponent<MagicianSpellController>();

                if (selectedClass == "Magician" &&
                    magician != null)
                {
                    healAmount = Mathf.Max(
                        1,
                        Mathf.CeilToInt(
                            healAmount * magician.RegenMultiplier
                        )
                    );
                }

                Heal(healAmount);
                healTimer = 0f;
            }
        }

        if (isDead && Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(
                SceneManager.GetActiveScene().buildIndex
            );
        }
    }


    private void OnGUI()
    {
        if (currentHealth < 25 && !isDead)
        {
            float intensity = 0.35f +
                Mathf.PingPong(
                    Time.unscaledTime * 0.5f, 0.3f);

            float border = 55f;
            GUI.color = new Color(1f, 0f, 0f, intensity);

            GUI.DrawTexture(
                new Rect(0, 0, Screen.width, border),
                Texture2D.whiteTexture);

            GUI.DrawTexture(
                new Rect(0, Screen.height - border,
                    Screen.width, border),
                Texture2D.whiteTexture);

            GUI.DrawTexture(
                new Rect(0, 0, border, Screen.height),
                Texture2D.whiteTexture);

            GUI.DrawTexture(
                new Rect(Screen.width - border, 0,
                    border, Screen.height),
                Texture2D.whiteTexture);

            GUI.color = Color.white;
        }

        GUI.Box(new Rect(20, 20, 220, 65), "");

        GUI.Label(
            new Rect(30, 25, 200, 25),
            "HEALTH: " + currentHealth + " / " + maxHealth);

        float percent = maxHealth > 0
            ? (float)currentHealth / maxHealth
            : 0f;

        GUI.color = Color.gray;
        GUI.DrawTexture(
            new Rect(30, 53, 190, 12),
            Texture2D.whiteTexture);

        GUI.color = percent < 0.25f
            ? Color.red : Color.green;

        GUI.DrawTexture(
            new Rect(30, 53, 190 * percent, 12),
            Texture2D.whiteTexture);

        GUI.color = Color.white;


        // แสดงคลาสที่ใช้อยู่ใต้แถบเลือด
        GUIStyle classStyle = new GUIStyle(GUI.skin.label);
        classStyle.fontSize = 16;
        classStyle.fontStyle = FontStyle.Bold;
        classStyle.normal.textColor = Color.cyan;
        classStyle.alignment = TextAnchor.MiddleLeft;

        string className =
            PlayerPrefs.GetString("SelectedClass", "Normal");

        GUI.Label(
            new Rect(30, 90, 220, 25),
            "CLASS: " + className.ToUpper(),
            classStyle
        );


        if (isDead)
        {
            GUI.color = new Color(0f, 0f, 0f, 0.75f);
            GUI.DrawTexture(
                new Rect(0, 0, Screen.width, Screen.height),
                Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUIStyle title = new GUIStyle(GUI.skin.label);
            title.fontSize = 60;
            title.fontStyle = FontStyle.Bold;
            title.alignment = TextAnchor.MiddleCenter;
            title.normal.textColor = Color.red;

            GUI.Label(
                new Rect(0, Screen.height / 2 - 100,
                    Screen.width, 90),
                "GAME OVER", title);

            GUIStyle restart = new GUIStyle(GUI.skin.label);
            restart.fontSize = 32;
            restart.fontStyle = FontStyle.Bold;
            restart.alignment = TextAnchor.MiddleCenter;
            restart.normal.textColor = Color.white;

            GUI.Label(
                new Rect(0, Screen.height / 2 + 10,
                    Screen.width, 60),
                "Press R to Restart", restart);
        }
    }
}
