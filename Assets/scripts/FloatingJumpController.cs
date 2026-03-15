using UnityEngine;

/// <summary>
/// Physics-based floating jump for an eye/floating character.
/// 
/// HOW IT WORKS:
///   - Idle             → gravity OFF, homeSnap holds character at homeY (true floating)
///   - Jump press       → gravity ON, instant upward velocity (snappy arc)
///   - Button held      → normal gravity (full arc)
///   - Button released  → gravity multiplied (short hop)
///   - Falling down     → gravity multiplied (fast fall)
///   - Returns to homeY → gravity OFF again, back to floating idle
/// </summary>
public class FloatingJumpController : MonoBehaviour
{
    [Header("Jump")]
    public int maxJumps = 3;
    public float jumpForce = 7f;

    [Header("Gravity Tuning")]
    public float jumpGravityScale = 3f;    // gravity ONLY during jump arc
    public float fallMultiplier = 2.2f;
    public float lowJumpMultiplier = 1.8f;

    [Header("Floating Idle")]
    public float homeSnapStrength = 8f;    // spring strength back to homeY
    public float homeSnapDeadzone = 0.04f; // snap-to distance before killing velocity

    [Header("Jump Buffer & Coyote Time")]
    public float jumpBufferTime = 0.12f;
    public float coyoteTime = 0.1f;

    private Rigidbody2D rb;
    private float homeY;
    private int jumpsUsed = 0;
    private bool isJumping = false;

    private float jumpBufferCounter = 0f;
    private float coyoteCounter = 0f;
    private bool jumpHeld = false;
    private bool jumpQueued = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Start with gravity OFF — eye floats at spawn position
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;

        homeY = transform.position.y;
    }

    void Update()
    {
        if (jumpQueued)
        {
            jumpBufferCounter = jumpBufferTime;
            jumpQueued = false;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // ── Track return to homeY ────────────────────────────────────────
        if (isJumping)
        {
            bool descending = rb.linearVelocity.y <= 0f;
            bool nearHome = Mathf.Abs(transform.position.y - homeY) <= homeSnapDeadzone && descending;
            bool crossedBelowHome = transform.position.y <= homeY && descending;

            if (nearHome || crossedBelowHome)
                ReturnToIdle();
            else
                coyoteCounter -= Time.deltaTime;
        }
        else
        {
            // Idle — keep coyote window open so first jump always works
            coyoteCounter = coyoteTime;
        }

        // ── Attempt jump ─────────────────────────────────────────────────
        bool canJump = (coyoteCounter > 0f && jumpsUsed == 0)
                    || (jumpsUsed > 0 && jumpsUsed < maxJumps);

        if (jumpBufferCounter > 0f && canJump)
        {
            DoJump();
            jumpBufferCounter = 0f;
        }
    }

    void FixedUpdate()
    {
        if (isJumping)
            ApplyGravityMultiplier();
        else
            ApplyHomeSnap();
    }

    // ── Jump: enable gravity and fire upward velocity ────────────────────────
    void DoJump()
    {
        rb.gravityScale = jumpGravityScale; // turn gravity ON for the arc
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpsUsed++;
        isJumping = true;
        coyoteCounter = 0f;
    }

    // ── Return to idle float: disable gravity, reset state ──────────────────
    void ReturnToIdle()
    {
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        transform.position = new Vector3(transform.position.x, homeY, transform.position.z);
        jumpsUsed = 0;
        isJumping = false;
    }

    // ── Asymmetric gravity during jump arc ───────────────────────────────────
    void ApplyGravityMultiplier()
    {
        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y
                                * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0f && !jumpHeld)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y
                                * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    // ── Idle hover: spring back to homeY if nudged ───────────────────────────
    void ApplyHomeSnap()
    {
        float dist = homeY - transform.position.y;
        if (Mathf.Abs(dist) > homeSnapDeadzone)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x,
                dist * homeSnapStrength * Time.fixedDeltaTime);
        }
        else
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            transform.position = new Vector3(transform.position.x, homeY, transform.position.z);
        }
    }

    /// <summary>Returns true if currently in a jump arc.</summary>
    public bool IsJumping() => isJumping;

    /// <summary>Returns jumps remaining in current sequence.</summary>
    public int GetRemainingJumps() => maxJumps - jumpsUsed;

    /// <summary>Queues a jump request (called by external input script).</summary>
    public void QueueJump() => jumpQueued = true;

    /// <summary>Sets whether jump button is currently held (for low-jump cutoff).</summary>
    public void SetJumpHeld(bool held) => jumpHeld = held;
}