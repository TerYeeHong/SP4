using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Unit, IKeyInput
{
    //[SerializeField] Transform player_transform;
    [Header("References")]
    [SerializeField] SkinnedMeshRenderer mesh_renderer_player;
    [SerializeField] PlayerRigging player_rigging;
    [SerializeField] Animator animator_player;
    [SerializeField] Rigidbody rigidbody_player;
    [SerializeField] CapsuleCollider collider_player;
    [SerializeField] float speed_player;
    [SerializeField] float jump_force_player;

    [Header("Sfx")]
    [SerializeField] AudioClip walk_sfx;
    [SerializeField] AudioClip sprint_burst_sfx;
    [SerializeField] AudioClip jump_sfx;

    Ability ability_basic = null; //assign through game events
    Ability ability_secondary = null; //assign through game events

    //Vector3 direction; //y not accounted, just horizontal direction

    CONTROL_STATE control_state_current = CONTROL_STATE.IDLE;
    STANCE stance_current = STANCE.MELEE_STANCE;

    public Queue<MovementAxisCommand> movementAxisCommandQueue = new();
    public Queue<AbilityCommand> abilityCommandQueue = new();
    public Queue<MovementButtonCommand> movementButtonCommandQueue = new();

    float FireHoldTimer = 0;

    Material[] character_mesh_materials_list; //to be set in Start(0
    
    enum CONTROL_STATE
    {
        IDLE,
        CROUCH_DELAY,
        CROUCH,
        WATER,
    }
    public enum STANCE
    {
        MELEE_STANCE,
        RANGE_STANCE,
    }

    private void OnEnable()
    {
        GameEvents.m_instance.updatePlayerOnWater.AddListener(OnUpdatePlayerOnWater);
        GameEvents.m_instance.useNewAbility.AddListener(OnUseNewAbility);
        GameEvents.m_instance.playerStance.AddListener(OnPlayerStance);
    }
    private void OnDisable()
    {
        GameEvents.m_instance.updatePlayerOnWater.RemoveListener(OnUpdatePlayerOnWater);
        GameEvents.m_instance.useNewAbility.RemoveListener(OnUseNewAbility);
        GameEvents.m_instance.playerStance.RemoveListener(OnPlayerStance);
    }
    void Start()
    {
        GameEvents.m_instance.playerStance.Invoke(stance_current);
        GameEvents.m_instance.useNewKeyInput.Invoke(this);
    }
    public override void Init()
    {
        base.Init();

        
        character_mesh_materials_list = mesh_renderer_player.materials;
        GetPlayerAbility();
    }

    void GetPlayerAbility()
    {
        AbilityHolder ability_holder = GetComponent<AbilityHolder>();
        List<AbilityPlayer> ability_list = ability_holder.AbilityList;
        int size = ability_list.Count;
        for (int i = 0; i < size; ++i)
        {
            //Make a copy of the scriptable object, we dont want the original asset file to be editted
            Ability ability_clone = Instantiate(ability_list[i].ability);
            ability_clone.Init();
            //Check which one to replace, when replaced, old one is just completely forgotten
            switch (ability_list[i].ability_type)
            {
                case PLAYER_ABILITY.BASIC:
                    ability_basic = ability_clone;
                    break;
                case PLAYER_ABILITY.SECONDARY:
                    ability_secondary = ability_clone;
                    break;
            }
        }

        //Adjust the ability if needed
        //Weird stuff, Although default uses is set to 1 in Ability.cs, it somehows starts at 0 no matter what so i add this here to add 1 to it if its 0
        if (ability_basic.GetUses() == 0)
            ability_basic.AddToUses();
        if (ability_secondary.GetUses() == 0)
            ability_secondary.AddToUses();
    }

    //READ COMMAND FUNCITON OVERRIDING IKeyInput
    public void ReadMovementAxisCommand(MovementAxisCommand moveAxisCommand)
    {
        movementAxisCommandQueue.Enqueue(moveAxisCommand);
    }
    public void ReadAbilityCommand(AbilityCommand abilityCommand)
    {
        abilityCommandQueue.Enqueue(abilityCommand);
    }
    public void ReadMovementButtonCommand(MovementButtonCommand movementButtonCommand)
    {
        movementButtonCommandQueue.Enqueue(movementButtonCommand);
    }

    void OnPlayerStance(STANCE stance)
    {
        stance_current = stance;
        switch (stance_current)
        {
            case STANCE.MELEE_STANCE:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                break;
            case STANCE.RANGE_STANCE:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                //Cursor.lockState = CursorLockMode.Locked;

                break;
        }
    }
    void OnUpdatePlayerOnWater(bool on_water)
    {
        animator_player.SetBool("OnWater", on_water);
        if (on_water)
        {
            animator_player.SetBool("OnGround", true);

            control_state_current = CONTROL_STATE.WATER;
            ControlStateSetCollider(CONTROL_STATE.WATER);
            animator_player.SetTrigger("OnWaterTrigger");

            rigidbody_player.useGravity = false;

            if (stance_current != STANCE.MELEE_STANCE)
                GameEvents.m_instance.playerStance.Invoke(STANCE.MELEE_STANCE);
        }
        else
        {
            rigidbody_player.useGravity = true;

            control_state_current = CONTROL_STATE.IDLE;
            ControlStateSetCollider(CONTROL_STATE.IDLE);
        }
    }



    //Same as Update but called by InputController
    public void UpdateKeyInput()
    {
        HandleKeyPress();
        if (stance_current == STANCE.MELEE_STANCE)
            RotatePlayer();

        //Update animator
        //animator_player.transform.localPosition = new Vector3(0, -0.9f, 0);
        Vector3 horizontal_velocity = rigidbody_player.velocity;
        horizontal_velocity.y = 0; 
        animator_player.SetFloat("MoveSpeed", horizontal_velocity.magnitude); //squared is more efficient
        animator_player.SetFloat("VelocityY", rigidbody_player.velocity.y);
    }

    void RotatePlayer()
    {
        //Rotation in water account for x and y
        if (control_state_current == CONTROL_STATE.WATER)
        {
            //float targetAngleY = Mathf.Atan2(facing_direction.x, facing_direction.z) * Mathf.Rad2Deg;
            //float targetAngleX = Mathf.Atan2(facing_direction.y, facing_direction.z) * Mathf.Rad2Deg;
            ////transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(-targetAngleX, targetAngleY, 0), 6 * Time.deltaTime);
            //Quaternion quaternionDir = new();
            //quaternionDir.eulerAngles = new Vector3(-targetAngleX, targetAngleY, 0);
            //transform.rotation = Quaternion.Slerp(transform.rotation, quaternionDir, 6 * Time.deltaTime);

            if (facing_direction != Vector3.zero)
                transform.LookAt(transform.position + facing_direction);
            //Quaternion.FromToRotation
        }
        //Rotation in land normally only account for y
        else
        {
            float targetAngle = Mathf.Atan2(facing_direction.x, facing_direction.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, targetAngle, 0), 6 * Time.deltaTime);
        }
    }
    void HandleKeyPress()
    {
        //Read command
        MovementButtonCommand move_button_command = null;
        MovementAxisCommand move_axis_command = null;
        AbilityCommand ability_command = null;
        if (movementButtonCommandQueue.Count > 0)
            move_button_command = movementButtonCommandQueue.Dequeue();
        if (movementAxisCommandQueue.Count > 0)
            move_axis_command = movementAxisCommandQueue.Dequeue();
        if (abilityCommandQueue.Count > 0)
            ability_command = abilityCommandQueue.Dequeue();

        //Different set of controls while in Water
        if (control_state_current == CONTROL_STATE.WATER)
        {
            //Move axis
            if (move_axis_command != null)
            {
                float horizontal = move_axis_command.HorizontalAxis;
                float vertical = move_axis_command.VerticalAxis;

                float final_speed = speed_player;
                //Check if fast swimming
                if (move_button_command != null
                    && move_button_command.SprintHold)
                {
                    final_speed *= 2.0f;
                }

                //Check if there is any movement first
                if (vertical != 0 || horizontal != 0)
                {
                    //save the direction with y axis in the water
                    facing_direction = (Camera.main.transform.forward.normalized * vertical + Camera.main.transform.right.normalized * horizontal).normalized;
                }
                facing_direction = (Camera.main.transform.forward.normalized * vertical + Camera.main.transform.right.normalized * horizontal).normalized;
                //No gravity in water
                rigidbody_player.velocity = facing_direction * final_speed + new Vector3(0, 0, 0);
            }
            else
                rigidbody_player.velocity = Vector3.zero;

        }
        //Default control state on land
        else
        {
            //Dont allow key update for jump and move if not grounded
            animator_player.SetBool("OnGround", Grounded());
            if (!Grounded())
                return;

            //Movement_locked is false by default, abilities can set it to true for 1 frame
            if (!movement_locked)
            {
                //detect jump
                if (control_state_current != CONTROL_STATE.WATER
                    && move_button_command != null
                    && move_button_command.JumpDown)
                {
                    GameEvents.m_instance.playNewAudioClip.Invoke(jump_sfx, AudioSfxManager.AUDIO_EFFECT.DEFAULT);

                    //Animator
                    animator_player.SetTrigger("OnJumpTrigger");

                    //Rb
                    rigidbody_player.AddForce(Vector3.up * jump_force_player, ForceMode.Impulse);

                    //remove crouch state if any
                    control_state_current = CONTROL_STATE.IDLE;
                    ControlStateSetCollider(CONTROL_STATE.IDLE);
                }

                //Detect crouch
                if (control_state_current != CONTROL_STATE.WATER
                    && move_button_command != null
                    && move_button_command.CrouchDown)
                {
                    //toggle
                    if (control_state_current == CONTROL_STATE.IDLE)
                    {
                        animator_player.SetTrigger("CrouchTrigger");
                        //animator_player.SetBool("Crouch", true);
                        StartCoroutine(ControlStateChange(CONTROL_STATE.CROUCH, 0.2f));
                        control_state_current = CONTROL_STATE.CROUCH_DELAY;
                        player_rigging.MoveTargetToCrouch(true);
                    }
                    else
                    {
                        StartCoroutine(ControlStateChange(CONTROL_STATE.IDLE, 0.2f));
                        player_rigging.MoveTargetToCrouch(false);

                        //control_state_current = CONTROL_STATE.IDLE;
                    }
                    //temp
                    control_state_current = CONTROL_STATE.CROUCH_DELAY;
                    return;
                }

                //dont take in inputs when crouching delay
                if (control_state_current == CONTROL_STATE.CROUCH_DELAY)
                    return;

                //Move axis
                if (move_axis_command != null)
                {
                    float horizontal = move_axis_command.HorizontalAxis;
                    float vertical = move_axis_command.VerticalAxis;

                    Vector3 camera_dir_forward = Camera.main.transform.forward;
                    Vector3 camera_dir_right = Camera.main.transform.right;
                    camera_dir_forward.y = 0;
                    camera_dir_right.y = 0;
                    camera_dir_forward = camera_dir_forward.normalized;
                    camera_dir_right = camera_dir_right.normalized;

                    float final_speed = speed_player;
                    //Dont allow sprinting in crouch form
                    if (control_state_current != CONTROL_STATE.CROUCH
                        && move_button_command != null
                        && move_button_command.SprintHold)
                    {
                        final_speed *= 2.0f;
                    }

                    //Check if there is any movement first
                    if (vertical != 0 || horizontal != 0)
                    {
                        if (move_button_command != null
                            && move_button_command.SprintDown)
                            GameEvents.m_instance.playNewAudioClip.Invoke(sprint_burst_sfx, AudioSfxManager.AUDIO_EFFECT.DEFAULT);

                        //save the direction (no y axis)
                        facing_direction = (camera_dir_forward * vertical + camera_dir_right * horizontal).normalized;

                        //Save the y velocity
                        rigidbody_player.velocity = facing_direction * final_speed + new Vector3(0, rigidbody_player.velocity.y, 0);
                    }
                }

                //If ads
                if (ability_command != null)
                {
                    if (ability_command.AimHold
                        && stance_current != STANCE.RANGE_STANCE)
                    {
                        //Uncrouch
                        if (control_state_current == CONTROL_STATE.CROUCH
                            || control_state_current == CONTROL_STATE.CROUCH_DELAY)
                        {
                            control_state_current = CONTROL_STATE.IDLE;
                            StartCoroutine(ControlStateChange(CONTROL_STATE.IDLE, 0.22f));
                        }

                        //change stance
                        if (control_state_current == CONTROL_STATE.IDLE)
                            GameEvents.m_instance.playerStance.Invoke(STANCE.RANGE_STANCE);
                    }
                    else if (!ability_command.AimHold
                        && stance_current != STANCE.MELEE_STANCE)
                    {
                        //change stance
                        GameEvents.m_instance.playerStance.Invoke(STANCE.MELEE_STANCE);
                    }
                }
                else if (stance_current != STANCE.MELEE_STANCE)
                {
                    //change stance
                    GameEvents.m_instance.playerStance.Invoke(STANCE.MELEE_STANCE);
                }
            }

            //Handle abilities
            //Casting basic attack
            movement_locked = false; //reset movement_locked, this can be set to true in ability scripts
            if (ability_basic != null)
            {
                bool ability_input = ability_command != null;
                bool FireDown = ability_input && ability_command.FireDown;
                bool FireHold = ability_input && ability_command.FireHold;
                bool FireUp = ability_input && ability_command.FireUp;

                //Update both abilities, Uncrouch if melee ability is activated
                if (ability_basic.UpdateAbility(gameObject, FireUp && stance_current == STANCE.MELEE_STANCE
                    && FireHoldTimer < 0.2f) && (control_state_current == CONTROL_STATE.CROUCH || control_state_current == CONTROL_STATE.CROUCH_DELAY))
                    StartCoroutine(ControlStateChange(CONTROL_STATE.IDLE, 0.22f));
                ability_secondary.UpdateAbility(gameObject, FireUp && stance_current == STANCE.RANGE_STANCE);

                if (FireHold)
                    FireHoldTimer += Time.deltaTime;
                else
                    FireHoldTimer = 0;
            }
        }

    
    }
    //void HandleKeyPress()
    //{
    //    //Handle attacks
    //    //Casting basic attack
    //    if (ability_basic != null)
    //        ability_basic.UpdateAbility(gameObject, Input.GetButtonDown("Fire1"));


    //    //Dont allow key update for jump and move if not grounded
    //    animator_player.SetBool("OnGround", Grounded());
    //    if (!Grounded())
    //        return;

    //    //detect jump
    //    if (control_state_current != CONTROL_STATE.WATER &&
    //        Input.GetButtonDown("Jump"))
    //    {
    //        //Animator
    //        animator_player.SetTrigger("OnJumpTrigger");

    //        //Rb
    //        rigidbody_player.AddForce(Vector3.up * jump_force_player, ForceMode.Impulse);

    //        //remove crouch state if any
    //        control_state_current = CONTROL_STATE.IDLE;
    //        ControlStateSetCollider(CONTROL_STATE.IDLE);
    //    }

    //    //Detect crouch
    //    if (control_state_current != CONTROL_STATE.WATER &&
    //        Input.GetButtonDown("Crouch"))
    //    {
    //        //toggle
    //        if (control_state_current == CONTROL_STATE.IDLE)
    //        {
    //            animator_player.SetTrigger("CrouchTrigger");
    //            //animator_player.SetBool("Crouch", true);
    //            StartCoroutine(ControlStateChange(CONTROL_STATE.CROUCH, 0.2f));
    //            //control_state_current = CONTROL_STATE.CROUCH;
    //        }
    //        else
    //        {
    //            StartCoroutine(ControlStateChange(CONTROL_STATE.IDLE, 0.2f));
    //            //control_state_current = CONTROL_STATE.IDLE;
    //        }
    //        //temp
    //        control_state_current = CONTROL_STATE.CROUCH_DELAY;
    //        return;
    //    }

    //    //dont take in inputs when crouching delay
    //    if (control_state_current == CONTROL_STATE.CROUCH_DELAY)
    //        return;

    //    float horizontal = Input.GetAxisRaw("Horizontal");
    //    float vertical = Input.GetAxisRaw("Vertical");

    //    Vector3 camera_dir_forward = Camera.main.transform.forward;
    //    Vector3 camera_dir_right = Camera.main.transform.right;
    //    camera_dir_forward.y = 0;
    //    camera_dir_right.y = 0;
    //    camera_dir_forward = camera_dir_forward.normalized;
    //    camera_dir_right = camera_dir_right.normalized;

    //    float final_speed = speed_player;
    //    //Dont allow sprinting in crouch form
    //    if (control_state_current != CONTROL_STATE.CROUCH &&
    //        Input.GetButton("Sprint"))
    //    {
    //        final_speed *= 2.0f;
    //    }

    //    //Check if there is any movement first
    //    if (vertical != 0 || horizontal != 0)
    //    {
    //        //save the direction (no y axis)
    //        direction = (camera_dir_forward * vertical + camera_dir_right * horizontal).normalized;

    //        //Save the y velocity
    //        rigidbody_player.velocity = direction * final_speed + new Vector3(0, rigidbody_player.velocity.y, 0);
    //    }
    //}

    bool Grounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, collider_player.bounds.size.y + 0.01f);
    }

    IEnumerator ControlStateChange(CONTROL_STATE new_state, float delay)
    {
        if (new_state == CONTROL_STATE.IDLE)
            player_rigging.MoveTargetToCrouch(false);

        yield return new WaitForSeconds(delay);
        control_state_current = new_state;
        ControlStateSetCollider(new_state);
    }

    void ControlStateSetCollider(CONTROL_STATE state)
    {
        switch (state)
        {
            case CONTROL_STATE.IDLE:
                collider_player.center = new Vector3(0, 0, 0);
                collider_player.height = 2.0f;
                collider_player.radius = 0.5f;
                break;
            case CONTROL_STATE.CROUCH:
                //collider_player.center = new Vector3(0, -0.19f, 0);
                //collider_player.height = 1.6f;
                //collider_player.radius = 0.5f;

                collider_player.center = new Vector3(0, -0.34f, 0);
                collider_player.height = 1.3f;
                collider_player.radius = 0.5f;
                break;
            case CONTROL_STATE.WATER:
                collider_player.center = new Vector3(0, 0, 0);
                collider_player.height = 2.0f;
                collider_player.radius = 1.0f;
                break;
        }
    }

    private void Update()
    {
        if (control_state_current == CONTROL_STATE.CROUCH
            || control_state_current == CONTROL_STATE.CROUCH_DELAY)
            animator_player.SetBool("Crouch", true);
        else
            animator_player.SetBool("Crouch", false);

        
        foreach (Material mat in character_mesh_materials_list)
        {
            //Quaternion.AngleAxis(90, );
            mat.SetVector("_HeadForward", transform.forward);
            mat.SetVector("_HeadRight", transform.right);
        }
    }

    public enum PLAYER_ABILITY
    {
        BASIC = 0,
        SECONDARY,
    }

    void OnUseNewAbility(Ability ability_clone, PLAYER_ABILITY ability_type)
    {
        //Check which one to replace, when replaced, old one is just completely forgotten
        switch (ability_type)
        {
            case PLAYER_ABILITY.BASIC:
                ability_basic = ability_clone;
                break;
            case PLAYER_ABILITY.SECONDARY:
                ability_secondary = ability_clone;
                break;
        }

        //Adjust the ability if needed
        //Weird stuff, Although default uses is set to 1 in Ability.cs, it somehows starts at 0 no matter what so i add this here to add 1 to it if its 0
        if (ability_basic.GetUses() == 0)
            ability_basic.AddToUses();
        if (ability_secondary.GetUses() == 0)
            ability_secondary.AddToUses();

        ////Give the pivot so ability knows where to cast from
        //ability_basic.TransformPivotOffset(new Vector3(0f, pivot_y_offset, 0f));
    }
}
