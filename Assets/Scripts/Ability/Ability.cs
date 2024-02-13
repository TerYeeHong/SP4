using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : ScriptableObject
{
    public enum ABILITY_STATE { 
        READY = 0,
        BEFORE_ACTIVE_DELAY,
        ACTIVE,
        COOLDOWN,
    };

    [Header("Attributes for Default Ability")]
    //Needed for all Abilities
    [SerializeField] protected Sprite image_ability;
    [SerializeField] protected string name_ability;
    [SerializeField] protected float cooldown_ability;
    [SerializeField] protected float delay_ability = 0;
    [SerializeField] protected float activetime_ability;

    [Header("Optional SFX")]
    [SerializeField] protected AudioClip audio_keypress_sfx = null;
    [SerializeField] protected AudioSfxManager.AUDIO_EFFECT audio_keypress_sfx_effect = AudioSfxManager.AUDIO_EFFECT.DEFAULT;
    [SerializeField] protected AudioClip audio_activation_sfx = null;
    [SerializeField] protected AudioSfxManager.AUDIO_EFFECT audio_activation_sfx_effect = AudioSfxManager.AUDIO_EFFECT.DEFAULT;


    [Header("Optional Animation")]
    //[SerializeField] protected AnimationClip animation_keypress = null;
    [SerializeField] protected AbilityAnimationEvent[] animation_keypress_animator_name = null;

    [Header("Optional VFX")]
    [SerializeField] protected string animation_event_name = null;


    //private vars to manage the ability
    protected ABILITY_STATE state = ABILITY_STATE.READY; //Ability state
    protected float cooldown_wait;
    protected float delay_wait;
    protected float activetime_wait;
    protected int uses_amount = 1; //How many uses currently
    protected int max_uses_on_standby = 1; //How many uses can be stacked, if more than 1, 


    //[Header("Attributes for Ability with Charges")]
    ////Addons if an ability can have multiple charges
    //[SerializeField]
    //int max_uses_on_standby = 1; //How many uses can be stacked, if more than 1, 
    //[SerializeField]
    //float chargetime_per_use_ability; //Should be longer than Cooldown else doesnt make sense

    ////Private var to handle ability with charges
    //int uses_amount = 1; //How many uses currently
    //float chargetime_wait;


    /*****************************************************
    * //Functions for All Abilities
    *******************************************************/
    public virtual bool UpdateAbility(GameObject parent, bool activate = false)
    {
        //Check State
        switch (state)
        {
            /**************************************************
            *STATE READY | If ability is ready 
            ***************************************************/
            case ABILITY_STATE.READY:
                //If player input = ability key, Set to active
                if (activate //if player key
                    && GetUses() > 0) //if can use
                {
                    OnFirstDelay(parent);
                    SetState(ABILITY_STATE.BEFORE_ACTIVE_DELAY);
                    MinusToUses();

                    return true;
                }
                break;

            /**************************************************
             *STATE BEFORE_ACTIVE | DELAY BEFORE SKILL IS ACTUALLY CASTED
            ***************************************************/
            case ABILITY_STATE.BEFORE_ACTIVE_DELAY:
                DeductDelay(Time.deltaTime);
                OnActiveDelay(parent);

                //If active time is finish, set state to cooldown
                if (DelayIsOver())
                {  
                    //and if can activate successfully (Calling this will activate it)
                    if (OnActivate(parent))
                    {
                        SetState(ABILITY_STATE.ACTIVE); //Change state to active 
                        //MinusToUses();
                    }
                    //Else say can't cast
                    else
                    {
                        //Debug.LogWarning(GetName() + "was not able to be casted");
                    }

                    ResetDelay();
                }
                //else
                //    //OnActive(parent); //Active ability
                break;

            /**************************************************
             *STATE ACTIVE | If ability is currently active 
            ***************************************************/
            case ABILITY_STATE.ACTIVE:
                DeductActiveTime(Time.deltaTime);

                //If active time is finish, set state to cooldown
                if (ActiveTimeIsOver())
                {
                    //Active is over, change back to ready
                    SetState(ABILITY_STATE.COOLDOWN); //change state to cooldown once its no longer active
                    ResetActiveTime(); //Reset active time
                    OnLastActive(parent); //Update last active
                }
                else
                    OnActive(parent); //Active ability
                break;

            /**************************************************
             *STATE COOLDOWN | If ability is in cooldown, den countdown using delta time
            ***************************************************/
            case ABILITY_STATE.COOLDOWN:
                DeductCooldown(Time.deltaTime);

                //If cooldown is finish, set state to ready
                if (CooldownIsOver())
                {
                    //Cooldown is over
                    SetState(ABILITY_STATE.READY); //change state to ready once cooldown complete
                    ResetCooldown(); //Reset Cooldown

                    //Debug.Log(GetName() + "Ability is ready");

                    //Only add to uses if ability is NOT a charge type (special exception)
                    if (GetMaxUses() == 1) //if Max use = 1, means normal ability
                        AddToUses();
                }
                break;
        };

        return false;
    }

    //When ability is instantiated, each have ability can have their own init when instantiated
    public virtual void Init()
    {

    }


    //Virtual for it to be overriden by ability subtypes, Activate is inital activation of ability, 
    //Activate() is the manual way of activating without checking for key input.
    public virtual bool Activate(GameObject parent)
    {
        //and if can activate successfully (Calling this will activate it)
        if (OnActivate(parent))
        {
            SetState(ABILITY_STATE.ACTIVE); //Change state to active 
            MinusToUses();
            return true;
        }
        return false;
    }
    //Return True if activated successfully, False if not. So as to not use consumption if couldnt activate
    public virtual bool OnActivate(GameObject parent)
    {
        if (audio_activation_sfx != null)
        {
            GameEvents.m_instance.playNewAudioClip.Invoke(audio_activation_sfx, audio_activation_sfx_effect);
        }
        return true;
    }
    //Virtual for it to be overriden by ability subtypes, OnActive is subsequent updates while ability is active
    public virtual void OnActive(GameObject parent)
    {

    }
    //Virtual for it to be overriden by ability subtypes, OnActiveDelay is subsequent updates while ability is in delay
    public virtual void OnActiveDelay(GameObject parent)
    {

    }
    //Virtual for it to be overriden by ability subtypes, OnLastActive is the last update before it goes into cooldown
    public virtual void OnLastActive(GameObject parent)
    {

    }
    //Virtual for it to be overriden by ability subtypes, OnFirstDelay is when ability first goes into BEFORE_ACTIVE_DELAY
    public virtual void OnFirstDelay(GameObject parent) {
        if (audio_keypress_sfx != null)
        {
            GameEvents.m_instance.playNewAudioClip.Invoke(audio_keypress_sfx, audio_keypress_sfx_effect);
        }
        if (parent.TryGetComponent(out CustomAnimator custom_animator))
        {
            foreach (AbilityAnimationEvent anim_trigger in animation_keypress_animator_name)
            {
                if (anim_trigger.anim_name == "")
                    continue;

                switch (anim_trigger.anim_event_type)
                {
                    case AbilityAnimationEvent.ANIMATION_EVENT_TYPE.TRIGGER:
                        custom_animator.SetTrigger(anim_trigger.anim_name);
                        break;
                    case AbilityAnimationEvent.ANIMATION_EVENT_TYPE.EXPRESSION:
                        custom_animator.SetExpression(anim_trigger.anim_name, anim_trigger.expression_duration);
                        break;
                }
            }

            if (animation_event_name != null && animation_event_name != "")
            {
                custom_animator.AnimationEventTrigger(animation_event_name);
            }
            return;
        }
    }

    //Sub Virtual functions
    public virtual void TransformPivotOffset(Vector3 vector3_pivot_offset)
    {

    } // USED FOR SHOOT ABILITIES

    /*****************************************************
    * //Functions for Abilities with charges
    *******************************************************/
    //Accessors 
    public Sprite GetSprite() { return image_ability; }
    public int GetMaxUses()
    {
        return max_uses_on_standby;
    }
    public int GetUses()
    {
        return uses_amount;
    }
    public float GetCooldownTimer() 
    { 
        return cooldown_wait;
    }
    public float GetCooldown() 
    { 
        return cooldown_ability; 
    }
    public float GetDelay()
    {
        return delay_ability;
    }
    public float GetActiveTime()
    {
        return activetime_ability;
    }


    //+/-= 1 to uses_amount
    public void AddToUses()
    {
        //if (uses_amount >= max_uses_on_standby)
        //{
        //    Debug.LogWarning("Ability should already have max charges");

        //}

        uses_amount += 1;
    }
    public void MinusToUses()
    {
        uses_amount -= 1;
    }

    //Get Ability name
    public string GetName()
    {
        return name_ability;
    }

    //Get ability state
    public ABILITY_STATE GetState()
    {
        return state;
    }
    //Set Ability state
    public void SetState(ABILITY_STATE new_state)
    {
        state = new_state;
    }

    //Accessers and Mutators for time_related attributes
    //Deduct time var by the amt of time passed

    public void StartCooldown()
    {
        cooldown_wait = cooldown_ability;
    }
    public void DeductCooldown(float time_passed)
    {
        cooldown_wait -= time_passed;
    }
    public void DeductDelay(float time_passed)
    {
        delay_wait -= time_passed;
    }
    public void DeductActiveTime(float time_passed)
    {
        activetime_wait -= time_passed;
    }
    //Checks if time var is <= 0 seconds
    public bool CooldownIsOver()
    {
        return cooldown_wait <= 0.0f;
    }
    public bool DelayIsOver()
    {
        return delay_wait <= 0.0f;
    }
    public bool ActiveTimeIsOver()
    {
        return activetime_wait <= 0.0f;
    }

    //Must reset after they have been tempered with for the next usage
    public void ResetCooldown()
    {
        cooldown_wait = cooldown_ability;
    }
    public void ResetDelay()
    {
        delay_wait = delay_ability;
    }
    public void ResetActiveTime()
    {
        activetime_wait = activetime_ability;
    }



    //THIS IS SET INITIAL VALUES
    private void Awake()
    {
        ResetActiveTime();
        ResetCooldown();
        ResetDelay();
    }
}

[System.Serializable]
public class AbilityAnimationEvent
{
    public enum ANIMATION_EVENT_TYPE
    {
        TRIGGER,
        EXPRESSION,
    }

    public ANIMATION_EVENT_TYPE anim_event_type;
    public string anim_name;
    public float expression_duration;
}

