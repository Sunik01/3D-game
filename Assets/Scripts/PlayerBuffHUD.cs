
using UnityEngine;

public class PlayerBuffHUD : MonoBehaviour
{
    public PlayerPotionEffects potionEffects;

    private GUIStyle boxStyle;
    private GUIStyle titleStyle;
    private GUIStyle timerStyle;

    private void Start()
    {
        if (potionEffects == null)
            potionEffects = PlayerPotionEffects.Instance;
    }

    private void OnGUI()
    {
        if (potionEffects == null)
            potionEffects = PlayerPotionEffects.Instance;

        if (potionEffects == null)
            return;

        if (boxStyle == null)
        {
            boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.fontSize = 16;
            boxStyle.alignment = TextAnchor.MiddleLeft;
            boxStyle.normal.textColor = Color.white;

            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 17;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = Color.white;

            timerStyle = new GUIStyle(GUI.skin.label);
            timerStyle.fontSize = 16;
            timerStyle.alignment = TextAnchor.MiddleRight;
            timerStyle.normal.textColor = Color.white;
        }

        float x = 15f;
        float rowHeight = 42f;
        float width = 245f;
        float gap = 7f;

        // วางแถบบัพจากล่างขึ้นบน
        float bottom = Screen.height - 15f;

        if (potionEffects.IsSpeedActive)
        {
            DrawBuff(x, bottom - rowHeight,
                width, rowHeight, gap,
                "Speed Boost",
                potionEffects.SpeedTimeLeft,
                new Color(0.10f, 0.35f, 0.65f));

            bottom -= rowHeight + gap;
        }

        if (potionEffects.IsResistanceActive)
        {
            DrawBuff(x, bottom - rowHeight,
                width, rowHeight, gap,
                "Resistance",
                potionEffects.ResistanceTimeLeft,
                new Color(0.08f, 0.40f, 0.28f));

            bottom -= rowHeight + gap;
        }

        if (potionEffects.IsDamageActive)
        {
            DrawBuff(x, bottom - rowHeight,
                width, rowHeight, gap,
                "Damage Boost",
                potionEffects.DamageTimeLeft,
                new Color(0.60f, 0.18f, 0.18f));
        }
    }

    private void DrawBuff(
        float x, float y, float width, float height,
        float gap, string buffName, float timeLeft,
        Color backgroundColor)
    {
        Color oldColor = GUI.color;
        GUI.color = backgroundColor;

        GUI.Box(new Rect(x, y, width, height), GUIContent.none);

        GUI.color = oldColor;

        GUI.Label(
            new Rect(x + 9f, y, width - 78f, height),
            buffName, titleStyle);

        GUI.Label(
            new Rect(x + width - 70f, y, 62f, height),
            Mathf.CeilToInt(Mathf.Max(0f, timeLeft)) + "s",
            timerStyle);
    }
}
