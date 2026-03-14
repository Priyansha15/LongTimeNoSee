using UnityEngine;

public class EyePlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public int maxJumps = 3;

    public float floatHeight = 0.85f;
    public float ascendSpeed = 4f;
    public float descendSpeed = 4f;
    public float peakGraceTime = 0.12f;

    private Rigidbody2D rb;
    private float homeY;

    private int requestedJumps = 0;
    private bool isFloating = false;
    // NEW: tracks whether a NEW jump was pressed during descent/grace
    private bool newJumpWhileDescending = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        homeY = transform.position.y;
    }

    void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(horizontalInput) > 0.001f)
        {
            Vector3 pos = transform.position;
            pos.x += horizontalInput * moveSpeed * Time.deltaTime;
            transform.position = pos;
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
        {
            if (requestedJumps < maxJumps)
            {
                requestedJumps++;
                newJumpWhileDescending = true; // signal to coroutine
            }

            if (!isFloating)
            {
                isFloating = true;
                StartCoroutine(FloatSequence());
            }
        }
    }

    private System.Collections.IEnumerator FloatSequence()
    {
        while (true)
        {
            // Snapshot the target for THIS ascent — don't recalculate mid-flight
            float ascendTargetY = homeY + floatHeight * requestedJumps;
            newJumpWhileDescending = false;

            // --- ASCEND ---
            while (transform.position.y < ascendTargetY - 0.001f)
            {
                // If new jump arrived mid-ascent, update the target upward
                if (newJumpWhileDescending)
                {
                    ascendTargetY = homeY + floatHeight * requestedJumps;
                    newJumpWhileDescending = false;
                }
                float newY = Mathf.MoveTowards(transform.position.y, ascendTargetY, ascendSpeed * Time.deltaTime);
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
                yield return null;
            }

            // --- GRACE WINDOW at peak ---
            float grace = 0f;
            while (grace < peakGraceTime)
            {
                if (newJumpWhileDescending)
                    break; // new jump pressed — restart ascent loop with updated requestedJumps
                grace += Time.deltaTime;
                yield return null;
            }

            // If new jump came in during grace, loop back up
            if (newJumpWhileDescending)
                continue;

            // --- DESCEND ---
            while (transform.position.y > homeY + 0.001f)
            {
                if (newJumpWhileDescending)
                    break; // new jump pressed — exit descent and re-enter ascend

                float newY = Mathf.MoveTowards(transform.position.y, homeY, descendSpeed * Time.deltaTime);
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
                yield return null;
            }

            // If interrupted by a new jump, loop back to ascend
            if (newJumpWhileDescending)
                continue;

            // Sequence complete
            break;
        }

        transform.position = new Vector3(transform.position.x, homeY, transform.position.z);
        requestedJumps = 0;
        isFloating = false;
    }
}   