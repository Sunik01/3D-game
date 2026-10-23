
using UnityEngine;
using System.Collections;

public class DamagePopup : MonoBehaviour
{
    public static void Create(Vector3 position, string message)
    {
        GameObject popup = new GameObject("DamagePopup");
        popup.transform.position = position;

        TextMesh text = popup.AddComponent<TextMesh>();
        text.text = message;
        text.fontSize = 64;
        text.characterSize = 0.04f;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = new Color(1f, 0.2f, 0.15f, 1f);
        text.fontStyle = FontStyle.Bold;

        popup.AddComponent<DamagePopup>().StartAnimation();
    }

    private void StartAnimation()
    {
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        TextMesh text = GetComponent<TextMesh>();
        Camera cam = Camera.main;

        Vector3 startPosition = transform.position;
        float duration = 0.8f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            transform.position =
                startPosition + Vector3.up * (t * 0.8f);

            if (cam == null)
                cam = Camera.main;

            if (cam != null)
                transform.rotation = cam.transform.rotation;

            if (text != null)
            {
                Color c = text.color;
                c.a = 1f - t;
                text.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
