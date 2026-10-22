
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    public string mainMenuSceneName = "MainMenu";

    private GameObject pausePanel;
    private bool isPaused = false;
    private PlayerHealth playerHealth;

    private Font menuFont;
    private GameObject escHint;


    void Start()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerHealth = FindFirstObjectByType<PlayerHealth>();

        menuFont = Resources.GetBuiltinResource<Font>(
            "LegacyRuntime.ttf"
        );

        CreatePauseMenu();

    }

    void Update()
    {
        // ไม่เปิด Pause Menu เมื่อผู้เล่นตายแล้ว
        if (playerHealth != null &&
            playerHealth.currentHealth <= 0)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ContinueGame();
            else
                PauseGame();
        }
    }

    void CreatePauseMenu()
    {
        // สร้าง EventSystem ถ้ายังไม่มี
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventObj = new GameObject("EventSystem");
            eventObj.AddComponent<EventSystem>();
            eventObj.AddComponent<StandaloneInputModule>();
        }

        GameObject canvasObj = new GameObject("PauseMenuCanvas");

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler =
            canvasObj.AddComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();
        // ข้อความบอกปุ่มหยุดเกม
        escHint = new GameObject("ESCHint");
        escHint.transform.SetParent(canvasObj.transform, false);

        RectTransform hintRect = escHint.AddComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0.5f, 1);
        hintRect.anchorMax = new Vector2(0.5f, 1);
        hintRect.pivot = new Vector2(0.5f, 1);
        hintRect.anchoredPosition = new Vector2(0, -25);
        hintRect.sizeDelta = new Vector2(300, 45);

        Text hintText = escHint.AddComponent<Text>();
        hintText.font = menuFont;
        hintText.text = "[ ESC ]  Pause Menu";
        hintText.fontSize = 24;
        hintText.fontStyle = FontStyle.Bold;
        hintText.color = Color.white;
        hintText.alignment = TextAnchor.MiddleLeft;

        // พื้นหลังเมนู
        pausePanel = new GameObject("PausePanel");
        pausePanel.transform.SetParent(canvasObj.transform, false);

        RectTransform panelRect =
            pausePanel.AddComponent<RectTransform>();

        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image background = pausePanel.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.8f);

        CreateText(
            pausePanel.transform,
            "PAUSED",
            64,
            new Vector2(0, 200),
            new Vector2(600, 100),
            Color.white
        );

        CreateButton(
            pausePanel.transform,
            "CONTINUE",
            new Vector2(0, 50),
            ContinueGame
        );

        CreateButton(
            pausePanel.transform,
            "EXIT TO MAIN MENU",
            new Vector2(0, -50),
            ExitToMainMenu
        );

        pausePanel.SetActive(false);
    }

    void CreateText(
        Transform parent,
        string content,
        int size,
        Vector2 position,
        Vector2 dimensions,
        Color color)
    {
        GameObject obj = new GameObject(content + "Text");
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;

        Text text = obj.AddComponent<Text>();
        text.font = menuFont;
        text.text = content;
        text.fontSize = size;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontStyle = FontStyle.Bold;
    }

    void CreateButton(
        Transform parent,
        string label,
        Vector2 position,
        UnityEngine.Events.UnityAction action)
    {
        GameObject obj = new GameObject(label);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(450, 80);

        Image image = obj.AddComponent<Image>();
        image.color = new Color(0.05f, 0.4f, 0.7f);

        Button button = obj.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        CreateText(
            obj.transform,
            label,
            28,
            Vector2.zero,
            new Vector2(430, 70),
            Color.white
        );
    }

    public void PauseGame()
    {
        isPaused = true;
        escHint.SetActive(false);
        Time.timeScale = 0f;

        pausePanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ContinueGame()
    {
        isPaused = false;
        escHint.SetActive(true);
        Time.timeScale = 1f;

        pausePanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(mainMenuSceneName);
    }
}
