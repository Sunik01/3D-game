
using UnityEngine;

public class HyperZombieZoneVisual : MonoBehaviour
{
    public float radius = 3f;
    public float height = 0.04f;

    private void Start()
    {
        CreateRing("Outer White Ring", radius, 0.12f, Color.white);
        CreateRing("Inner Black Ring", radius * 0.78f, 0.12f, Color.black);
        CreateRing("Center White Ring", radius * 0.48f, 0.10f, Color.white);
    }

    private void CreateRing(
        string ringName, float ringRadius,
        float thickness, Color color)
    {
        GameObject ring = new GameObject(ringName);
        ring.transform.SetParent(transform, false);
        ring.transform.localPosition = new Vector3(0f, height, 0f);
        ring.transform.localRotation = Quaternion.identity;

        LineRenderer line = ring.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = 96;
        line.startWidth = thickness;
        line.endWidth = thickness;

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        Material material = new Material(shader);
        material.color = color;
        line.material = material;
        line.startColor = color;
        line.endColor = color;

        for (int i = 0; i < line.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f / line.positionCount;
            line.SetPosition(i, new Vector3(
                Mathf.Cos(angle) * ringRadius,
                0f,
                Mathf.Sin(angle) * ringRadius
            ));
        }
    }
}
