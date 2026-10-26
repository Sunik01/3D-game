using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class MagicianSpellController : MonoBehaviour
{
    [Header("Cooldowns")]
    public float ringCooldown = 10f;
    public float rangedCooldown = 15f;
    public float buffCooldown = 30f;

    [Header("Spell 1: Ring")]
    public float ringRadius = 4f;
    public int ringDamage = 20;
    public float ringDuration = 3f;
    public float ringTick = 0.5f;

    [Header("Spell 2: Ranged Zone")]
    public float castRange = 20f;
    public float zoneRadius = 3f;
    public int zoneDamage = 8;
    public float zoneDuration = 4f;
    public float zoneTick = 0.5f;

    [Header("Ranged Zone Ground")]
    public LayerMask groundLayers = ~0;
    public float groundRayHeight = 5f;
    public float groundRayDistance = 20f;
    public float groundOffset = 0.08f;

    [Header("Spell 3: Buff")]
    public float buffDuration = 8f;
    public float buffSpeedMultiplier = 1.5f;
    public float buffDamageTakenMultiplier = 0.5f;
    public float buffRegenMultiplier = 2f;

    [Header("Buff Visual")]
    public Color buffColor = new Color(0.2f, 0.8f, 1f, 1f);
    public float buffVisualRadius = 1.2f;
    public float buffVisualHeight = 0.15f;
    public float buffPulseSpeed = 3f;
    public float buffRingWidth = 0.09f;

    [Header("Cooldown UI")]
    public bool showCooldownUI = true;
    public Vector2 uiSize = new Vector2(250f, 150f);
    public float uiMargin = 20f;

    public float SpeedMultiplier { get; private set; } = 1f;
    public float DamageTakenMultiplier { get; private set; } = 1f;
    public float RegenMultiplier { get; private set; } = 1f;

    private float nextRingTime;
    private float nextRangedTime;
    private float nextBuffTime;
    private float buffEndTime;

    private Camera playerCamera;
    private GameObject buffVisual;
    private LineRenderer buffRing;
    private LineRenderer buffInnerRing;
    private bool isBuffActive;

    private GUIStyle titleStyle;
    private GUIStyle spellStyle;
    private GUIStyle statusStyle;
    private GUIStyle buffStyle;

    private bool infiniteMana;
    private string cheatBuffer = "";
    private const string InfiniteManaCode = "INFMANA";
    private const int RingSegments = 64;

    private bool IsMagician =>
        PlayerPrefs.GetString("SelectedClass", "Normal") == "Magician";

    private bool HasInfiniteMana =>
        IsMagician &&
        (infiniteMana || PlayerPrefs.GetInt("InfManaUnlocked", 0) == 1);

    private void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (IsMagician && PlayerPrefs.GetInt("InfManaUnlocked", 0) == 1)
            infiniteMana = true;
    }

    private void Update()
    {
        if (!IsMagician)
        {
            cheatBuffer = "";
            return;
        }

        CheckCheatCode();

        if (Input.GetKeyDown(KeyCode.Z) &&
            (HasInfiniteMana || Time.time >= nextRingTime))
        {
            if (!HasInfiniteMana)
                nextRingTime = Time.time + ringCooldown;
            StartCoroutine(CastRing());
        }

        if (Input.GetKeyDown(KeyCode.X) &&
            (HasInfiniteMana || Time.time >= nextRangedTime))
        {
            if (!HasInfiniteMana)
                nextRangedTime = Time.time + rangedCooldown;
            CastRangedZone();
        }

        if (Input.GetKeyDown(KeyCode.C) &&
            !isBuffActive &&
            (HasInfiniteMana || Time.time >= nextBuffTime))
        {
            if (!HasInfiniteMana)
                nextBuffTime = Time.time + buffCooldown;
            StartCoroutine(CastBuff());
        }
    }

    private IEnumerator CastRing()
    {   
        GameObject visual = CreateRingVisual(transform, ringRadius, Color.cyan, 0.08f);
        float elapsed = 0f;
        float damageTimer = 0f;

        while (elapsed < ringDuration)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= Mathf.Max(0.05f, ringTick))
            {
                damageTimer = 0f;
                Collider[] hits = Physics.OverlapSphere(transform.position, ringRadius);
                HashSet<ZombieHealth> damaged = new HashSet<ZombieHealth>();

                HashSet<ZombieHealth> damagedZombies = new HashSet<ZombieHealth>();
                HashSet<BossHealth> damagedBosses = new HashSet<BossHealth>();

                foreach (Collider hit in hits)
                {
                    BossHealth boss = hit.GetComponentInParent<BossHealth>();

                    if (boss != null)
                    {
                        if (damagedBosses.Add(boss))
                            boss.TakeDamage(ringDamage);

                        continue;
                    }

                    ZombieHealth zombie = hit.GetComponentInParent<ZombieHealth>();

                    if (zombie != null && damagedZombies.Add(zombie))
                        zombie.TakeDamage(ringDamage, gameObject);
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (visual != null)
        {

        }
            Destroy(visual);
    }

    private void CastRangedZone()
    {
        Collider[] hits = Physics.OverlapSphere(
    transform.position, castRange);

        HashSet<ZombieHealth> nearbyZombies = new HashSet<ZombieHealth>();
        HashSet<BossHealth> nearbyBosses = new HashSet<BossHealth>();

        foreach (Collider hit in hits)
        {
            BossHealth boss = hit.GetComponentInParent<BossHealth>();

            if (boss != null)
            {
                if (nearbyBosses.Add(boss))
                    StartCoroutine(RangedBossDamageRoutine(boss));

                continue;
            }

            ZombieHealth zombie = hit.GetComponentInParent<ZombieHealth>();

            if (zombie != null && nearbyZombies.Add(zombie))
                StartCoroutine(RangedDamageRoutine(zombie));
        }

        Debug.Log(nearbyZombies.Count == 0
            ? "X: ไม่มีซอมบี้อยู่ในระยะสกิล"
            : "X: สร้างวงเวทใส่ซอมบี้ " + nearbyZombies.Count + " ตัว");
    }

    private IEnumerator RangedBossDamageRoutine(BossHealth target)
    {
        if (target == null)
            yield break;

        Vector3 origin =
            target.transform.position + Vector3.up * groundRayHeight;

        Vector3 groundPosition;

        if (Physics.Raycast(origin, Vector3.down,
            out RaycastHit hit, groundRayDistance,
            groundLayers, QueryTriggerInteraction.Ignore))
        {
            groundPosition = hit.point + Vector3.up * groundOffset;
        }
        else
        {
            groundPosition =
                target.transform.position + Vector3.up * groundOffset;
        }

        GameObject visual = CreateGroundZone(
            groundPosition, zoneRadius, Color.magenta, 0.12f);

        float elapsed = 0f;
        float timer = 0f;

        while (elapsed < zoneDuration && target != null)
        {
            timer += Time.deltaTime;

            if (timer >= Mathf.Max(0.05f, zoneTick))
            {
                timer = 0f;

                if (Vector3.Distance(
                    target.transform.position, groundPosition)
                    <= zoneRadius + 2f)
                {
                    target.TakeDamage(zoneDamage);
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (visual != null)
            Destroy(visual);
    }
    private IEnumerator RangedDamageRoutine(ZombieHealth target)
    {
        if (target == null)
            yield break;

        Vector3 rayOrigin = target.transform.position + Vector3.up * groundRayHeight;
        Vector3 groundPosition;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit groundHit,
            groundRayDistance, groundLayers, QueryTriggerInteraction.Ignore))
        {
            groundPosition = groundHit.point + Vector3.up * groundOffset;
        }
        else
        {
            groundPosition = target.transform.position + Vector3.up * groundOffset;
            Debug.LogWarning("Magician X: ไม่พบพื้นใต้ " + target.name +
                             " จึงวางวงเวทที่ตำแหน่งซอมบี้");
        }

        GameObject visual = CreateGroundZone(groundPosition, zoneRadius, Color.magenta, 0.12f);
        float elapsed = 0f;
        float damageTimer = 0f;

        while (elapsed < zoneDuration && target != null)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= Mathf.Max(0.05f, zoneTick))
            {
                damageTimer = 0f;
                Collider[] areaHits = Physics.OverlapSphere(groundPosition, zoneRadius);
                HashSet<ZombieHealth> damaged = new HashSet<ZombieHealth>();

                foreach (Collider hit in areaHits)
                {
                    ZombieHealth zombie = hit.GetComponentInParent<ZombieHealth>();
                    if (zombie == null || !damaged.Add(zombie))
                        continue;
                    zombie.TakeDamage(zoneDamage, gameObject);
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (visual != null)
            Destroy(visual);
    }

    private IEnumerator CastBuff()
    {
        isBuffActive = true;
        buffEndTime = Time.time + buffDuration;
        SpeedMultiplier = buffSpeedMultiplier;
        DamageTakenMultiplier = buffDamageTakenMultiplier;
        RegenMultiplier = buffRegenMultiplier;

        CreateBuffVisual();
        Debug.Log("Magician buff activated!");
        yield return new WaitForSeconds(buffDuration);

        SpeedMultiplier = 1f;
        DamageTakenMultiplier = 1f;
        RegenMultiplier = 1f;
        isBuffActive = false;

        if (buffVisual != null)
            Destroy(buffVisual);

        buffVisual = null;
        buffRing = null;
        buffInnerRing = null;
        Debug.Log("Magician buff ended.");
    }

    private void CreateBuffVisual()
    {
        if (buffVisual != null)
            Destroy(buffVisual);

        buffVisual = new GameObject("MagicianBuffAura");
        buffVisual.transform.SetParent(transform, false);
        buffVisual.transform.localPosition = new Vector3(0f, buffVisualHeight, 0f);

        buffRing = CreateLine(buffVisual.transform, buffVisualRadius, buffRingWidth, buffColor);
        buffInnerRing = CreateLine(buffVisual.transform, buffVisualRadius * 0.72f,
            buffRingWidth * 0.65f, new Color(0.8f, 0.4f, 1f, 1f));

        for (int i = 0; i < 8; i++)
        {
            GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "MagicBuffOrb";
            orb.transform.SetParent(buffVisual.transform, false);
            orb.transform.localScale = Vector3.one * 0.13f;

            float angle = i * Mathf.PI * 2f / 8f;
            orb.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * buffVisualRadius, 0.35f,
                Mathf.Sin(angle) * buffVisualRadius);

            Collider orbCollider = orb.GetComponent<Collider>();
            if (orbCollider != null)
                Destroy(orbCollider);

            Renderer orbRenderer = orb.GetComponent<Renderer>();
            if (orbRenderer != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader == null) shader = Shader.Find("Unlit/Color");
                if (shader == null) shader = Shader.Find("Sprites/Default");

                if (shader != null)
                {
                    Material mat = new Material(shader);
                    mat.color = buffColor;
                    orbRenderer.material = mat;
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (!isBuffActive || buffVisual == null)
            return;

        float pulse = 1f + Mathf.Sin(Time.time * buffPulseSpeed) * 0.12f;
        buffVisual.transform.localScale = Vector3.one * pulse;
        buffVisual.transform.Rotate(0f, 100f * Time.deltaTime, 0f);

        float alpha = 0.55f + Mathf.PingPong(Time.time * 2f, 0.4f);
        if (buffRing != null)
        {
            Color c = buffColor;
            c.a = alpha;
            buffRing.startColor = c;
            buffRing.endColor = c;
        }

        if (buffInnerRing != null)
        {
            Color c = new Color(0.8f, 0.4f, 1f, alpha);
            buffInnerRing.startColor = c;
            buffInnerRing.endColor = c;
        }
    }

    private GameObject CreateRingVisual(Transform followTarget, float radius, Color color, float width)
    {
        GameObject ring = new GameObject("MagicRing");
        ring.transform.SetParent(followTarget, false);
        ring.transform.localPosition = new Vector3(0f, 0.1f, 0f);
        CreateLine(ring.transform, radius, width, color);
        return ring;
    }

    private LineRenderer CreateLine(Transform parent, float radius, float width, Color color)
    {
        GameObject lineObject = new GameObject("MagicCircle");
        lineObject.transform.SetParent(parent, false);

        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = RingSegments;
        line.startWidth = width;
        line.endWidth = width;
        line.numCapVertices = 4;
        line.numCornerVertices = 4;
        line.shadowCastingMode = ShadowCastingMode.Off;
        line.receiveShadows = false;

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");

        if (shader != null)
        {
            Material mat = new Material(shader);
            mat.color = color;
            line.material = mat;
        }

        line.startColor = color;
        line.endColor = color;

        for (int i = 0; i < RingSegments; i++)
        {
            float angle = i * Mathf.PI * 2f / RingSegments;
            line.SetPosition(i, new Vector3(
                Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
        }

        return line;
    }

    private GameObject CreateGroundZone(Vector3 position, float radius, Color color, float width)
    {
        GameObject zone = new GameObject("MagicianGroundZone");
        zone.transform.position = position;

        LineRenderer line = zone.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = RingSegments;
        line.startWidth = width;
        line.endWidth = width;
        line.shadowCastingMode = ShadowCastingMode.Off;
        line.receiveShadows = false;

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");

        if (shader != null)
        {
            Material material = new Material(shader);
            material.color = color;
            line.material = material;
        }

        line.startColor = color;
        line.endColor = color;

        for (int i = 0; i < RingSegments; i++)
        {
            float angle = i * Mathf.PI * 2f / RingSegments;
            line.SetPosition(i, new Vector3(
                Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
        }

        return zone;
    }

    private void SetupGUIStyles()
    {
        if (titleStyle != null)
            return;

        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 18;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.cyan;

        spellStyle = new GUIStyle(GUI.skin.label);
        spellStyle.fontSize = 14;
        spellStyle.fontStyle = FontStyle.Bold;
        spellStyle.normal.textColor = Color.white;

        statusStyle = new GUIStyle(GUI.skin.label);
        statusStyle.fontSize = 13;
        statusStyle.alignment = TextAnchor.MiddleRight;
        statusStyle.fontStyle = FontStyle.Bold;

        buffStyle = new GUIStyle(GUI.skin.label);
        buffStyle.fontSize = 13;
        buffStyle.fontStyle = FontStyle.Bold;
        buffStyle.normal.textColor = new Color(0.4f, 1f, 0.8f);
    }

    private void OnGUI()
    {
        if (!showCooldownUI || !IsMagician)
            return;

        SetupGUIStyles();

        float width = Mathf.Max(200f, uiSize.x);
        float height = Mathf.Max(120f, uiSize.y);
        float x = Screen.width - width - uiMargin;
        float y = Screen.height - height - uiMargin;

        GUI.Box(new Rect(x, y, width, height), GUIContent.none);
        GUI.Label(new Rect(x + 12, y + 7, width - 24, 25), "MAGICIAN SKILLS", titleStyle);

        DrawCooldownRow(x, y + 35, width, "Z", "Aura Ring", nextRingTime, ringCooldown, Color.cyan);
        DrawCooldownRow(x, y + 66, width, "X", "Ranged Zone", nextRangedTime, rangedCooldown, new Color(1f, 0.4f, 1f));
        DrawCooldownRow(x, y + 97, width, "C", "Orbital Buff", nextBuffTime, buffCooldown, new Color(0.4f, 1f, 0.8f));

        if (HasInfiniteMana)
        {
            GUI.Label(new Rect(x + 12, y + height - 22, width - 24, 18),
                "INFMANA ACTIVE", buffStyle);
        }
        else if (isBuffActive)
        {
            float remaining = Mathf.Max(0f, buffEndTime - Time.time);
            GUI.Label(new Rect(x + 12, y + height - 22, width - 24, 18),
                "BUFF ACTIVE " + remaining.ToString("F1") + "s", buffStyle);
        }
    }

    private void DrawCooldownRow(float x, float y, float width, string key,
        string spellName, float readyTime, float cooldown, Color color)
    {
        float remaining = Mathf.Max(0f, readyTime - Time.time);
        bool ready = HasInfiniteMana || remaining <= 0f;

        GUI.color = color;
        GUI.Box(new Rect(x + 10, y, 25, 25), key);
        GUI.color = Color.white;

        GUI.Label(new Rect(x + 43, y + 2, 105, 22), spellName, spellStyle);
        statusStyle.normal.textColor = ready ? Color.green : Color.yellow;

        string status = HasInfiniteMana ? "NO CD" : (ready ? "READY" : remaining.ToString("F1") + "s");
        GUI.Label(new Rect(x + width - 83, y + 2, 72, 22), status, statusStyle);

        float barWidth = width - 22f;
        float progress = cooldown > 0f ? 1f - Mathf.Clamp01(remaining / cooldown) : 1f;

        GUI.color = new Color(0.12f, 0.12f, 0.18f);
        GUI.DrawTexture(new Rect(x + 11, y + 25, barWidth, 3), Texture2D.whiteTexture);
        GUI.color = color;
        GUI.DrawTexture(new Rect(x + 11, y + 25, barWidth * progress, 3), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    private void CheckCheatCode()
    {
        foreach (char c in Input.inputString)
        {
            if (char.IsControl(c))
                continue;

            cheatBuffer += char.ToUpperInvariant(c);
            if (cheatBuffer.Length > InfiniteManaCode.Length)
                cheatBuffer = cheatBuffer.Substring(cheatBuffer.Length - InfiniteManaCode.Length);

            if (cheatBuffer == InfiniteManaCode)
            {
                infiniteMana = true;
                PlayerPrefs.SetInt("InfManaUnlocked", 1);
                PlayerPrefs.Save();
                cheatBuffer = "";

                nextRingTime = 0f;
                nextRangedTime = 0f;
                nextBuffTime = 0f;
                Debug.Log("INFMANA ACTIVATED! Magician cooldowns disabled.");
            }
        }
    }

    private void OnDisable()
    {
        SpeedMultiplier = 1f;
        DamageTakenMultiplier = 1f;
        RegenMultiplier = 1f;
        isBuffActive = false;

        if (buffVisual != null)
            Destroy(buffVisual);
    }
}
