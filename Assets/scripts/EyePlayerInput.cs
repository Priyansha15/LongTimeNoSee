using UnityEngine;

/// <summary>
/// Reads player input and forwards it to movement/jump controllers.
/// This keeps character logic reusable across different character prefabs.
/// </summary>
[RequireComponent(typeof(FloatingJumpController))]
[RequireComponent(typeof(HorizontalMovementController))]
public class EyePlayerInput : MonoBehaviour
{
    private FloatingJumpController jumpController;
    private HorizontalMovementController movementController;

    void Start()
    {
        jumpController = GetComponent<FloatingJumpController>();
        movementController = GetComponent<HorizontalMovementController>();

        if (jumpController == null)
            Debug.LogWarning("FloatingJumpController not found on this GameObject.");

        if (movementController == null)
            Debug.LogWarning("HorizontalMovementController not found on this GameObject.");
    }

    void Update()
    {
        float horizontal = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            horizontal -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            horizontal += 1f;

        if (movementController != null)
            movementController.SetInput(horizontal);

        bool jumpHeld = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);

        if (jumpController != null)
        {
            jumpController.SetJumpHeld(jumpHeld);

            if (jumpPressed)
                jumpController.QueueJump();
        }
    }
}