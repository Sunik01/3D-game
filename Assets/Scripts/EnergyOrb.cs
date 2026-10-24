
using UnityEngine;

public class EnergyOrb : MonoBehaviour
{
    public int moneyReward = 150;
    private bool collected;

    private void OnTriggerEnter(Collider other)
    {
        if (collected)
            return;

        if (!other.CompareTag("Player") &&
            !other.transform.root.CompareTag("Player"))
            return;

        if (MoneyManager.Instance == null)
        {
            Debug.LogError("ไม่พบ MoneyManager!");
            return;
        }

        collected = true;

        string selectedClass =
            PlayerPrefs.GetString("SelectedClass", "Normal");

        int reward = moneyReward;

        // Maze Runner ได้เงินจาก Orb เพิ่ม 50%
        if (selectedClass == "MazeRunner")
            reward = Mathf.RoundToInt(moneyReward * 1.5f);

        MoneyManager.Instance.AddMoney(reward);

        EnergyOrbSpawner spawner =
            FindFirstObjectByType<EnergyOrbSpawner>();

        if (spawner != null)
        {
            spawner.CollectOrb(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
