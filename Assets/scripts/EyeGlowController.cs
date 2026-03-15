using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Adds a subtle glow pulse to the eye sprite.
/// Attach to the eye/player object.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class EyeGlowController : MonoBehaviour
{
    [Header("Sprite Glow")]
    public Color baseTint = Color.white;
    public Color glowTint = new Color(1f, 0.9f, 0.9f, 1f);
    public float colorPulseSpeed = 1.5f;
    public bool animateSpriteTint = true;

    [Header("2D Light Glow")]
    public bool usePointLight = true;
    public Light2D glowLight;
    public Color lightColor = new Color(1f, 0.25f, 0.25f, 1f);
    public float lightIntensity = 0.45f;
    public float lightPulseAmount = 0.12f;
    public float lightPulseSpeed = 1.8f;
    public float lightRadius = 1.1f;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (usePointLight)
            EnsureGlowLight();
    }

    void Update()
    {
        float t = 0.5f + 0.5f * Mathf.Sin(Time.time * colorPulseSpeed * Mathf.PI * 2f);

        if (animateSpriteTint)
            spriteRenderer.color = Color.Lerp(baseTint, glowTint, t);
        else
            spriteRenderer.color = baseTint;

        if (usePointLight && glowLight != null)
        {
            glowLight.color = lightColor;
            glowLight.pointLightOuterRadius = lightRadius;

            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * lightPulseSpeed * Mathf.PI * 2f);
            glowLight.intensity = lightIntensity + (pulse - 0.5f) * 2f * lightPulseAmount;
        }
    }

    void EnsureGlowLight()
    {
        if (glowLight != null)
            return;

        glowLight = GetComponentInChildren<Light2D>();
        if (glowLight != null)
            return;

        GameObject lightObject = new GameObject("EyeGlowLight");
        lightObject.transform.SetParent(transform, false);
        lightObject.transform.localPosition = Vector3.zero;

        glowLight = lightObject.AddComponent<Light2D>();
        glowLight.lightType = Light2D.LightType.Point;
        glowLight.pointLightOuterRadius = lightRadius;
        glowLight.intensity = lightIntensity;
        glowLight.color = lightColor;
        glowLight.overlapOperation = Light2D.OverlapOperation.Additive;
    }
}
