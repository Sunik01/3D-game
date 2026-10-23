
using UnityEngine;
using UnityEngine.UI;

public class ZombieHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Money Drop")]
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
        if (healthCanvas == null) return;

        // ติดตามตำแหน่งเหนือหัวซอมบี้
        float highestY = transform.position.y + topOffset;

        foreach (Renderer r in renderersToCheck)
        {
            if (r != null && r.enabled)
                highestY = Mathf.Max(highestY, r.bounds.max.y + 0.35f);
        }

        healthCanvas.transform.position =
            new Vector3(transform.position.x, highestY, transform.position.z);

        // หันหลอดเลือดเข้าหากล้อง
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null)
            healthCanvas.transform.rotation = Quaternion.LookRotation(
                healthCanvas.transform.position - mainCamera.transform.position
            );
    }

    private void CreateHealthBar()
    {
        GameObject canvasObject = new GameObject("ZombieHealthBar");
        healthCanvas = canvasObject.AddComponent<Canvas>();
        healthCanvas.renderMode = RenderMode.WorldSpace;
        canvasObject.AddComponent<CanvasScaler>();

        RectTransform canvasRect =
            canvasObject.GetComponent<RectTransform>();

        canvasRect.sizeDelta = new Vector2(100, 12);
        canvasRect.localScale = Vector3.one * 0.01f;

        // พื้นหลังสีแดง แสดงส่วนที่เสียเลือด
        GameObject redObject = new GameObject("HealthLost");
        redObject.transform.SetParent(canvasObject.transform, false);

        healthLost = redObject.AddComponent<Image>();
        healthLost.color = Color.red;

        RectTransform redRect = healthLost.rectTransform;
        redRect.anchorMin = Vector2.zero;
        redRect.anchorMax = Vector2.one;
        redRect.offsetMin = Vector2.zero;
        redRect.offsetMax = Vector2.zero;

        // แถบสีเขียว แสดงเลือดที่เหลือ
        GameObject greenObject = new GameObject("HealthFill");
        greenObject.transform.SetParent(canvasObject.transform, false);

        healthFill = greenObject.AddComponent<Image>();
        healthFill.color = Color.green;

        RectTransform greenRect = healthFill.rectTransform;
        greenRect.anchorMin = new Vector2(0, 0);
        greenRect.anchorMax = new Vector2(1, 1);
        greenRect.pivot = new Vector2(0, 0.5f);
        greenRect.offsetMin = Vector2.zero;
        greenRect.offsetMax = Vector2.zero;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);

        UpdateHealthBar();

        DamagePopup.Create(
            transform.position + Vector3.up * 1.5f,
            "-" + damage
        );

        Debug.Log("Zombie HP: " + currentHealth);

        if (currentHealth <= 0)
            Die();
    }

    private void UpdateHealthBar()
    {
        if (healthFill == null) return;

        float healthPercent = maxHealth > 0
            ? Mathf.Clamp01((float)currentHealth / maxHealth)
            : 0f;

        // ลดความกว้างของแถบสีเขียวตามเลือดที่เหลือ
        RectTransform fillRect = healthFill.rectTransform;
        fillRect.anchorMax = new Vector2(healthPercent, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (MoneyManager.Instance != null)
        {
            int reward = dropMoney;

            if (PlayerPrefs.GetString("SelectedClass", "Normal") == "Elite")
            {
                reward = Mathf.FloorToInt(dropMoney * 0.9f);
            }

            MoneyManager.Instance.AddMoney(reward);
        }

        if (healthCanvas != null)
            Destroy(healthCanvas.gameObject);

        Destroy(gameObject);
    }
}
