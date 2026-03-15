using UnityEngine;

/// <summary>
/// Swaps eye sprites based on movement and jump state.
/// Works without Animator and reuses existing controller scripts.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class EyeVisualController : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite idleSprite;
    public Sprite leftSprite;
    public Sprite rightSprite;
    public Sprite jumpSprite;
    public Sprite duckSprite;

    [Header("Input")]
    public KeyCode duckKey = KeyCode.S;
    public KeyCode duckAltKey = KeyCode.DownArrow;

    [Header("Tuning")]
    public float verticalVelocityThreshold = 0.08f;

    [Header("Left/Right Flip")]
    public bool useAutoFlip = true;
    public bool rightIsDefaultFacing = true;

    [Header("Compatibility")]
    public bool disableAnimatorOnStart = true;
    public bool useRawInputFallback = true;

    [Header("Procedural Pose")]
    public bool useProceduralVerticalPose = true;
    public float poseLerpSpeed = 14f;
    public float jumpStretchY = 1.14f;
    public float jumpSquashX = 0.9f;
    public float fallSquashY = 0.86f;
    public float fallStretchX = 1.08f;

    [Header("Blink")]
    public bool enableBlink = true;
    public float blinkIntervalMin = 2f;
    public float blinkIntervalMax = 4.5f;
    public float blinkDuration = 0.08f;
    public float blinkClosedYScale = 0.2f;
    public float blinkBulgeXScale = 1.08f;

    private SpriteRenderer spriteRenderer;
    private HorizontalMovementController moveController;
    private Rigidbody2D rb;
    private Animator animatorComponent;
    private Vector3 baseScale;
    private float blinkTimer;
    private float blinkRemaining;
    private bool isBlinking;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        moveController = GetComponent<HorizontalMovementController>();
        rb = GetComponent<Rigidbody2D>();
        animatorComponent = GetComponent<Animator>();
        baseScale = transform.localScale;
    }

    void Start()
    {
        if (disableAnimatorOnStart && animatorComponent != null)
            animatorComponent.enabled = false;

        ScheduleNextBlink();
    }

    void Update()
    {
        bool isDucking = Input.GetKey(duckKey) || Input.GetKey(duckAltKey);
        float moveInput = GetHorizontalInput();
        float velocityY = rb != null ? rb.linearVelocity.y : 0f;
        UpdateBlinkState();

        if (Mathf.Abs(moveInput) > 0.01f)
            ApplyFacing(moveInput);

        if (velocityY > verticalVelocityThreshold)
        {
            if (jumpSprite != null)
                SetSpriteIfAssigned(jumpSprite);
            else
                SetHorizontalOrIdleSprite(moveInput);
        }
        else if (velocityY < -verticalVelocityThreshold)
        {
            if (duckSprite != null)
                SetSpriteIfAssigned(duckSprite);
            else
                SetHorizontalOrIdleSprite(moveInput);
        }
        else if (isDucking)
        {
            if (duckSprite != null)
                SetSpriteIfAssigned(duckSprite);
            else
                SetHorizontalOrIdleSprite(moveInput);
        }
        else if (moveInput < -0.01f)
        {
            SetHorizontalSprite(false);
        }
        else if (moveInput > 0.01f)
        {
            SetHorizontalSprite(true);
        }
        else
        {
            SetSpriteIfAssigned(idleSprite);
        }

        ApplyProceduralPose(velocityY, isDucking);
    }

    void SetHorizontalOrIdleSprite(float moveInput)
    {
        if (moveInput < -0.01f)
            SetHorizontalSprite(false);
        else if (moveInput > 0.01f)
            SetHorizontalSprite(true);
        else
            SetSpriteIfAssigned(idleSprite);
    }

    float GetHorizontalInput()
    {
        float controllerInput = moveController != null ? moveController.CurrentInput : 0f;
        if (!useRawInputFallback)
            return controllerInput;

        float rawInput = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            rawInput -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            rawInput += 1f;

        if (Mathf.Abs(controllerInput) > 0.01f)
            return controllerInput;

        return rawInput;
    }

    void SetHorizontalSprite(bool movingRight)
    {
        if (movingRight)
        {
            if (rightSprite != null)
                spriteRenderer.sprite = rightSprite;
            else
                SetSpriteIfAssigned(leftSprite);
            return;
        }

        if (leftSprite != null)
            spriteRenderer.sprite = leftSprite;
        else
            SetSpriteIfAssigned(rightSprite);
    }

    void ApplyFacing(float moveInput)
    {
        if (!useAutoFlip)
            return;

        bool movingRight = moveInput > 0f;
        spriteRenderer.flipX = rightIsDefaultFacing ? !movingRight : movingRight;
    }

    void UpdateBlinkState()
    {
        if (!enableBlink)
        {
            isBlinking = false;
            return;
        }

        if (isBlinking)
        {
            blinkRemaining -= Time.deltaTime;
            if (blinkRemaining <= 0f)
            {
                isBlinking = false;
                ScheduleNextBlink();
            }
            return;
        }

        blinkTimer -= Time.deltaTime;
        if (blinkTimer <= 0f)
        {
            isBlinking = true;
            blinkRemaining = blinkDuration;
        }
    }

    void ScheduleNextBlink()
    {
        float minInterval = Mathf.Max(0.1f, blinkIntervalMin);
        float maxInterval = Mathf.Max(minInterval, blinkIntervalMax);
        blinkTimer = Random.Range(minInterval, maxInterval);
    }

    void ApplyProceduralPose(float velocityY, bool isDucking)
    {
        float targetX = baseScale.x;
        float targetY = baseScale.y;

        if (useProceduralVerticalPose)
        {
            if (velocityY > verticalVelocityThreshold)
            {
                targetX *= jumpSquashX;
                targetY *= jumpStretchY;
            }
            else if (velocityY < -verticalVelocityThreshold || isDucking)
            {
                targetX *= fallStretchX;
                targetY *= fallSquashY;
            }
        }

        if (enableBlink && isBlinking)
        {
            targetX *= blinkBulgeXScale;
            targetY *= blinkClosedYScale;
        }

        Vector3 targetScale = new Vector3(targetX, targetY, baseScale.z);
        float speed = Mathf.Max(1f, poseLerpSpeed);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, speed * Time.deltaTime);
    }

    void SetSpriteIfAssigned(Sprite sprite)
    {
        if (sprite != null)
            spriteRenderer.sprite = sprite;
    }
}
