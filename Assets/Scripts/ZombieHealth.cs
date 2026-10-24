
using UnityEngine;
using UnityEngine.UI;

public class ZombieHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth = 100;
    public int dropMoney = 10;

    private bool isDead;
    private Image healthFill;
    private Canvas healthCanvas;
    private Camera mainCamera;
    private Image healthLost;
    private Renderer[] renderersToCheck;
    private float topOffset = 0.5f;

    private void Awake()
    {
        currentHealth = maxHealth;
        renderersToCheck = GetComponentsInChildren<Renderer>();
        mainCamera = Camera.main;

        CreateHealthBar();
        UpdateHealthBar();
    }

    private void LateUpdate()
    {
        if (healthCanvas == null)
            return;

        float highestY = transform.position.y + topOffset;

        foreach (Renderer r in renderersToCheck)
        {
            if (r != null && r.enabled)
                highestY = Mathf.Max(
                    highestY, r.bounds.max.y + 0.35f);
        }

        healthCanvas.transform.position =
            new Vector3(
                transform.position.x,
                highestY,
                transform.position.z);

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null)
        {
            healthCanvas.transform.rotation =
                Quaternion.LookRotation(
                    healthCanvas.transform.position -
                    mainCamera.transform.position);
        }
    }

    private void CreateHealthBar()
    {
        GameObject canvasObject =
            new GameObject("ZombieHealthBar");

        healthCanvas = canvasObject.AddComponent<Canvas>();
        healthCanvas.renderMode = RenderMode.WorldSpace;
        canvasObject.AddComponent<CanvasScaler>();

        RectTransform canvasRect =
            canvasObject.GetComponent<RectTransform>();

        canvasRect.sizeDelta = new Vector2(100, 12);
        canvasRect.localScale = Vector3.one * 0.01f;

        GameObject redObject = new GameObject("HealthLost");
        redObject.transform.SetParent(canvasObject.transform, false);

        healthLost = redObject.AddComponent<Image>();
        healthLost.color = Color.red;

        RectTransform redRect = healthLost.rectTransform;
        redRect.anchorMin = Vector2.zero;
        redRect.anchorMax = Vector2.one;
        redRect.offsetMin = Vector2.zero;
        redRect.offsetMax = Vector2.zero;

        GameObject greenObject = new GameObject("HealthFill");
        greenObject.transform.SetParent(canvasObject.transform, false);

        healthFill = greenObject.AddComponent<Image>();
        healthFill.color = Color.green;

        RectTransform greenRect = healthFill.rectTransform;
        greenRect.anchorMin = Vector2.zero;
        greenRect.anchorMax = Vector2.one;
        greenRect.pivot = new Vector2(0, 0.5f);
        greenRect.offsetMin = Vector2.zero;
        greenRect.offsetMax = Vector2.zero;
    }

    public void TakeDamage(int damage, GameObject attacker = null)
    {
        if (isDead || damage <= 0)
            return;

        currentHealth = Mathf.Clamp(
            currentHealth - damage, 0, maxHealth);

        UpdateHealthBar();

        DamagePopup.Create(
            transform.position + Vector3.up * 1.5f,
            "-" + damage);

        if (currentHealth <= 0)
            Die(attacker);
    }

    private void UpdateHealthBar()
    {
        if (healthFill == null)
            return;

        float percent = maxHealth > 0
            ? Mathf.Clamp01((float)currentHealth / maxHealth)
            : 0f;

        RectTransform rect = healthFill.rectTransform;
        rect.anchorMax = new Vector2(percent, 1f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }


    private void Die(GameObject attacker)
    {
        if (isDead)
            return;

        isDead = true;

        // ให้เงินเมื่อซอมบี้ตาย
        if (MoneyManager.Instance != null)
        {
            int reward = dropMoney;

            if (PlayerPrefs.GetString("SelectedClass", "Normal") == "Elite")
            {
                reward = Mathf.FloorToInt(dropMoney * 0.9f);
            }

            MoneyManager.Instance.AddMoney(reward);
        }

        // ค้นหาระบบคลาสจากตัวผู้โจมตี
        PlayerClassSystem classSystem = null;

        if (attacker != null)
        {
            classSystem = attacker.GetComponent<PlayerClassSystem>();

            if (classSystem == null)
                classSystem = attacker.GetComponentInParent<PlayerClassSystem>();

            if (classSystem == null)
                classSystem = attacker.GetComponentInChildren<PlayerClassSystem>();
        }

        // เกมเล่นคนเดียว: ใช้ระบบคลาสของผู้เล่นเป็นตัวสำรอง
        if (classSystem == null)
            classSystem = PlayerClassSystem.Instance;

        if (classSystem != null)
        {
            classSystem.RegisterZombieKill();
        }
        else
        {
            Debug.LogWarning(
                "ไม่พบ PlayerClassSystem ตอนซอมบี้ตาย!");
        }

        if (healthCanvas != null)
            Destroy(healthCanvas.gameObject);

        Destroy(gameObject);
    }

}
