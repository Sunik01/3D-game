using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Scene")]
    public string gameSceneName = "MainScene";

    private Font font;
    private Canvas canvas;
    private GameObject root;
    private GameObject panel;
    private Text message;
    private InputField cheatInput;
    private Text moneyLabel;

    // 0 = หน้า 1, 1 = หน้า 2, 2 = หน้า 3
    private int classPage = 0;

    private readonly Color bg =
        new Color(0.035f, 0.07f, 0.12f);

    private readonly Color blue =
        new Color(0.05f, 0.48f, 0.8f);

    private readonly Color white =
        new Color(0.92f, 0.96f, 1f);

    private void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        font = Resources.GetBuiltinResource<Font>(
            "LegacyRuntime.ttf");

        GameObject canvasObject =
            new GameObject("MainMenuCanvas");

        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler =
            canvasObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(1920, 1080);

        canvasObject.AddComponent<GraphicRaycaster>();

        // เพิ่ม EventSystem ให้ปุ่ม UI รับการคลิกได้
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem =
                new GameObject("EventSystem");

            eventSystem.AddComponent<
                UnityEngine.EventSystems.EventSystem>();

            eventSystem.AddComponent<
                UnityEngine.EventSystems.StandaloneInputModule>();
        }

        root = new GameObject("MenuBackground");
        root.transform.SetParent(canvas.transform, false);

        RectTransform rt =
            root.AddComponent<RectTransform>();

        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        root.AddComponent<Image>().color = bg;

        ShowMain();
    }

    // =========================
    // PANEL
    // =========================

    private GameObject NewPanel(string title)
    {
        if (panel != null)
            Destroy(panel);

        panel = new GameObject("MenuPanel");
        panel.transform.SetParent(root.transform, false);

        RectTransform rt =
            panel.AddComponent<RectTransform>();

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(800, 850);
        rt.anchoredPosition = Vector2.zero;

        panel.AddComponent<Image>().color =
            new Color(0.02f, 0.04f, 0.08f, 0.96f);

        AddText(
            panel.transform,
            title,
            44,
            new Vector2(0, 350),
            new Vector2(720, 80),
            true
        );

        return panel;
    }

    private Text AddText(
        Transform parent,
        string value,
        int size,
        Vector2 pos,
        Vector2 dimensions,
        bool bold = false)
    {
        GameObject obj = new GameObject("Text");
        obj.transform.SetParent(parent, false);

        RectTransform rt =
            obj.AddComponent<RectTransform>();

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = dimensions;

        Text t = obj.AddComponent<Text>();
        t.font = font;
        t.text = value;
        t.fontSize = size;
        t.color = white;
        t.alignment = TextAnchor.MiddleCenter;

        t.fontStyle = bold
            ? FontStyle.Bold
            : FontStyle.Normal;

        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;

        return t;
    }

    private void AddButton(
        Transform parent,
        string label,
        Vector2 pos,
        UnityEngine.Events.UnityAction action)
    {
        GameObject obj = new GameObject("Button_" + label);
        obj.transform.SetParent(parent, false);

        RectTransform rt =
            obj.AddComponent<RectTransform>();

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(480, 65);

        Image image = obj.AddComponent<Image>();
        image.color = blue;

        Button button = obj.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        AddText(
            obj.transform,
            label,
            19,
            Vector2.zero,
            new Vector2(460, 60),
            true
        );
    }

    // =========================
    // MAIN MENU
    // =========================

    private void ShowMain()
    {
        NewPanel("SKY ISLAND SURVIVAL");

        AddText(
            panel.transform,
            "SURVIVE THE FLOATING ISLAND",
            22,
            new Vector2(0, 270),
            new Vector2(650, 50)
        );

        AddButton(
            panel.transform,
            "START GAME",
            new Vector2(0, 165),
            () => ShowClassPage(0)
        );

        AddButton(
            panel.transform,
            "GAME INFO",
            new Vector2(0, 80),
            ShowInfo
        );

        AddButton(
            panel.transform,
            "CREDITS",
            new Vector2(0, -5),
            ShowCredits
        );

        AddButton(
            panel.transform,
            "CHEAT CODES",
            new Vector2(0, -90),
            ShowCheats
        );

        AddButton(
            panel.transform,
            "EXIT GAME",
            new Vector2(0, -175),
            ExitGame
        );
    }

    // =========================
    // CLASS PAGE NAVIGATION
    // =========================

    private void ShowClassPage(int page)
    {
        classPage = Mathf.Clamp(page, 0, 2);

        if (classPage == 0)
            ShowClasses();
        else if (classPage == 1)
            ShowMoreClasses();
        else
            ShowAdvancedClasses();
    }

    // =========================
    // PAGE 1: RUNNER, NORMAL, TANK
    // =========================

    private void ShowClasses()
    {
        classPage = 0;

        NewPanel("SELECT YOUR CLASS");
        AddMoneyLabel();

        AddText(
            panel.transform,
            "PAGE 1 / 3 - STARTER CLASSES",
            22,
            new Vector2(0, 255),
            new Vector2(650, 40)
        );

        AddButton(
            panel.transform,
            "SPEED RUNNER | 75 HP | Speed 8",
            new Vector2(0, 150),
            () => SelectClass("Runner")
        );

        AddButton(
            panel.transform,
            "NORMAL | 100 HP | Speed 6",
            new Vector2(0, 50),
            () => SelectClass("Normal")
        );

        AddButton(
            panel.transform,
            "TANK | 160 HP | Speed 4",
            new Vector2(0, -50),
            () => SelectClass("Tank")
        );

        AddButton(
            panel.transform,
            "NEXT PAGE >>",
            new Vector2(0, -180),
            () => ShowClassPage(1)
        );

        AddButton(
            panel.transform,
            "MAIN MENU",
            new Vector2(0, -270),
            ShowMain
        );
    }

    // =========================
    // PAGE 2: SOFT, HEALTH EMPIRE, ELITE
    // =========================

    private void ShowMoreClasses()
    {
        classPage = 1;

        NewPanel("SPECIAL CLASSES");
        AddMoneyLabel();

        AddText(
            panel.transform,
            "PAGE 2 / 3 - SPECIAL CLASSES",
            22,
            new Vector2(0, 255),
            new Vector2(650, 40)
        );

        AddButton(
            panel.transform,
            ClassButtonText(
                "Soft", 150,
                "50 HP | Speed 9 | High Jump"),
            new Vector2(0, 150),
            () => BuyOrSelectClass("Soft", 150)
        );

        AddButton(
            panel.transform,
            ClassButtonText(
                "HealthEmpire", 250,
                "400 HP | Speed 5 | Heal Regen -500%"),
            new Vector2(0, 50),
            () => BuyOrSelectClass("HealthEmpire", 250)
        );

        AddButton(
            panel.transform,
            ClassButtonText(
                "Elite", 500,
                "125 HP | Speed 7 | Sword 35 | -10% Money"),
            new Vector2(0, -50),
            () => BuyOrSelectClass("Elite", 500)
        );

        AddButton(
    panel.transform,
    "<< BACK",
    new Vector2(-145, -180),
    () => ShowClassPage(0)
);

        AddButton(
            panel.transform,
            "NEXT >>",
            new Vector2(145, -180),
            () => ShowClassPage(2)
        );

        AddButton(
            panel.transform,
            "MAIN MENU",
            new Vector2(0, -270),
            ShowMain
        );
    }

    // =========================
    // PAGE 3: VAMPIRE, GODSWORD, RICHMAN
    // =========================

    private void ShowAdvancedClasses()
    {
        classPage = 2;

        NewPanel("ADVANCED CLASSES");
        AddMoneyLabel();

        AddText(
            panel.transform,
            "PAGE 3 / 3 - ADVANCED CLASSES",
            22,
            new Vector2(0, 255),
            new Vector2(650, 40)
        );

        AddButton(
            panel.transform,
            ClassButtonText(
                "Vampire", 750,
                "120 HP | Speed 6 | Lifesteal 5%"),
            new Vector2(0, 150),
            () => BuyOrSelectClass("Vampire", 750)
        );

        AddButton(
            panel.transform,
            ClassButtonText(
                "GodSword", 1000,
                "140 HP | Speed 6.5 | Sword 50 | Attack Speed +25%"),
            new Vector2(0, 50),
            () => BuyOrSelectClass("GodSword", 1000)
        );

        AddButton(
            panel.transform,
            ClassButtonText(
                "Richman", 1000,
                "180 HP | Speed 8 | Money +20%"),
            new Vector2(0, -50),
            () => BuyOrSelectClass("Richman", 1000)
        );

        AddButton(
            panel.transform,
            "<< BACK",
            new Vector2(0, -180),
            () => ShowClassPage(1)
        );

        AddButton(
            panel.transform,
            "MAIN MENU",
            new Vector2(0, -270),
            ShowMain
        );
    }

    // =========================
    // CLASS PURCHASE
    // =========================

    private string ClassButtonText(
        string className,
        int price,
        string stats)
    {
        bool unlocked =
            PlayerPrefs.GetInt(
                "ClassUnlocked_" + className, 0) == 1;

        if (unlocked)
        {
            return className +
                " | UNLOCKED - SELECT\n" + stats;
        }

        return className +
            " | BUY $" + price + "\n" + stats;
    }

    private void AddMoneyLabel()
    {
        int balance = PlayerPrefs.GetInt("PlayerMoney", 0);

        moneyLabel = AddText(
            panel.transform,
            "YOUR MONEY: $" + balance,
            26,
            new Vector2(0, 305),
            new Vector2(650, 45),
            true
        );

        moneyLabel.color = Color.yellow;
    }

    private void BuyOrSelectClass(
        string className,
        int price)
    {
        string key = "ClassUnlocked_" + className;

        bool unlocked =
            PlayerPrefs.GetInt(key, 0) == 1;

        // หากปลดล็อกแล้ว ให้เลือกคลาส
        if (unlocked)
        {
            SelectClass(className);
            return;
        }

        int balance = PlayerPrefs.GetInt("PlayerMoney", 0);

        if (balance < price)
        {
            ShowPurchaseMessage(
                "Not enough money! Need $" + price);

            return;
        }

        balance -= price;

        PlayerPrefs.SetInt("PlayerMoney", balance);
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();

        // รีเฟรชหน้าที่กำลังเปิดอยู่
        ShowClassPage(classPage);

        ShowPurchaseMessage(
            className + " UNLOCKED! Click again to play.");

        Debug.Log(
            "Unlocked: " + key +
            " | Money left: $" + balance);
    }

    private void ShowPurchaseMessage(string text)
    {
        AddText(
            panel.transform,
            text,
            18,
            new Vector2(0, -350),
            new Vector2(720, 40),
            true
        );
    }

    private void SelectClass(string className)
    {
        // คลาสฟรี
        if (className == "Runner" ||
            className == "Normal" ||
            className == "Tank")
        {
            PlayerPrefs.SetString("SelectedClass", className);
            PlayerPrefs.Save();

            SceneManager.LoadScene(gameSceneName);
            return;
        }

        // ตรวจสอบคลาสที่ต้องซื้อ
        if (PlayerPrefs.GetInt(
            "ClassUnlocked_" + className, 0) != 1)
        {
            Debug.LogWarning(
                "Class has not been unlocked: " + className);

            return;
        }

        PlayerPrefs.SetString("SelectedClass", className);
        PlayerPrefs.Save();

        Debug.Log("Selected class: " + className);

        SceneManager.LoadScene(gameSceneName);
    }

    // =========================
    // GAME INFO
    // =========================

    private void ShowInfo()
    {
        NewPanel("GAME INFO");

        AddText(
            panel.transform,
            "SKY ISLAND SURVIVAL\n\n" +
            "Explore the maze on a floating island.\n\n" +
            "W A S D : Move\n" +
            "Mouse : Look around\n" +
            "SPACE : Jump\n\n" +
            "Avoid traps and kill zombies.\n" +
            "Manage your health and survive!",
            24,
            new Vector2(0, 70),
            new Vector2(650, 400)
        );

        AddButton(
            panel.transform,
            "BACK",
            new Vector2(0, -250),
            ShowMain
        );
    }

    // =========================
    // CREDITS
    // =========================

    private void ShowCredits()
    {
        NewPanel("CREDITS");

        AddText(
            panel.transform,
            "GAME DEVELOPER\n\n" +
            "Your Name Here\n\n" +
            "Created with Unity 6",
            28,
            new Vector2(0, 70),
            new Vector2(650, 250)
        );

        AddButton(
            panel.transform,
            "BACK",
            new Vector2(0, -200),
            ShowMain
        );
    }

    // =========================
    // CHEAT CODES
    // =========================

    private void ShowCheats()
    {
        NewPanel("CHEAT CODES");

        AddText(
            panel.transform,
            "Enter a code to unlock bonus features",
            21,
            new Vector2(0, 230),
            new Vector2(650, 60)
        );

        GameObject inputObject = new GameObject("CheatInput");
        inputObject.transform.SetParent(panel.transform, false);

        RectTransform rt =
            inputObject.AddComponent<RectTransform>();

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(480, 65);
        rt.anchoredPosition = new Vector2(0, 140);

        inputObject.AddComponent<Image>().color = white;

        cheatInput = inputObject.AddComponent<InputField>();

        Text inputText = AddText(
            inputObject.transform,
            "",
            24,
            Vector2.zero,
            new Vector2(450, 55)
        );

        inputText.color = Color.black;
        cheatInput.textComponent = inputText;

        Text placeholder = AddText(
            inputObject.transform,
            "Enter code...",
            22,
            Vector2.zero,
            new Vector2(450, 55)
        );

        placeholder.color = Color.gray;
        cheatInput.placeholder = placeholder;

        message = AddText(
            panel.transform,
            "",
            22,
            new Vector2(0, 65),
            new Vector2(650, 60)
        );

        AddButton(
            panel.transform,
            "UNLOCK",
            new Vector2(0, -20),
            CheckCheat
        );

        AddButton(
            panel.transform,
            "RESET CHEATS",
            new Vector2(0, -100),
            ResetCheats
        );

        AddButton(
            panel.transform,
            "BACK",
            new Vector2(0, -180),
            ShowMain
        );
    }

    private void ResetCheats()
    {
        PlayerPrefs.DeleteKey("SpeedJumpUnlocked");
        PlayerPrefs.DeleteKey("GodModeUnlocked");

        PlayerPrefs.SetInt("PlayerMoney", 0);

        PlayerPrefs.DeleteKey("ClassUnlocked_Soft");
        PlayerPrefs.DeleteKey("ClassUnlocked_HealthEmpire");
        PlayerPrefs.DeleteKey("ClassUnlocked_Elite");
        PlayerPrefs.DeleteKey("ClassUnlocked_Vampire");
        PlayerPrefs.DeleteKey("ClassUnlocked_GodSword");
        PlayerPrefs.DeleteKey("ClassUnlocked_Richman");

        PlayerPrefs.Save();

        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.money = 0;
        }

        if (cheatInput != null)
            cheatInput.text = "";

        if (message != null)
        {
            message.text = "RESET COMPLETE! Money: $0";
            message.color = Color.yellow;
        }

        Debug.Log(
            "RESET COMPLETE! Money and purchased classes reset.");
    }

    private void CheckCheat()
    {
        if (cheatInput == null || message == null)
            return;

        string code = cheatInput.text.Trim().ToUpper();

        if (code == "SKY2026")
        {
            PlayerPrefs.SetInt("SpeedJumpUnlocked", 1);
            PlayerPrefs.Save();

            message.text =
                "UNLOCKED! Super Speed + High Jump";

            message.color = Color.green;
        }
        else if (code == "GODMODE")
        {
            PlayerPrefs.SetInt("GodModeUnlocked", 1);
            PlayerPrefs.Save();

            message.text = "UNLOCKED! Infinite Health";
            message.color = Color.green;
        }
        else if (code == "RICHMASTER")
        {
            int richMoney = 999999999;

            PlayerPrefs.SetInt("PlayerMoney", richMoney);
            PlayerPrefs.Save();

            if (MoneyManager.Instance != null)
                MoneyManager.Instance.money = richMoney;

            message.text =
                "RICHMASTER ACTIVATED! Unlimited Money!";

            message.color = Color.green;

            Debug.Log(
                "RICHMASTER ACTIVATED! Money = " + richMoney);
        }
        else
        {
            message.text = "Invalid code. Try again.";
            message.color = Color.red;
        }
    }

    // =========================
    // EXIT
    // =========================

    private void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}