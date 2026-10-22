
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MainMenuUI : MonoBehaviour
{
    public string gameSceneName = "MainScene";

    private Font font;
    private Canvas canvas;
    private GameObject root;
    private GameObject panel;
    private Text message;
    private InputField cheatInput;

    private readonly Color bg = new Color(0.035f, 0.07f, 0.12f);
    private readonly Color blue = new Color(0.05f, 0.48f, 0.8f);
    private readonly Color white = new Color(0.92f, 0.96f, 1f);

    void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject canvasObject = new GameObject("MainMenuCanvas");
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.GetComponent<CanvasScaler>().referenceResolution =
            new Vector2(1920, 1080);
        canvasObject.AddComponent<GraphicRaycaster>();

        root = new GameObject("MenuBackground");
        root.transform.SetParent(canvas.transform, false);
        RectTransform rt = root.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        root.AddComponent<Image>().color = bg;

        ShowMain();
    }

    GameObject NewPanel(string title)
    {
        if (panel != null) Destroy(panel);

        panel = new GameObject("MenuPanel");
        panel.transform.SetParent(root.transform, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(800, 850);
        rt.anchoredPosition = Vector2.zero;

        panel.AddComponent<Image>().color =
            new Color(0.02f, 0.04f, 0.08f, 0.96f);

        AddText(panel.transform, title, 44,
            new Vector2(0, 350), new Vector2(720, 80), true);

        return panel;
    }

    Text AddText(Transform parent, string value, int size,
        Vector2 pos, Vector2 dimensions, bool bold = false)
    {
        GameObject obj = new GameObject("Text");
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
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
        t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    void AddButton(Transform parent, string label,
        Vector2 pos, UnityEngine.Events.UnityAction action)
    {
        GameObject obj = new GameObject(label);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
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

        AddText(obj.transform, label, 24,
            Vector2.zero, new Vector2(460, 60), true);
    }

    void ShowMain()
    {
        NewPanel("SKY ISLAND SURVIVAL");

        AddText(panel.transform,
            "SURVIVE THE FLOATING ISLAND",
            22, new Vector2(0, 270), new Vector2(650, 50));

        AddButton(panel.transform, "START GAME",
            new Vector2(0, 165), ShowClasses);
        AddButton(panel.transform, "GAME INFO",
            new Vector2(0, 80), ShowInfo);
        AddButton(panel.transform, "CREDITS",
            new Vector2(0, -5), ShowCredits);
        AddButton(panel.transform, "CHEAT CODES",
            new Vector2(0, -90), ShowCheats);
        AddButton(panel.transform, "EXIT GAME",
            new Vector2(0, -175), ExitGame);
    }

    void ShowClasses()
    {
        NewPanel("SELECT YOUR CLASS");

        AddText(panel.transform,
            "Choose your survival style",
            22, new Vector2(0, 270), new Vector2(650, 45));

        AddButton(panel.transform,
            "SPEED RUNNER  |  Fast, 75 HP",
            new Vector2(0, 160),
            () => SelectClass("Runner"));

        AddButton(panel.transform,
            "NORMAL  |  Balanced, 100 HP",
            new Vector2(0, 70),
            () => SelectClass("Normal"));

        AddButton(panel.transform,
            "TANK  |  Slow, 160 HP",
            new Vector2(0, -20),
            () => SelectClass("Tank"));

        AddButton(panel.transform, "BACK",
            new Vector2(0, -150), ShowMain);
    }

    void SelectClass(string className)
    {
        PlayerPrefs.SetString("SelectedClass", className);
        PlayerPrefs.Save();

        SceneManager.LoadScene(gameSceneName);
    }

    void ShowInfo()
    {
        NewPanel("GAME INFO");

        AddText(panel.transform,
            "SKY ISLAND SURVIVAL\n\n" +
            "Explore the maze on a floating island.\n\n" +
            "W A S D : Move\n" +
            "Mouse : Look around\n" +
            "SPACE : Jump\n\n" +
            "Avoid trap and kill zombie.\n" +
            "Manage your health and survive!",
            24, new Vector2(0, 70), new Vector2(650, 400));

        AddButton(panel.transform, "BACK",
            new Vector2(0, -250), ShowMain);
    }

    void ShowCredits()
    {
        NewPanel("CREDITS");

        AddText(panel.transform,
            "GAME DEVELOPER\n\n" +
            "Your Name Here\n\n" +
            "Created with Unity 6",
            28, new Vector2(0, 70), new Vector2(650, 250));

        AddButton(panel.transform, "BACK",
            new Vector2(0, -200), ShowMain);
    }

    void ShowCheats()
    {
        NewPanel("CHEAT CODES");

        AddText(panel.transform,
            "Enter a code to unlock bonus features",
            21, new Vector2(0, 230), new Vector2(650, 60));

        GameObject inputObject = new GameObject("CheatInput");
        inputObject.transform.SetParent(panel.transform, false);

        RectTransform rt = inputObject.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(480, 65);
        rt.anchoredPosition = new Vector2(0, 140);

        inputObject.AddComponent<Image>().color = white;
        cheatInput = inputObject.AddComponent<InputField>();

        Text inputText = AddText(inputObject.transform, "", 24,
            Vector2.zero, new Vector2(450, 55));
        inputText.color = Color.black;
        cheatInput.textComponent = inputText;

        Text placeholder = AddText(inputObject.transform,
            "Enter code...", 22, Vector2.zero,
            new Vector2(450, 55));
        placeholder.color = Color.gray;
        cheatInput.placeholder = placeholder;

        message = AddText(panel.transform, "", 22,
            new Vector2(0, 65), new Vector2(650, 60));


        AddButton(panel.transform, "UNLOCK",
            new Vector2(0, -20), CheckCheat);

        AddButton(panel.transform, "RESET CHEATS",
            new Vector2(0, -100), ResetCheats);

        AddButton(panel.transform, "BACK",
            new Vector2(0, -180), ShowMain);

    }

    void ResetCheats()
    {
        PlayerPrefs.DeleteKey("SpeedJumpUnlocked");
        PlayerPrefs.DeleteKey("GodModeUnlocked");
        PlayerPrefs.Save();

        if (cheatInput != null)
            cheatInput.text = "";

        if (message != null)
        {
            message.text = "Cheats have been reset!";
            message.color = Color.yellow;
        }

        Debug.Log("Cheat codes have been reset.");
    }


    void CheckCheat()
    {
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

            message.text =
                "UNLOCKED! Infinite Health";
            message.color = Color.green;
        }
        else
        {
            message.text = "Invalid code. Try again.";
            message.color = Color.red;
        }
    }

    void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
