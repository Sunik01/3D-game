
using UnityEngine;

public class PlayerPotionEffects : MonoBehaviour
{
    public static PlayerPotionEffects Instance { get; private set; }

    [Header("Potion Duration")]
    public float duration = 60f;

    [Header("Active Effects")]
    public bool speedActive;
    public bool resistanceActive;
    public bool damageActive;

    private float speedTimer;
    private float resistanceTimer;
    private float damageTimer;

    public float SpeedMultiplier => speedActive ? 1.5f : 1f;
    public float DamageMultiplier => damageActive ? 1.5f : 1f;
    public float DamageTakenMultiplier => resistanceActive ? 0.75f : 1f;
    public float SpeedTimeLeft => speedTimer;
    public float ResistanceTimeLeft => resistanceTimer;
    public float DamageTimeLeft => damageTimer;

    public bool IsSpeedActive => speedActive;
    public bool IsResistanceActive => resistanceActive;
    public bool IsDamageActive => damageActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (speedActive)
        {
            speedTimer -= Time.deltaTime;

            if (speedTimer <= 0f)
                speedActive = false;
        }

        if (resistanceActive)
        {
            resistanceTimer -= Time.deltaTime;

            if (resistanceTimer <= 0f)
                resistanceActive = false;
        }

        if (damageActive)
        {
            damageTimer -= Time.deltaTime;

            if (damageTimer <= 0f)
                damageActive = false;
        }
    }

    public void DrinkSpeedPotion()
    {
        speedActive = true;
        speedTimer = duration;
        Debug.Log("Speed Potion: +50% speed for 60 seconds");
    }

    public void DrinkResistancePotion()
    {
        resistanceActive = true;
        resistanceTimer = duration;
        Debug.Log("Resistance Potion: -25% incoming damage for 60 seconds");
    }

    public void DrinkDamagePotion()
    {
        damageActive = true;
        damageTimer = duration;
        Debug.Log("Damage Potion: +50% damage for 60 seconds");
    }

    public float GetSpeedRemaining()
    {
        return speedActive ? Mathf.Max(0f, speedTimer) : 0f;
    }

    public float GetResistanceRemaining()
    {
        return resistanceActive ? Mathf.Max(0f, resistanceTimer) : 0f;
    }

    public float GetDamageRemaining()
    {
        return damageActive ? Mathf.Max(0f, damageTimer) : 0f;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
