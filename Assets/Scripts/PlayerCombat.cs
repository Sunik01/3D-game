using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCombat : MonoBehaviour
{
    public Camera playerCamera;
    public Transform sword;

    public int swordDamage = 25;
    public float attackRange = 3f;
    public float attackCooldown = 0.5f;
    public float swingDuration = 0.25f;

    public Vector3 swingRotation =
        new Vector3(-70f, 70f, 0f);

    private bool canAttack = true;
    private Quaternion swordStartRotation;

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        if (sword != null)
            swordStartRotation = sword.localRotation;

        string selectedClass =
            PlayerPrefs.GetString("SelectedClass", "Normal");

        if (selectedClass == "Elite")
            swordDamage = 35;
        else if (selectedClass == "GodSword")
            swordDamage = 50;
        else
            swordDamage = 25;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && canAttack)
            StartCoroutine(SwingSword());
    }

    private IEnumerator SwingSword()
    {
        canAttack = false;

        if (sword != null)
        {
            float elapsed = 0f;

            Quaternion targetRotation =
                swordStartRotation *
                Quaternion.Euler(swingRotation);

            while (elapsed < swingDuration / 2f)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(
                    elapsed / (swingDuration / 2f));

                sword.localRotation = Quaternion.Slerp(
                    swordStartRotation, targetRotation, t);

                yield return null;
            }

            DealSwordDamage();

            elapsed = 0f;

            while (elapsed < swingDuration / 2f)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(
                    elapsed / (swingDuration / 2f));

                sword.localRotation = Quaternion.Slerp(
                    targetRotation, swordStartRotation, t);

                yield return null;
            }

            sword.localRotation = swordStartRotation;
        }
        else
        {
            DealSwordDamage();
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private void DealSwordDamage()
    {
        Transform origin = playerCamera != null
            ? playerCamera.transform : transform;

        Vector3 center = origin.position +
            origin.forward * (attackRange * 0.55f);

        Collider[] hits = Physics.OverlapSphere(center, 1.5f);

        HashSet<ZombieHealth> hitZombies =
            new HashSet<ZombieHealth>();

        foreach (Collider hit in hits)
        {
            ZombieHealth zombie =
                hit.GetComponentInParent<ZombieHealth>();

            if (zombie == null || hitZombies.Contains(zombie))
                continue;

            Vector3 direction =
                zombie.transform.position - origin.position;

            if (direction.sqrMagnitude > 0.001f &&
                Vector3.Angle(origin.forward, direction) > 70f)
                continue;

            hitZombies.Add(zombie);

            float damageMultiplier = 1f;

            if (PlayerPotionEffects.Instance != null)
            {
                damageMultiplier *=
                    PlayerPotionEffects.Instance.DamageMultiplier;
            }

            PlayerClassSystem classSystem =
            GetComponentInParent<PlayerClassSystem>();

            if (classSystem == null)
                classSystem = GetComponentInChildren<PlayerClassSystem>();

            if (classSystem == null)
                classSystem = PlayerClassSystem.Instance;

            if (classSystem != null)
                damageMultiplier *= classSystem.DamageMultiplier;

            int finalDamage = Mathf.Max(
                1, Mathf.RoundToInt(swordDamage * damageMultiplier));

            //  ËßºŸÈ‡≈Ëπ∑’Ë‚®¡µ’‰ª¥È«¬
            PlayerClassSystem ownerClassSystem =
    GetComponentInParent<PlayerClassSystem>();

            if (ownerClassSystem == null)
                ownerClassSystem = GetComponentInChildren<PlayerClassSystem>();

            if (ownerClassSystem == null)
                ownerClassSystem = PlayerClassSystem.Instance;

            GameObject attackerObject = ownerClassSystem != null
                ? ownerClassSystem.gameObject
                : gameObject;

            zombie.TakeDamage(finalDamage, attackerObject);

            Debug.Log("Sword hit Zombie! Damage: " + finalDamage);

            if (PlayerPrefs.GetString(
                "SelectedClass", "Normal") == "Vampire")
            {
                int healAmount =
                    Mathf.FloorToInt(finalDamage * 0.05f);

                PlayerHealth health = GetComponent<PlayerHealth>();

                if (health != null && healAmount > 0)
                    health.Heal(healAmount);
            }
        }
    }
}