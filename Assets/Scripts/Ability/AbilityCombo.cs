using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Ability/Ability Combo")]
public class AbilityCombo : Ability
{
    [Header("AbilityCombo Details")]
    [SerializeField] List<Combo> ability_combo_list;
    [SerializeField] float combo_stop_duration;

    [SerializeField] protected string animation_keypress_combo_integer_name;

    int combo_counter = 0;
    float combo_stop_timer;
    List<Ability> ability_list;

    CustomAnimator custom_animator_parent = null;

    public override void Init()
    {
        ability_list = new();

        int size = ability_combo_list.Count;
        for (int i = 0; i < size; ++i){
            //Init all the abilities, make clones, dont use original asset file
            Ability ability_clone = Instantiate(ability_combo_list[i].ability);
            ability_list.Add(ability_clone);
        }

        //no cd at the start
        cooldown_wait = 0;
        delay_wait = 0;

    }

    public override bool OnActivate(GameObject parent)
    {
        return base.OnActivate(parent);
    }

    public override void OnFirstDelay(GameObject parent)
    {
        base.OnFirstDelay(parent);
    }

    public override void OnActive(GameObject parent)
    {
        base.OnActive(parent);
    }

    public override void OnLastActive(GameObject parent)
    {
        base.OnLastActive(parent);
    }



    //HAS TO OVERRIDE DEFAULT UPDATEABILITY
    public override bool UpdateAbility(GameObject parent, bool activate = false)
    {
        //different implementations
        //1. Ignore uses amount
        //2. Should keep checking for Input in order to perform the next combo
        //3. Start cooldown when (1.combo ends OR 2.wait for next combo never came)

        //Debug.Log("Delay " + delay_wait);
        //Debug.Log("CD >" + cooldown_wait);

        //Check if in cooldown
        if (!CooldownIsOver())
        {
            DeductCooldown(Time.deltaTime);
            return false; //not activated
        }

        //Handle timer first,
        if (DelayIsOver())
        {
            //check current combo //==0 means no attack active, do not update any ability
            if (combo_counter != 0)
            {
                //NOW UPDATE THE ABILITY
                //OVERRIDEN VERSION
                UpdateComboAbility(parent);

                //Check if combo is broken already, UNless its already the last combo
                if (combo_counter < ability_combo_list.Count &&
                    combo_stop_timer <= 0)
                {
                    //Go into cooldown and reset combo
                    ComboBreak();
                }
                //If not broken yet, start counting down
                else
                {
                    combo_stop_timer -= Time.deltaTime;
                }
            }

            //If can take in activation input again, handle it 
            if (activate
                && combo_counter < ability_combo_list.Count) //Last already, let it finish on its own
            {
                //Set animator if need to
                if (custom_animator_parent == null
                    && parent.TryGetComponent(out CustomAnimator custom_animator))
                {
                    custom_animator_parent = custom_animator;
                }

                //Activate the current ability if it hasnt been activated yet
                if (combo_counter > 0)
                {
                    Ability ability_current = ability_list[combo_counter - 1];
                    if (ability_current.GetState() == ABILITY_STATE.BEFORE_ACTIVE_DELAY)
                    {
                        ability_current.OnActivate(parent);
                    }
                }
                NextCombo();
            }
        }
        //Delay before the next
        else
        {
            DeductDelay(Time.deltaTime);
        }

        //END
        if (activate)
            return true;
        return false;
    }

    void ComboBreak()
    {
        combo_counter = 0;
        ResetCooldown();
        delay_wait = 0;
    }
    void NextCombo()
    {
        //start the next stop
        combo_stop_timer = combo_stop_duration;

        //Add to combo counter
        ++combo_counter;
        if (combo_counter > ability_list.Count)
        {
            ComboBreak();
        }

        //Make new ability fresh (this is for the combo)
        if (combo_counter > 0)
        {
            Ability ability_next = ability_list[combo_counter - 1];
            ability_next.ResetDelay();
            ability_next.ResetActiveTime();
            ability_next.ResetCooldown();
            ability_next.SetState(ABILITY_STATE.READY);

            //Start delay to stop inputs
            delay_wait = ability_combo_list[combo_counter - 1].delay;
        }

        //Update animation
        if (custom_animator_parent != null)
        {
            custom_animator_parent.SetInteger(animation_keypress_combo_integer_name, combo_counter - 1);
            //Debug.LogWarning("Combo played is " + (combo_counter - 1).ToString());
        }

    }

    void UpdateComboAbility(GameObject parent)
    {
        Ability ability_current = ability_list[combo_counter - 1];
        switch (ability_current.GetState())
        {
            /**************************************************
            *STATE READY | If ability is ready 
            ***************************************************/
            case ABILITY_STATE.READY:
                //If player input = ability key, Set to active
                ability_current.OnFirstDelay(parent);
                ability_current.SetState(ABILITY_STATE.BEFORE_ACTIVE_DELAY);
                break;

            /**************************************************
             *STATE BEFORE_ACTIVE | DELAY BEFORE SKILL IS ACTUALLY CASTED
            ***************************************************/
            case ABILITY_STATE.BEFORE_ACTIVE_DELAY:
                ability_current.DeductDelay(Time.deltaTime);
                ability_current.OnActiveDelay(parent);

                //If active time is finish, set state to cooldown
                if (ability_current.DelayIsOver())
                {
                    //and if can activate successfully (Calling this will activate it)
                    if (ability_current.OnActivate(parent))
                    {
                        ability_current.SetState(ABILITY_STATE.ACTIVE); //Change state to active                         //MinusToUses();
                    }
                }
                //else
                //    //OnActive(parent); //Active ability
                break;

            /**************************************************
             *STATE ACTIVE | If ability is currently active 
            ***************************************************/
            case ABILITY_STATE.ACTIVE:
                ability_current.DeductActiveTime(Time.deltaTime);

                //If active time is finish, set state to cooldown
                if (ability_current.ActiveTimeIsOver())
                {
                    //Active is over, change back to ready
                    ability_current.SetState(ABILITY_STATE.COOLDOWN); //change state to cooldown once its no longer active
                    ability_current.OnLastActive(parent); //Update last active
                }
                else
                    ability_current.OnActive(parent); //Active ability
                break;

            /**************************************************
             *STATE COOLDOWN | If ability is in cooldown, den countdown using delta time
            ***************************************************/
            case ABILITY_STATE.COOLDOWN:
                //If its the last combo, call combo break when in cooldown
                if (combo_counter >= ability_combo_list.Count)
                {
                    ComboBreak();
                }
                break;
        }

    }
}


[System.Serializable]
public class Combo
{
    public Ability ability;
    public float delay;
}