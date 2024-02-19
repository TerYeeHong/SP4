using UnityEngine;
using Photon.Pun;

public class SkyPlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    public float speed = 10f;
    public float deceleration = 5f;
    public float jumpForce = 5f;
    public float runMultiplier = 1.5f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public SkyPlayerAnimation playerAnimation;

    public float maxSlideTime = 2f; // Duration of the slide
    public float slideForce = 10f; // Force applied during sliding
    private bool isSliding = false;
    private float slideTimer;
    public float slideYScale = 0.4f; // Adjust player height during slide
    private float startYScale = 0.8f; // Original Y scale of the player

    private bool isGrounded;
    private int jumpCount = 0;
    private int maxJumps = 2;
    public float dashForce = 20f;
    private bool isDashing;
    private bool isRunning;
    public float groundDrag = 5f;
    public float airResistance = 2f;
    public float stopDrag = 10f; 
    public float maxSpeed = 5f;
    float horizontalInput;
    float verticalInput;
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
    public void SlidingMovement()
    {
        if (isSliding)
        {
            if (slideTimer > 0)
            {
                Vector3 inputDirection = transform.forward * verticalInput + transform.right * horizontalInput;
                rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);
                slideTimer -= Time.fixedDeltaTime;
            }
            else
            {
                StopSlide();
            }
        }
    }
    public void SlideCheck()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && !isSliding && (horizontalInput != 0 || verticalInput != 0))
        {
            StartSlide();
        }
        if (Input.GetKeyUp(KeyCode.LeftControl) && isSliding)
        {
            StopSlide();
        }
    }
    private void StartSlide()
    {
        isSliding = true;
        transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse); // Optional, for enhanced sliding effect
        slideTimer = maxSlideTime;
    }

    private void StopSlide()
    {
        isSliding = false;
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
    }
    bool IsGrounded()
    {
        return Physics.Raycast(groundCheck.position, Vector3.down, groundDistance, groundLayer);
    }
    public void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }
    public void HandleMovement()
    {
        Vector3 movementDirection = transform.right * horizontalInput + transform.forward * verticalInput;
        float currentSpeed = speed;

        // Check for running
        isRunning = Input.GetKey(KeyCode.LeftShift);
        if (isRunning)
        {
            currentSpeed *= runMultiplier;
        }

        Vector3 force = movementDirection.normalized * currentSpeed;
        force.y = 0;
        rb.AddForce(force, ForceMode.Force);
    }
    public void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity if needed
        if (flatVel.magnitude > maxSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * maxSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }

       if (isGrounded)
        {
            if (horizontalInput != 0 || verticalInput != 0)
            {
                rb.drag = groundDrag;
            }
            else
            {
                rb.drag = stopDrag;
            }
        }
       else
        {
            rb.drag = 0;
        }
    }
    public void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.G) && !isDashing)
        {
            Vector3 dashDirection = transform.right * horizontalInput + transform.forward * verticalInput;

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
