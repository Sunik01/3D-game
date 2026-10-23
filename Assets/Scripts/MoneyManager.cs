
using UnityEngine;
using UnityEngine.UI;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    public int money = 0;
    private Text moneyText;

    private const string MoneyKey = "PlayerMoney";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        money = PlayerPrefs.GetInt(MoneyKey, money);
        CreateMoneyUI();
    }


    public void AddMoney(int amount)        
    {
        // ตรวจสอบคลาสที่เลือกอยู่
        string selectedClass =
            PlayerPrefs.GetString("SelectedClass", "Normal");

        // Richman ได้รับเงินเพิ่ม 20%
        int finalAmount = amount;

        if (selectedClass == "Richman" && amount > 0)
        {
            finalAmount = Mathf.RoundToInt(amount * 1.2f);
            Debug.Log("Richman Bonus: +" + (finalAmount - amount));
        }

        // เพิ่มเงินและบันทึก
        money = Mathf.Max(0, money + finalAmount);

        SaveMoney();
        UpdateMoneyUI();

        Debug.Log("Money received: $" + finalAmount);
        Debug.Log("Total Money: $" + money);
    }


    public bool SpendMoney(int amount)
    {
        if (amount < 0 || money < amount)
            return false;

        money -= amount;
        SaveMoney();
        UpdateMoneyUI();
        return true;
    }

    private void SaveMoney()
    {
        PlayerPrefs.SetInt(MoneyKey, money);
        PlayerPrefs.Save();
    }

    private void CreateMoneyUI()
    {
        GameObject canvasObject = new GameObject("MoneyCanvas");

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject textObject = new GameObject("MoneyText");
        textObject.transform.SetParent(canvasObject.transform, false);

        moneyText = textObject.AddComponent<Text>();
        moneyText.font =
            Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        moneyText.fontSize = 28;
        moneyText.fontStyle = FontStyle.Bold;
        moneyText.color = Color.yellow;
        moneyText.alignment = TextAnchor.UpperRight;

        RectTransform rect = moneyText.rectTransform;
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-25, -25);
        rect.sizeDelta = new Vector2(250, 50);

        UpdateMoneyUI();
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
            moneyText.text = "MONEY: $" + money;
    }
    public void SetUnlimitedMoney()
    {
        money = 999999999;

        PlayerPrefs.SetInt("PlayerMoney", money);
        PlayerPrefs.Save();

        UpdateMoneyUI();

        Debug.Log("RICHMASTER activated! Unlimited money!");

    }
}
