using UnityEngine;
using Photon.Pun;

public class SkyPlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    public float speed = 10f;
    public float jumpForce = 5f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public SkyPlayerAnimation playerAnimation;

    private bool isGrounded;
    private int jumpCount = 0;
    private int maxJumps = 2;
    public float dashForce = 20f;
    private bool isDashing;

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
        Vector3 force = movementDirection * speed - rb.velocity;
        force.y = 0; // Ignore vertical component of the force to avoid affecting jumps
        if (!isGrounded && (x != 0 || z != 0))
        {
            // Apply a reduced force for air control
            Vector3 airControlForce = movementDirection * (speed * 0.5f) - rb.velocity;
            airControlForce.y = 0; // Avoid affecting vertical movement
            rb.AddForce(airControlForce, ForceMode.VelocityChange);
        }
        else
        {
            rb.AddForce(force, ForceMode.VelocityChange);
        }
    }

    public void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing)
        {
            Vector3 dashDirection = transform.forward; // Dash forward relative to player's facing direction
            rb.AddForce(dashDirection * dashForce, ForceMode.VelocityChange);
            isDashing = true;
            Invoke("ResetDash", 0.5f); // Cooldown before next dash
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
        // Check if the player is grounded
        if (isGrounded)
        {
            // Reset jump count when landing
            if (jumpCount > 0)
            {
                playerAnimation.ChangeAnimationState(SkyPlayerAnimation.PLAYER_LANDING);
                jumpCount = 0;
            }
            // Determine walking or idle based on horizontal movement
            else if (rb.velocity.magnitude > 0.1f && (Mathf.Abs(rb.velocity.x) > 0.1f || Mathf.Abs(rb.velocity.z) > 0.1f))
            {
                print("walking");
                playerAnimation.ChangeAnimationState(SkyPlayerAnimation.PLAYER_WALK);
            }

            else
            {
                playerAnimation.ChangeAnimationState(SkyPlayerAnimation.PLAYER_IDLE);
            }
        }
        else
        {
            // When not grounded, determine if the player is jumping or falling
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


    // Draws a Gizmo in the editor to visualize the ground check
    void OnDrawGizmos()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundDistance);
    }
}
