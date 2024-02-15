using UnityEngine;
using Photon.Pun;

public class SkyPlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    public float speed = 10f;
    public float jumpForce = 5f;
    public float runMultiplier = 1.5f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public SkyPlayerAnimation playerAnimation;

    private bool isGrounded;
    private int jumpCount = 0;
    private int maxJumps = 2;
    public float dashForce = 20f;
    private bool isDashing;
    private bool isRunning;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
    }

    public void ResetJump()
    {
        isGrounded = IsGrounded();
        if (isGrounded)
            jumpCount = 0;

    }

    bool IsGrounded()
    {
        return Physics.Raycast(groundCheck.position, Vector3.down, groundDistance, groundLayer);
    }
    public void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 movementDirection = transform.right * x + transform.forward * z;
        float currentSpeed = speed;

        // Check for running
        isRunning = Input.GetKey(KeyCode.LeftControl);
        if (isRunning)
        {
            currentSpeed *= runMultiplier;
        }

        Vector3 force = movementDirection.normalized * currentSpeed - rb.velocity;
        force.y = 0;
        rb.AddForce(force, ForceMode.VelocityChange);
    }

    public void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing)
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            Vector3 dashDirection = transform.right * x + transform.forward * z;

            if (dashDirection.magnitude > 0) // Ensure there's some direction for the dash
            {
                rb.AddForce(dashDirection.normalized * dashForce, ForceMode.VelocityChange);
                isDashing = true;
                playerAnimation.ChangeAnimationState(SkyPlayerAnimation.PLAYER_DASH); // Trigger dash animation
                Invoke("ResetDash", 0.5f); // Cooldown before next dash
            }
        }
    }

    private void ResetDash()
    {
        isDashing = false;
    }
    public void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && (isGrounded || jumpCount < maxJumps - 1))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpCount++;
        }
    }
    public void UpdateAnimationState()
    {
        if (!isDashing) // Ensure dash animation transitions smoothly to other states
        {
            // Check if the player is grounded
            if (isGrounded)
            {
                if (jumpCount > 0)
                {
                    playerAnimation.ChangeAnimationState(SkyPlayerAnimation.PLAYER_LANDING);
                    jumpCount = 0;
                }
                else if (rb.velocity.magnitude > 0.1f && (Mathf.Abs(rb.velocity.x) > 0.1f || Mathf.Abs(rb.velocity.z) > 0.1f))
                {
                    if (isRunning)
                    {
                        // Transition to running animation if moving and Left Control is pressed
                        playerAnimation.ChangeAnimationState(SkyPlayerAnimation.PLAYER_RUN);
                    }
                    else
                    {
                        // Walking animation
                        playerAnimation.ChangeAnimationState(SkyPlayerAnimation.PLAYER_WALK);
                    }
                }
                else
                {
                    // Idle animation
                    playerAnimation.ChangeAnimationState(SkyPlayerAnimation.PLAYER_IDLE);
                }
            }
            else
            {
                if (rb.velocity.y > 0)
                {
                    playerAnimation.ChangeAnimationState(SkyPlayerAnimation.PLAYER_JUMP);
                }
                else if (rb.velocity.y < 0)
                {
                    playerAnimation.ChangeAnimationState(SkyPlayerAnimation.PLAYER_FALL);
                }
            }
        }
    }
    // Draws a Gizmo in the editor to visualize the ground check
    void OnDrawGizmos()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundDistance);
    }
}
