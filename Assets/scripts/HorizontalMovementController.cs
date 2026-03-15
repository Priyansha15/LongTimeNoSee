using UnityEngine;

/// <summary>
/// Handles horizontal movement independent of character or jump logic.
/// Attach to any character with a Transform and call Move(direction) to move.
/// </summary>
public class HorizontalMovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Screen Clamp")]
    public bool keepInsideScreen = true;
    public Camera targetCamera;

    private Rigidbody2D rb;
    private float moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        ApplyHorizontalMovement();
    }

    void LateUpdate()
    {
        if (keepInsideScreen)
            ClampInsideScreen();
    }

    /// <summary>
    /// Move the character horizontally.
    /// direction: -1 (left), 0 (no move), 1 (right)
    /// </summary>
    public void Move(float direction)
    {
        SetInput(direction);
    }

    /// <summary>
    /// Sets horizontal input from -1 to 1.
    /// </summary>
    public void SetInput(float direction)
    {
        moveInput = Mathf.Clamp(direction, -1f, 1f);
    }

    /// <summary>
    /// Directly set horizontal velocity via Rigidbody2D if available.
    /// Useful for physics-based movement.
    /// </summary>
    public void SetVelocityX(float velocityX)
    {
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(velocityX, rb.linearVelocity.y);
        }
    }

    void ApplyHorizontalMovement()
    {
        float vx = moveInput * moveSpeed;

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);
        }
        else
        {
            transform.position += Vector3.right * vx * Time.fixedDeltaTime;
        }
    }

    void ClampInsideScreen()
    {
        Camera cam = targetCamera != null ? targetCamera : Camera.main;
        if (cam == null)
            return;

        float depth = Mathf.Abs(transform.position.z - cam.transform.position.z);
        if (depth < 0.01f)
            depth = cam.nearClipPlane + 1f;

        float halfWidth = GetHalfWidth();
        float minX = cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, depth)).x + halfWidth;
        float maxX = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, depth)).x - halfWidth;

        Vector3 pos = transform.position;
        float clampedX = Mathf.Clamp(pos.x, minX, maxX);
        if (Mathf.Abs(clampedX - pos.x) <= Mathf.Epsilon)
            return;

        pos.x = clampedX;
        transform.position = pos;

        if (rb != null)
            rb.position = new Vector2(clampedX, rb.position.y);
    }

    float GetHalfWidth()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            return col.bounds.extents.x;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            return sr.bounds.extents.x;

        return 0f;
    }
}
