using UnityEngine;

/// <summary>
/// Keeps Scene 1 background dark with a subtle breathing tone.
/// Attach to Main Camera.
/// </summary>
public class DarkSceneController : MonoBehaviour
{
    [Header("Camera")]
    public Camera targetCamera;

    [Header("Dark Theme")]
    public Color baseDarkColor = new Color(0.03f, 0.04f, 0.08f, 1f);
    public Color pulseDarkColor = new Color(0.05f, 0.06f, 0.1f, 1f);
    public float pulseSpeed = 0.2f;
    public bool animatePulse = true;

    void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        ApplyColor(0f);
    }

    void Update()
    {
        if (targetCamera == null)
            return;

        if (!animatePulse)
        {
            ApplyColor(0f);
            return;
        }

        float t = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f);
        ApplyColor(t);
    }

    void ApplyColor(float t)
    {
        if (targetCamera == null)
            return;

        targetCamera.backgroundColor = Color.Lerp(baseDarkColor, pulseDarkColor, t);
    }
}
