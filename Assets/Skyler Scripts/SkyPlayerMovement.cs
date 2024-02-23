using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class SkyPlayerMovement : MonoBehaviour
{
    [Header("References for Status Checks")]
    [SerializeField] Status speed_divine;
    [SerializeField] Status speed_moderate;
    [SerializeField] Status speed_mini;
    [SerializeField] Status jump;
    [SerializeField] Status sprint;
    [SerializeField] Status sliding;
    [SerializeField] Status stamina;

    protected int speed_divine_count;
    protected int speed_moderate_count;
    protected int speed_mini_count;
    protected int jump_count;
    protected int sprint_count;
    protected int sliding_count;
    protected int stamina_count;

    private void OnEnable()
    {
        GameEvents.m_instance.onStatusChange.AddListener(StatusCheckAll);
    }
    private void OnDisable()
    {
        GameEvents.m_instance.onStatusChange.RemoveListener(StatusCheckAll);
    }

    public void StatusCheckAll()
    {
        speed_divine_count = PFGlobalData.GetBlessingCount(speed_divine.Name_status);
        speed_moderate_count = PFGlobalData.GetBlessingCount(speed_moderate.Name_status);
        speed_mini_count = PFGlobalData.GetBlessingCount(speed_mini.Name_status);
        jump_count = PFGlobalData.GetBlessingCount(jump.Name_status);
        sprint_count = PFGlobalData.GetBlessingCount(sprint.Name_status);
        sliding_count = PFGlobalData.GetBlessingCount(sliding.Name_status);
        stamina_count = PFGlobalData.GetBlessingCount(stamina.Name_status);

        //Set stats
        speed = 4.0f 
            + 1.0f * speed_mini_count 
            + 2.0f * speed_moderate_count 
            + 4.0f * speed_divine_count;

        //Sprinting
        staminaDepletionRate = 10.0f + 5.0f * sprint_count;
        runMultiplier = 1.5f + 0.2f * sprint_count;

        //stamina
        maxStamina = 100.0f + stamina_count * 30.0f;
        currentStamina = maxStamina;

        //jump
        jumpStaminaCost = 30.0f;
        maxJumps = jump_count;

        //Stamina is done inside the code


        //Disable and enable stamina bar
        if (jump_count > 0 || sprint_count > 0)
            staminaBar.gameObject.SetActive(true);
        else
            staminaBar.gameObject.SetActive(false);
    }


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

    public float maxStamina = 100f;
    private float currentStamina;
    public float staminaDepletionRate = 10f; // Per second, while running
    public float jumpStaminaCost = 20f; // Per jump
    public float staminaRegenerationRate = 5f; // Per second
    public UnityEngine.UI.Image staminaBar; 
    public Slider staminaSlider;

    [SerializeField] AudioClip slidingSFX;
    [SerializeField] AudioClip walkSFX;
    [SerializeField] AudioClip runningSFX;
    [SerializeField] AudioClip jumpSFX;
    [SerializeField] AudioClip landingSFX;
    [SerializeField] AudioClip dashSFX;
    float runAudioTimer;
    float runAudioCooldown = 0.3f;
    float walkAudioTimer;
    float walkAudioCooldown = 0.7f;
    float walkingTimer;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentStamina = maxStamina;

        StatusCheckAll();
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
                GameEvents.m_instance.playNewAudioClip.Invoke(slidingSFX, AudioSfxManager.AUDIO_EFFECT.DEFAULT);

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
        if (sliding_count == 0)
            return;

        isSliding = true;
        transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse); // Optional, for enhanced sliding effect
        slideTimer = maxSlideTime;

        currentStamina += (3 + 2.0f * sliding_count) * Time.deltaTime;
        UpdateStaminaBar();
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
        bool typing = FindObjectOfType<MultiplayerMessages>().typing;
        if (!typing)
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            verticalInput = Input.GetAxisRaw("Vertical");
        }
    }
    public void HandleMovement()
    {
        bool typing = FindObjectOfType<MultiplayerMessages>().typing;
        if (!typing)
        {

            Vector3 movementDirection = transform.right * horizontalInput + transform.forward * verticalInput;
            float currentSpeed = speed;

            // Check for running
            if (sprint_count > 0)
            {
                isRunning = Input.GetKey(KeyCode.LeftShift);
                if (isRunning && currentStamina > 0)
                {
                    currentSpeed *= runMultiplier;
                    currentStamina -= staminaDepletionRate * Time.deltaTime;
                    UpdateStaminaBar();
                }
            }


            Vector3 force = movementDirection.normalized * currentSpeed;
            force.y = 0;
            rb.velocity = new Vector3(force.x, rb.velocity.y, force.z);
        }
    }

    public void HandleStaminaRegen()
    {
        if (!isRunning && currentStamina < maxStamina)
        {
            currentStamina += staminaRegenerationRate * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, maxStamina);
            UpdateStaminaBar();
        }
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
            if ((horizontalInput < -0.1 || horizontalInput > 0.1)  || (verticalInput < -0.1 || verticalInput > 0.1))
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
                GameEvents.m_instance.playNewAudioClip.Invoke(dashSFX, AudioSfxManager.AUDIO_EFFECT.DEFAULT);
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
        if (Input.GetButtonDown("Jump") && (jumpCount < maxJumps) && currentStamina >= jumpStaminaCost)
        {
            stopDrag = 0;
            groundDrag = 0;
             

            GameEvents.m_instance.playNewAudioClip.Invoke(jumpSFX, AudioSfxManager.AUDIO_EFFECT.DEFAULT);
            currentStamina -= jumpStaminaCost;
            UpdateStaminaBar();
            //rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
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
                    GameEvents.m_instance.playNewAudioClip.Invoke(landingSFX, AudioSfxManager.AUDIO_EFFECT.DEFAULT);
                    jumpCount = 0;
                }
                else if (rb.velocity.magnitude > 0.1f && (Mathf.Abs(rb.velocity.x) > 0.1f || Mathf.Abs(rb.velocity.z) > 0.1f))
                {
                    if (isRunning)
                    {
                        // Transition to running animation if moving and Left Control is pressed
                        playerAnimation.ChangeAnimationState(SkyPlayerAnimation.PLAYER_RUN);
                        if (runAudioTimer <= 0f)
                        {
                            // Play running audio
                            GameEvents.m_instance.playNewAudioClip.Invoke(runningSFX, AudioSfxManager.AUDIO_EFFECT.DEFAULT);
                            runAudioTimer = runAudioCooldown; // Reset timer
                        }
                        else
                        {
                            runAudioTimer -= Time.deltaTime;
                        }
                    }
                    else
                    {
                        // Walking animation
                        playerAnimation.ChangeAnimationState(SkyPlayerAnimation.PLAYER_WALK);
                        if (walkAudioTimer <= 0f)
                        {
                            // Play running audio
                            GameEvents.m_instance.playNewAudioClip.Invoke(runningSFX, AudioSfxManager.AUDIO_EFFECT.DEFAULT);
                            walkAudioTimer = walkAudioCooldown; // Reset timer
                        }
                        else
                        {
                            walkAudioTimer -= Time.deltaTime;
                        }
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
                    GetComponent<SkyPlayerController>().playerHealth.CheckForOffMap();
                }
            }
        }
    }

    public void UpdateStaminaBar()
    {
        if (staminaBar != null)
        {
            staminaSlider.value = currentStamina / maxStamina;

            // Change color to red when using stamina
            if (isRunning || currentStamina < maxStamina)
            {
                staminaBar.color = Color.red;
            }
            // Change color to yellow when stamina is full or regenerating
            if (!isRunning && currentStamina == maxStamina)
            {
                staminaBar.color = Color.yellow;
            }
            else if (!isRunning && currentStamina > maxStamina * 0.5f) // Adjust threshold as needed
            {
                // Blend between red and yellow based on stamina level
                float lerpFactor = (currentStamina - maxStamina * 0.5f) / (maxStamina * 0.5f);
                staminaBar.color = Color.Lerp(Color.red, Color.yellow, lerpFactor);
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
