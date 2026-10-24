
using UnityEngine;

public class PlayerClassSystem : MonoBehaviour
{
    public static PlayerClassSystem Instance { get; private set; }

    public string SelectedClass { get; private set; }

    [Header("Reaper Bonuses")]
    public int reaperKills = 0;
    public int maxReaperKills = 30;
    public float damageBonusPerKill = 0.02f;
    public float speedBonusPerKill = 0.25f;
    public int maxHealthBonusPerKill = 5;
    public float regenBonusPerKill = 0.05f;

    public float DamageMultiplier =>
        SelectedClass == "Reaper"
            ? 1f + reaperKills * damageBonusPerKill
            : 1f;

    public float SpeedBonus =>
        SelectedClass == "Reaper"
            ? reaperKills * speedBonusPerKill
            : 0f;

    public float RegenBonus =>
        SelectedClass == "Reaper"
            ? reaperKills * regenBonusPerKill
            : 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        SelectedClass = PlayerPrefs.GetString(
            "SelectedClass", "Normal");

        Debug.Log("Selected Class: " + SelectedClass);
    }

    public bool IsClass(string className)
    {
        return SelectedClass == className;
    }


    public void RegisterZombieKill()
    {
        if (SelectedClass != "Reaper")
            return;

        if (reaperKills >= maxReaperKills)
            return;

        reaperKills++;

        PlayerHealth health = GetComponent<PlayerHealth>();

        if (health == null)
            health = GetComponentInParent<PlayerHealth>();

        if (health == null)
            health = GetComponentInChildren<PlayerHealth>();

        if (health != null)
        {
            health.AddMaxHealth(maxHealthBonusPerKill);
        }
        else
        {
            Debug.LogError(
                "Reaper: ไม่พบ PlayerHealth ในตัวผู้เล่น");
        }

        Debug.Log(
            "REAPER KILLS: " + reaperKills +
            "/" + maxReaperKills);
    }


    private void OnGUI()
    {
        if (SelectedClass != "Reaper")
            return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 22;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.LowerRight;

        GUI.Label(
            new Rect(
                Screen.width - 300,
                Screen.height - 75,
                280,
                45),
            "REAPER KILLS: " + reaperKills +
            " / " + maxReaperKills,
            style);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
