
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BossFightManager : MonoBehaviour
{
    public static BossFightManager Instance { get; private set; }

    [Header("Red Orb Unlock")]
    public GameObject redOrbPrefab;
    public Transform[] redOrbSpawnPoints = new Transform[4];
    public int orbsNeededToSpawnBoss = 1;
    public int collectedRedOrbs = 0;

    private bool bossUnlocked;
    private bool bossSpawned;
    private bool spawningBoss;

    [Header("Boss Arena")]
    public GameObject bossPrefab;
    public Transform bossSpawnPoint;
    public Transform player;
    public Collider arenaTrigger;
    public GameObject[] barrierObjects;

    [Header("Boss Ground Placement")]
    public LayerMask groundLayers = ~0;
    public float groundRayHeight = 30f;
    public float groundRayDistance = 100f;
    public float bossGroundOffset = 0f;

    [Header("Random Zombie Waves")]
    public GameObject[] zombiePrefabs;
    public Transform[] zombieSpawnPoints;
    public float zombieSpawnInterval = 12f;
    public int maxExtraZombiesAlive = 5;

    [Header("Victory")]
    public string mainMenuSceneName = "MainMenu";
    public int victoryMoneyReward = 1000;

    private readonly List<GameObject> liveZombies =
        new List<GameObject>();

    private Canvas canvas;
    private Text centerMessage;
    private bool arenaLocked;
    private bool victory;
    private Coroutine messageRoutine;

    public bool IsBossFightActive => bossSpawned && arenaLocked && !victory;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning(
                "พบ BossFightManager ซ้ำ จะลบตัวที่ซ้ำ");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        Time.timeScale = 1f;

        FindPlayer();
        CreateUI();
        SetBarrier(false);

        if (arenaTrigger != null)
            arenaTrigger.enabled = false;

        // รีเซ็ตสถานะสำหรับการเริ่มฉากใหม่
        collectedRedOrbs = 0;
        bossUnlocked = false;
        bossSpawned = false;
        spawningBoss = false;
        victory = false;
        arenaLocked = false;

        // ตรวจสอบว่ามีบอสวางอยู่ใน Scene อยู่แล้วหรือไม่
        BossHealth existingBoss = FindExistingBoss();

        if (existingBoss != null)
        {
            // เชื่อมบอสเดิมแทนการสร้างบอสซ้ำ
            bossSpawned = true;
            bossUnlocked = true;

            existingBoss.manager = this;

            if (player != null)
                existingBoss.player = player;

            Debug.LogWarning(
                "พบ Boss ใน Scene แล้ว: " + existingBoss.name +
                " จะไม่สร้างบอสเพิ่ม");

            if (arenaTrigger != null)
                arenaTrigger.enabled = true;

            StartCoroutine(ZombieWaveRoutine());
            return;
        }

        // ไม่มีบอสใน Scene จึงสร้าง Orb
        SpawnRedOrbs();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void FindPlayer()
    {
        if (player != null)
            return;

        GameObject p = GameObject.FindGameObjectWithTag("Player");

        if (p != null)
            player = p.transform;
        else
            Debug.LogWarning(
                "ไม่พบ Player Tag กรุณาตรวจสอบ Tag ของผู้เล่น");
    }

    private BossHealth FindExistingBoss()
    {
        BossHealth[] bosses =
            FindObjectsByType<BossHealth>(
                FindObjectsSortMode.None);

        foreach (BossHealth boss in bosses)
        {
            if (boss != null &&
                boss.gameObject.scene.IsValid() &&
                boss.gameObject.scene == gameObject.scene)
            {
                return boss;
            }
        }

        return null;
    }

    private void SpawnRedOrbs()
    {
        if (redOrbPrefab == null)
        {
            Debug.LogError(
                "ยังไม่ได้กำหนด Red Orb Prefab ใน Inspector");
            return;
        }

        if (redOrbSpawnPoints == null ||
            redOrbSpawnPoints.Length == 0)
        {
            Debug.LogError(
                "ยังไม่ได้กำหนด Red Orb Spawn Points");
            return;
        }

        int spawnedCount = 0;
        int count = Mathf.Min(4, redOrbSpawnPoints.Length);

        for (int i = 0; i < count; i++)
        {
            Transform point = redOrbSpawnPoints[i];

            if (point == null)
            {
                Debug.LogWarning(
                    "Red Orb Spawn Point ช่อง " + i +
                    " ยังว่างอยู่");
                continue;
            }

            GameObject orb = Instantiate(
                redOrbPrefab,
                point.position,
                point.rotation);

            // ค้นหา Collider ในตัวลูกด้วย
            Collider orbCollider =
                orb.GetComponentInChildren<Collider>();

            if (orbCollider == null)
            {
                Debug.LogError(
                    "Orb ไม่มี Collider: " + orb.name);
            }

            BossOrb orbScript = orb.GetComponent<BossOrb>();

            if (orbScript == null)
                orbScript = orb.AddComponent<BossOrb>();

            spawnedCount++;
        }

        Debug.Log(
            "สร้าง Red Orb สำเร็จ " + spawnedCount + " ลูก");
    }

    public void CollectRedOrb()
    {
        if (bossUnlocked || bossSpawned ||
            spawningBoss || victory)
        {
            Debug.Log(
                "ไม่รับ Orb เพิ่ม เพราะบอสปลดล็อกหรือเกิดแล้ว");
            return;
        }

        collectedRedOrbs++;

        Debug.Log(
            "Red Orb collected: " + collectedRedOrbs +
            " / " + Mathf.Max(1, orbsNeededToSpawnBoss));

        ShowMessage(
            "RED ORB COLLECTED " + collectedRedOrbs,
            2f);

        if (collectedRedOrbs >=
            Mathf.Max(1, orbsNeededToSpawnBoss))
        {
            bossUnlocked = true;
            spawningBoss = true;

            ShowMessage("BOSS HAVE SPAWN", 3f);

            StartCoroutine(SpawnBossRoutine());
        }
    }

    private IEnumerator SpawnBossRoutine()
    {
        yield return new WaitForSeconds(2f);

        if (bossSpawned || victory)
        {
            spawningBoss = false;
            yield break;
        }

        // ตรวจซ้ำเพื่อป้องกันบอสซ้ำ
        BossHealth existingBoss = FindExistingBoss();

        if (existingBoss != null)
        {
            bossSpawned = true;
            spawningBoss = false;
            existingBoss.manager = this;

            FindPlayer();

            if (player != null)
                existingBoss.player = player;

            Debug.LogWarning(
                "พบ Boss อยู่แล้ว จึงไม่สร้างบอสซ้ำ");

            if (arenaTrigger != null)
                arenaTrigger.enabled = true;

            StartCoroutine(ZombieWaveRoutine());
            yield break;
        }

        if (bossPrefab == null)
        {
            Debug.LogError(
                "Boss Prefab ยังไม่ได้กำหนดใน Inspector");

            spawningBoss = false;
            bossUnlocked = false;
            yield break;
        }

        if (bossSpawnPoint == null)
        {
            Debug.LogError(
                "Boss Spawn Point ยังไม่ได้กำหนดใน Inspector");

            spawningBoss = false;
            bossUnlocked = false;
            yield break;
        }

        Vector3 spawnPosition = bossSpawnPoint.position;

        Vector3 rayOrigin =
            bossSpawnPoint.position +
            Vector3.up * groundRayHeight;

        // ใช้ LayerMask ตรวจหาพื้น
        if (Physics.Raycast(
            rayOrigin,
            Vector3.down,
            out RaycastHit hit,
            groundRayDistance + groundRayHeight,
            groundLayers,
            QueryTriggerInteraction.Ignore))
        {
            spawnPosition =
                hit.point + Vector3.up * bossGroundOffset;

            Debug.Log(
                "พบพื้นสำหรับเกิดบอสที่: " + hit.point);
        }
        else
        {
            // ถ้าหาพื้นไม่พบ ให้ใช้ตำแหน่ง Spawn Point
            Debug.LogWarning(
                "Raycast หาพื้นไม่พบ ใช้ Boss Spawn Point โดยตรง");
        }

        GameObject boss = Instantiate(
            bossPrefab,
            spawnPosition,
            bossSpawnPoint.rotation);

        BossHealth health =
            boss.GetComponentInChildren<BossHealth>(true);

        if (health == null)
        {
            Debug.LogError(
                "Boss Prefab ไม่มี BossHealth กรุณาเพิ่ม " +
                "BossHealth ลงในตัวบอส");

            Destroy(boss);
            spawningBoss = false;
            bossUnlocked = false;
            yield break;
        }

        health.manager = this;

        FindPlayer();
        health.player = player;

        bossSpawned = true;
        spawningBoss = false;

        if (arenaTrigger != null)
            arenaTrigger.enabled = true;

        Debug.Log(
            "Boss spawned successfully: " + boss.name +
            " at " + spawnPosition);

        StartCoroutine(ZombieWaveRoutine());
    }

    public void PlayerEnteredArena()
    {
        if (!bossSpawned || !bossUnlocked ||
            victory || arenaLocked)
            return;

        arenaLocked = true;

        SetBarrier(true);

        ShowMessage("DEFEAT THE BOSS TO ESCAPE!", 3f);
    }

    public void BossDefeated()
    {
        if (victory)
            return;

        victory = true;
        arenaLocked = false;

        SetBarrier(false);
        StopAllCoroutines();

        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.AddMoney(victoryMoneyReward);
        }
        else
        {
            Debug.LogWarning(
                "MoneyManager.Instance not found");
        }

        ShowVictory();
    }

    private void SetBarrier(bool active)
    {
        if (barrierObjects == null)
            return;

        foreach (GameObject barrier in barrierObjects)
        {
            if (barrier != null)
                barrier.SetActive(active);
        }
    }

    private IEnumerator ZombieWaveRoutine()
    {
        while (!victory)
        {
            yield return new WaitForSeconds(
                Mathf.Max(2f, zombieSpawnInterval));

            liveZombies.RemoveAll(z => z == null);

            if (liveZombies.Count >= maxExtraZombiesAlive ||
                zombiePrefabs == null ||
                zombieSpawnPoints == null)
            {
                continue;
            }

            List<GameObject> prefabs = new List<GameObject>();

            foreach (GameObject prefab in zombiePrefabs)
            {
                if (prefab != null)
                    prefabs.Add(prefab);
            }

            List<Transform> points = new List<Transform>();

            foreach (Transform point in zombieSpawnPoints)
            {
                if (point != null)
                    points.Add(point);
            }

            if (prefabs.Count == 0 || points.Count == 0)
                continue;

            GameObject chosenPrefab =
                prefabs[Random.Range(0, prefabs.Count)];

            Transform chosenPoint =
                points[Random.Range(0, points.Count)];

            GameObject zombie = Instantiate(
                chosenPrefab,
                chosenPoint.position,
                chosenPoint.rotation);

            liveZombies.Add(zombie);
        }
    }

    private void CreateUI()
    {
        // ป้องกันการสร้าง UI ซ้ำ
        if (canvas != null)
            return;

        GameObject root = new GameObject("BossFightCanvas");

        canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution =
            new Vector2(1920, 1080);

        root.AddComponent<GraphicRaycaster>();

        GameObject msg = new GameObject("BossMessage");
        msg.transform.SetParent(root.transform, false);

        RectTransform rect = msg.AddComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax =
            new Vector2(0.5f, 0.82f);
        rect.sizeDelta = new Vector2(1100, 100);

        centerMessage = msg.AddComponent<Text>();
        centerMessage.font =
            Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        centerMessage.fontSize = 44;
        centerMessage.fontStyle = FontStyle.Bold;
        centerMessage.alignment = TextAnchor.MiddleCenter;
        centerMessage.color = Color.red;
        centerMessage.text = "";
    }

    public void ShowMessage(string message, float seconds)
    {
        if (centerMessage == null)
            return;

        if (messageRoutine != null)
            StopCoroutine(messageRoutine);

        messageRoutine =
            StartCoroutine(MessageRoutine(message, seconds));
    }

    private IEnumerator MessageRoutine(
        string message, float seconds)
    {
        centerMessage.text = message;

        yield return new WaitForSeconds(seconds);

        if (centerMessage != null)
            centerMessage.text = "";

        messageRoutine = null;
    }

    private void ShowVictory()
    {
        if (canvas == null)
            CreateUI();

        GameObject panel = new GameObject("BossVictoryPanel");
        panel.transform.SetParent(canvas.transform, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.88f);

        CreateText(
            panel.transform, "YOU WIN", 70,
            new Vector2(0, 130),
            new Vector2(800, 120), Color.yellow);

        CreateButton(
            panel.transform, "CONTINUE",
            new Vector2(0, 10), () =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(
                    SceneManager.GetActiveScene().buildIndex);
            });

        CreateButton(
            panel.transform, "EXIT TO MAIN MENU",
            new Vector2(0, -90), () =>
            {
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                SceneManager.LoadScene(mainMenuSceneName);
            });

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CreateText(
        Transform parent,
        string value,
        int size,
        Vector2 pos,
        Vector2 dims,
        Color color)
    {
        GameObject obj = new GameObject(value);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax =
            new Vector2(0.5f, 0.5f);
        rect.sizeDelta = dims;
        rect.anchoredPosition = pos;

        Text text = obj.AddComponent<Text>();
        text.font =
            Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = value;
        text.fontSize = size;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
    }

    private void CreateButton(
        Transform parent,
        string label,
        Vector2 pos,
        UnityEngine.Events.UnityAction action)
    {
        GameObject obj = new GameObject(label);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax =
            new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(520, 70);
        rect.anchoredPosition = pos;

        Image image = obj.AddComponent<Image>();
        image.color = new Color(0.65f, 0.05f, 0.05f);

        Button button = obj.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        CreateText(
            obj.transform, label, 26,
            Vector2.zero, new Vector2(500, 65),
            Color.white);
    }
}
