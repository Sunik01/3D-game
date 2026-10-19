
using UnityEngine;
using UnityEngine.SceneManagement;

public class LavaDeath : MonoBehaviour
{
    private bool gameOver = false;

    void OnTriggerEnter(Collider other)
    {
        if (gameOver) return;

        if (other.CompareTag("Player"))
        {
            gameOver = true;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void Update()
    {
        if (gameOver && Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(
                SceneManager.GetActiveScene().buildIndex
            );
        }
    }

    void OnGUI()
    {
        if (!gameOver) return;

        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 60;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = Color.red;

        GUIStyle textStyle = new GUIStyle();
        textStyle.fontSize = 24;
        textStyle.alignment = TextAnchor.MiddleCenter;
        textStyle.normal.textColor = Color.white;

        GUI.Box(
            new Rect(0, 0, Screen.width, Screen.height), ""
        );

        GUI.Label(
            new Rect(0, Screen.height / 2 - 100,
            Screen.width, 100),
            "GAME OVER", titleStyle
        );

        GUI.Label(
            new Rect(0, Screen.height / 2,
            Screen.width, 60),
            "You fell into the lava!",
            textStyle
        );

        GUI.Label(
            new Rect(0, Screen.height / 2 + 60,
            Screen.width, 60),
            "Press R to Restart",
            textStyle
        );
    }
}
